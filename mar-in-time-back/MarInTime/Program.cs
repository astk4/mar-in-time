using MarInTime.Application.Repositories;
using MarInTime.Application.Services;
using MarInTime.Infrastructure.Persistence;
using MarInTime.Infrastructure.Repositories;
using MarInTime.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO.Converters;
using System.Text.Json.Serialization;
using StackExchange.Redis;
using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;
using MarInTime.Presentation;
using Microsoft.Extensions.Logging.Console;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore.Storage;

namespace MarInTime
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("secrets.json", true);

            bool inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            string confKey = inContainer ? "Docker" : "Main";

            builder.Services.AddDbContext<MainDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration["Data:"+confKey],
                                  npgsql => npgsql.UseNetTopologySuite());
            });

            builder.Services.AddTransient<IPortRepository, PortRepository>();

            builder.Services.AddTransient<ILandChecker, LandChecker>();

            builder.Services.AddTransient<IGisStreamRepository, EconomicZoneRepository>();
            builder.Services.AddTransient<ISpatialRepository<ExclusiveEconomicZone>, EconomicZoneRepository>();
            builder.Services.AddScoped<IGisService<EconomicZoneDto>, EezService>();
            builder.Services.AddHostedService<AisBufferingBackgroundService>();

            string redisHost = builder.Configuration["Redis:Host:"+confKey]!,
                   redisPort = builder.Configuration["Redis:Port:"+confKey]!;
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}");
            builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

            builder.Services.AddMemoryCache();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            builder.Services.AddControllers()
                            .AddJsonOptions(options =>
                            {
                                options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
                                options.JsonSerializerOptions.Converters.Insert(0, new GeoJsonConverterFactory());
                            });

            string corsConfKey = "CORS_Settings:" + confKey;
            string[] allowedHosts = builder.Configuration.GetSection(corsConfKey+":AllowedHosts")!.Get<string[]>()!,
                     allowedMethods = builder.Configuration.GetSection(corsConfKey+":AllowedMethods")!.Get<string[]>()!;

            int[] allowedPorts = builder.Configuration.GetSection(corsConfKey+":AllowedPorts")!.Get<int[]>()!;

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MainFrontendPolicy", builder =>
                {
                    builder.SetIsOriginAllowed(origin =>
                            {
                                Uri originUri = new Uri(origin);
                                return allowedHosts.Contains(originUri.Host) && allowedPorts.Contains(originUri.Port);
                            })
                           .WithMethods(allowedMethods)
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            builder.Services.AddGrpc();
            builder.Services.AddSignalR()
                            .AddMessagePackProtocol();

            builder.Logging.AddSimpleConsole(opt => opt.ColorBehavior = LoggerColorBehavior.Enabled);

            if (inContainer)
            {
                builder.WebHost.ConfigureKestrel((options) =>
                {
                    options.ListenAnyIP(8080, opt1 => opt1.Protocols = HttpProtocols.Http1);
                    options.ListenAnyIP(8081, opt2 => opt2.Protocols = HttpProtocols.Http2);
                });
            }

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("MainFrontendPolicy");

            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<AisReceiverService>();
            app.MapHub<AisResultHub>("/hubs/ais");

            if (inContainer) { app.Logger.LogInformation("Containerization detected"); }
            
            using (IServiceScope scope = app.Services.CreateScope())
            {
                MainDbContext dbc = scope.ServiceProvider.GetRequiredService<MainDbContext>();
                dbc.Database.Migrate();

                ApplyMigrationsDirectory(dbc.Database, "Infrastructure/Persistence/CustomSchemaMigrations/");
                ApplyMigrationsDirectory(dbc.Database, "Infrastructure/Persistence/DataMigrations/");
            }

            app.Run();
        }

        private static void ApplyMigrationsDirectory(DatabaseFacade db, string pathToDir)
        {
            string[] migFiles = Directory.GetFiles(pathToDir).Order().ToArray();
            foreach (string filePath in migFiles)
            {
                using (IDbContextTransaction transaction = db.BeginTransaction())
                {
                    string script = File.ReadAllText(filePath);
                    db.ExecuteSqlRaw(script);
                    transaction.Commit();
                }
            }
        }
    }
}

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

namespace MarInTime
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<MainDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration["Data:Main"],
                                  npgsql => npgsql.UseNetTopologySuite());
            });

            builder.Services.AddTransient<IPortRepository, PortRepository>();

            builder.Services.AddTransient<ILandChecker, LandChecker>();

            builder.Services.AddTransient<IGisStreamRepository, EconomicZoneRepository>();
            builder.Services.AddTransient<ISpatialRepository<ExclusiveEconomicZone>, EconomicZoneRepository>();
            builder.Services.AddScoped<IGisService<EconomicZoneDto>, EezService>();

            string redisHost = builder.Configuration["Redis:Host"]!,
                   redisPort = builder.Configuration["Redis:Port"]!;
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

            string[] allowedHosts = builder.Configuration.GetSection("CORS_Settings:AllowedHosts")!.Get<string[]>()!,
                     allowedMethods = builder.Configuration.GetSection("CORS_Settings:AllowedMethods")!.Get<string[]>()!;

            int[] allowedPorts = builder.Configuration.GetSection("CORS_Settings:AllowedPorts")!.Get<int[]>()!;

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

            app.Run();
        }
    }
}

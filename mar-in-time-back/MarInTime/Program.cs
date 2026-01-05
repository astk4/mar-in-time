using MarInTime.Application.Repositories;
using MarInTime.Application.Services;
using MarInTime.Infrastructure.Persistence;
using MarInTime.Infrastructure.Repositories;
using MarInTime.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO.Converters;
using System.Text.Json.Serialization;

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
            builder.Services.AddTransient<IGisRepository, EconomicZoneRepository>();
            builder.Services.AddScoped<IEezService, EezService>();

            builder.Services.AddControllers()
                            .AddJsonOptions(options =>
                            {
                                options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
                                options.JsonSerializerOptions.Converters.Insert(0, new GeoJsonConverterFactory());
                            });

            string[] allowedHosts = builder.Configuration.GetSection("CORS_Settings:AllowedHosts")!.Get<string[]>()!,
                     allowedMethods = builder.Configuration.GetSection("CORS_Settings:AllowedMethods")!.Get<string[]>()!;

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MainFrontendPolicy", builder =>
                {
                    builder.SetIsOriginAllowed(origin => allowedHosts.Contains(new Uri(origin).Host))
                           .WithMethods(allowedMethods)
                           .AllowAnyHeader();
                });
            });

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
            app.UseAuthorization();

            app.MapControllers();

            // startup logic
            using (IServiceScope scope = app.Services.CreateScope())
            {
                var gisServices = scope.ServiceProvider.GetServices<IEezService>();
                if (gisServices != null)
                {
                    foreach (IEezService gzs in gisServices)
                    {
                        gzs.InitZoomTableNames();
                    }
                }
            }

            app.Run();
        }
    }
}

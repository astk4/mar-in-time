using MarInTime.Application.Repositories;
using MarInTime.Infrastructure.Persistence;
using MarInTime.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MarInTime
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<MainDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration["Data:Main"]);
            });

            builder.Services.AddTransient<IPortRepository, PortRepository>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

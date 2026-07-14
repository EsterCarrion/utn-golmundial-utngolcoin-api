using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UTNGolMundial.UTNGolCoin.Api.Data;
using UTNGolMundial.UTNGolCoin.Api.Services;

namespace UTNGolMundial.UTNGolCoin.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Cadena de conexión MariaDB/MySQL
            var connectionString = builder.Configuration.GetConnectionString("GolCoinConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'GolCoinConnection'.");

            // DbContext
            builder.Services.AddDbContext<GolCoinDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                )
            );

            // Servicios de negocio
            builder.Services.AddScoped<TransaccionService>();
            builder.Services.AddScoped<BilleteraService>();
            builder.Services.AddScoped<PrediccionService>();
            builder.Services.AddScoped<LiquidacionService>();
            builder.Services.AddScoped<BonoDiarioService>();

            // Controllers API
            builder.Services.AddControllers();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Swagger solo en desarrollo
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Configuración para producción
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Rutas API
            app.MapControllers();

            app.Run();
        }
    }
}
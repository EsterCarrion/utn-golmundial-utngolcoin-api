using Microsoft.EntityFrameworkCore;
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

            builder.Services.AddDbContext<GolCoinDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                )
            );

            // Servicios de negocio
            builder.Services.AddScoped<TransaccionService>();
            builder.Services.AddScoped<BilleteraService>();

            //Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            //Swagger solo en desarrollo
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //Manejo de errores en producción
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();

            //Rutas  API
            app.MapControllers();

            //Ruta MVC
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            
            app.Run();
        }
    }
}

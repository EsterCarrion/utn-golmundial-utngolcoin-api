using Microsoft.EntityFrameworkCore;
using UTNGolMundial.UTNGolCoin.Api.Models;

namespace UTNGolMundial.UTNGolCoin.Api.Data
{
    public class GolCoinDbContext : DbContext
    {
        public GolCoinDbContext(DbContextOptions<GolCoinDbContext> options)
            : base(options)
        {
        }

        public DbSet<Billetera> Billeteras { get; set; } = default!;
        public DbSet<Transaccion> Transacciones { get; set; } = default!;
        public DbSet<Prediccion> Predicciones { get; set; } = default!;
        public DbSet<BonoDiario> BonosDiarios { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Billeteras
            modelBuilder.Entity<Billetera>(entity =>
            {
                entity.ToTable("billeteras");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Saldo)
                    .HasPrecision(18, 2);

                entity.Property(e => e.FechaCreacion)
                    .IsRequired();

                entity.Property(e => e.Activa)
                    .IsRequired();

                entity.HasIndex(e => e.UsuarioId)
                    .IsUnique();
            });

            // Transacciones
            modelBuilder.Entity<Transaccion>(entity =>
            {
                entity.ToTable("transacciones");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Tipo)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Monto)
                    .HasPrecision(18, 2);

                entity.Property(e => e.SaldoResultante)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(250);

                entity.Property(e => e.Fecha)
                    .IsRequired();

                entity.HasOne(e => e.Billetera)
                    .WithMany(e => e.Transacciones)
                    .HasForeignKey(e => e.BilleteraId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Prediccion)
                    .WithMany(e => e.Transacciones)
                    .HasForeignKey(e => e.PrediccionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Predicciones
            modelBuilder.Entity<Prediccion>(entity =>
            {
                entity.ToTable("predicciones");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.ResultadoPronosticado)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.MontoApostado)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Cuota)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Estado)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.ResultadoOficial)
                    .HasMaxLength(20);

                entity.Property(e => e.PremioPagado)
                    .HasPrecision(18, 2);

                entity.Property(e => e.FechaCreacion)
                    .IsRequired();

                entity.HasIndex(e => new { e.UsuarioId, e.PartidoId })
                    .IsUnique();
            });

            // Bonos diarios
            modelBuilder.Entity<BonoDiario>(entity =>
            {
                entity.ToTable("bonos_diarios");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.FechaBono)
                    .IsRequired();

                entity.Property(e => e.Monto)
                    .HasPrecision(18, 2);

                entity.Property(e => e.FechaRegistro)
                    .IsRequired();

                entity.HasIndex(e => new { e.UsuarioId, e.FechaBono })
                    .IsUnique();

                entity.HasOne(e => e.Transaccion)
                    .WithMany()
                    .HasForeignKey(e => e.TransaccionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
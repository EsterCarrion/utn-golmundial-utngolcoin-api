using Microsoft.EntityFrameworkCore;
using UTNGolMundial.UTNGolCoin.Api.Config;
using UTNGolMundial.UTNGolCoin.Api.Data;
using UTNGolMundial.UTNGolCoin.Api.Models;

namespace UTNGolMundial.UTNGolCoin.Api.Services
{
    public class BilleteraService
    {
        private readonly GolCoinDbContext _context;
        private readonly TransaccionService _transaccionService;

        public BilleteraService(
            GolCoinDbContext context,
            TransaccionService transaccionService)
        {
            _context = context;
            _transaccionService = transaccionService;
        }

        public async Task<Billetera> CrearBilleteraAsync(int usuarioId)
        {
            var billeteraExistente = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            if (billeteraExistente != null)
            {
                return billeteraExistente;
            }

            var billetera = new Billetera
            {
                UsuarioId = usuarioId,
                Saldo = GolCoinConfig.BonoBienvenida,
                FechaCreacion = DateTime.UtcNow,
                Activa = true
            };

            _context.Billeteras.Add(billetera);
            await _context.SaveChangesAsync();

            await _transaccionService.RegistrarTransaccionAsync(
                billetera.Id,
                usuarioId,
                GolCoinConfig.TipoBonoBienvenida,
                GolCoinConfig.BonoBienvenida,
                billetera.Saldo,
                "Bono de bienvenida por registro de usuario"
            );

            return billetera;
        }

        public async Task<decimal> ConsultarSaldoAsync(int usuarioId)
        {
            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            if (billetera == null)
            {
                throw new InvalidOperationException("El usuario no tiene billetera registrada.");
            }

            return billetera.Saldo;
        }

        public async Task<decimal> SumarSaldoAsync(
            int usuarioId,
            decimal monto,
            string tipoTransaccion,
            string? descripcion = null,
            int? prediccionId = null)
        {
            if (monto <= 0)
            {
                throw new ArgumentException("El monto a sumar debe ser mayor a cero.");
            }

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            if (billetera == null)
            {
                throw new InvalidOperationException("El usuario no tiene billetera registrada.");
            }

            billetera.Saldo += monto;
            billetera.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _transaccionService.RegistrarTransaccionAsync(
                billetera.Id,
                usuarioId,
                tipoTransaccion,
                monto,
                billetera.Saldo,
                descripcion,
                prediccionId
            );

            return billetera.Saldo;
        }

        public async Task<decimal> DescontarSaldoAsync(
            int usuarioId,
            decimal monto,
            string tipoTransaccion,
            string? descripcion = null,
            int? prediccionId = null)
        {
            if (monto <= 0)
            {
                throw new ArgumentException("El monto a descontar debe ser mayor a cero.");
            }

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            if (billetera == null)
            {
                throw new InvalidOperationException("El usuario no tiene billetera registrada.");
            }

            if (billetera.Saldo < monto)
            {
                throw new InvalidOperationException("Saldo insuficiente.");
            }

            billetera.Saldo -= monto;
            billetera.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _transaccionService.RegistrarTransaccionAsync(
                billetera.Id,
                usuarioId,
                tipoTransaccion,
                -monto,
                billetera.Saldo,
                descripcion,
                prediccionId
            );

            return billetera.Saldo;
        }

        public async Task<bool> TieneSaldoSuficienteAsync(int usuarioId, decimal monto)
        {
            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            return billetera != null && billetera.Saldo >= monto;
        }
    }
}
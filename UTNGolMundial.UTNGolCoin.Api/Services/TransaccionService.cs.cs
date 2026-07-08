using Microsoft.EntityFrameworkCore;
using UTNGolMundial.UTNGolCoin.Api.Data;
using UTNGolMundial.UTNGolCoin.Api.Models;

namespace UTNGolMundial.UTNGolCoin.Api.Services
{
    public class TransaccionService
    {
        private readonly GolCoinDbContext _context;

        public TransaccionService(GolCoinDbContext context)
        {
            _context = context;
        }

        public async Task<Transaccion> RegistrarTransaccionAsync(
            int billeteraId,
            int usuarioId,
            string tipo,
            decimal monto,
            decimal saldoResultante,
            string? descripcion = null,
            int? prediccionId = null)
        {
            var transaccion = new Transaccion
            {
                BilleteraId = billeteraId,
                UsuarioId = usuarioId,
                Tipo = tipo,
                Monto = monto,
                SaldoResultante = saldoResultante,
                Descripcion = descripcion,
                PrediccionId = prediccionId,
                Fecha = DateTime.UtcNow
            };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            return transaccion;
        }

        public async Task<List<Transaccion>> ObtenerHistorialPorUsuarioAsync(int usuarioId)
        {
            return await _context.Transacciones
                .Where(t => t.UsuarioId == usuarioId)
                .OrderByDescending(t => t.Fecha)
                .ToListAsync();
        }
    }
}
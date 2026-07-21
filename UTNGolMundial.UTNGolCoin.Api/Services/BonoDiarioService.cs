using Microsoft.EntityFrameworkCore;
using UTNGolMundial.UTNGolCoin.Api.Config;
using UTNGolMundial.UTNGolCoin.Api.Data;
using UTNGolMundial.UTNGolCoin.Api.Models;

namespace UTNGolMundial.UTNGolCoin.Api.Services
{
    public class BonoDiarioService
    {
        private readonly GolCoinDbContext _context;
        private readonly TransaccionService _transaccionService;

        public BonoDiarioService(
            GolCoinDbContext context,
            TransaccionService transaccionService)
        {
            _context = context;
            _transaccionService = transaccionService;
        }


        //Método para otorgar un bono masivo a todos los usuarios con saldo cero
        public async Task<int> OtorgarBonoMasivoAdminAsync()
        {
            const decimal montoBono = 1m;

            var billeterasSinSaldo = await _context.Billeteras
                .Where(b => b.Saldo == 0)
                .ToListAsync();

            foreach (var billetera in billeterasSinSaldo)
            {
                billetera.Saldo += montoBono;

                var transaccion = new Transaccion
                {
                    UsuarioId = billetera.UsuarioId,
                    BilleteraId = billetera.Id,
                    Tipo = "BONO_ADMIN",
                    Monto = montoBono,
                    SaldoResultante = billetera.Saldo,
                    Descripcion = "Bono de una moneda otorgado por el administrador",
                    Fecha = DateTime.UtcNow
                };

                _context.Transacciones.Add(transaccion);
            }

            await _context.SaveChangesAsync();

            return billeterasSinSaldo.Count;
        }



        public async Task<object> OtorgarBonoDiarioAsync(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                throw new ArgumentException("El usuarioId debe ser mayor a 0.");
            }

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            if (billetera == null)
            {
                throw new InvalidOperationException("El usuario no tiene billetera registrada.");
            }

            if (!billetera.Activa)
            {
                throw new InvalidOperationException("La billetera del usuario no está activa.");
            }

            // El bono diario solo aplica cuando el saldo es exactamente 0.
            if (billetera.Saldo != 0)
            {
                return new
                {
                    mensaje = "El usuario no aplica al bono diario porque su saldo no es 0.",
                    usuarioId,
                    saldoActual = billetera.Saldo,
                    bonoOtorgado = false
                };
            }

            var fechaHoy = DateTime.UtcNow.Date;

            var yaRecibioBonoHoy = await _context.BonosDiarios
                .AnyAsync(b => b.UsuarioId == usuarioId && b.FechaBono == fechaHoy);

            if (yaRecibioBonoHoy)
            {
                return new
                {
                    mensaje = "El usuario ya recibió el bono diario el día de hoy.",
                    usuarioId,
                    fechaBono = fechaHoy,
                    bonoOtorgado = false
                };
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var montoBono = GolCoinConfig.BonoDiarioSaldoCero;

                billetera.Saldo += montoBono;
                billetera.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var transaccion = await _transaccionService.RegistrarTransaccionAsync(
                    billetera.Id,
                    usuarioId,
                    GolCoinConfig.TipoBonoDiario,
                    montoBono,
                    billetera.Saldo,
                    "Bono diario otorgado por saldo cero"
                );

                var bonoDiario = new BonoDiario
                {
                    UsuarioId = usuarioId,
                    FechaBono = fechaHoy,
                    Monto = montoBono,
                    TransaccionId = transaccion.Id,
                    FechaRegistro = DateTime.UtcNow
                };

                _context.BonosDiarios.Add(bonoDiario);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new
                {
                    mensaje = "Bono diario otorgado correctamente.",
                    usuarioId,
                    montoBono,
                    saldoActual = billetera.Saldo,
                    fechaBono = fechaHoy,
                    transaccionId = transaccion.Id,
                    bonoOtorgado = true
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
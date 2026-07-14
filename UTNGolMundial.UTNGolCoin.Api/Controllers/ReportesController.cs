using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UTNGolMundial.UTNGolCoin.Api.Config;
using UTNGolMundial.UTNGolCoin.Api.Data;

namespace UTNGolMundial.UTNGolCoin.Api.Controllers
{
    [Route("api/reportes")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly GolCoinDbContext _context;

        public ReportesController(GolCoinDbContext context)
        {
            _context = context;
        }

        // GET: api/reportes/monedas-circulacion
        // Reporte general de monedas disponibles actualmente en las billeteras.
        [HttpGet("monedas-circulacion")]
        public async Task<IActionResult> ObtenerMonedasEnCirculacion()
        {
            var totalBilleteras = await _context.Billeteras.CountAsync();

            var totalBilleterasActivas = await _context.Billeteras
                .CountAsync(b => b.Activa);

            var monedasEnCirculacion = await _context.Billeteras
                .Where(b => b.Activa)
                .Select(b => (decimal?)b.Saldo)
                .SumAsync() ?? 0;

            var totalBonosBienvenida = await _context.Transacciones
                .Where(t => t.Tipo == GolCoinConfig.TipoBonoBienvenida)
                .Select(t => (decimal?)t.Monto)
                .SumAsync() ?? 0;

            var totalBonosDiarios = await _context.Transacciones
                .Where(t => t.Tipo == GolCoinConfig.TipoBonoDiario)
                .Select(t => (decimal?)t.Monto)
                .SumAsync() ?? 0;

            var totalPremiosPagados = await _context.Transacciones
                .Where(t => t.Tipo == GolCoinConfig.TipoPremio)
                .Select(t => (decimal?)t.Monto)
                .SumAsync() ?? 0;

            var totalApostado = await _context.Transacciones
                .Where(t => t.Tipo == GolCoinConfig.TipoPrediccion)
                .Select(t => (decimal?)t.Monto)
                .SumAsync() ?? 0;

            return Ok(new
            {
                moneda = GolCoinConfig.NombreMoneda,
                simbolo = GolCoinConfig.SimboloMoneda,
                totalBilleteras,
                totalBilleterasActivas,
                monedasEnCirculacion,
                totalBonosBienvenida,
                totalBonosDiarios,
                totalPremiosPagados,

                // Las apuestas se guardan como monto negativo, por eso se muestra también en positivo.
                totalApostado = Math.Abs(totalApostado),

                fechaReporte = DateTime.UtcNow
            });
        }

        // GET: api/reportes/partidos-mas-predicciones
        // Reporte de partidos ordenados por mayor cantidad de predicciones.
        [HttpGet("partidos-mas-predicciones")]
        public async Task<IActionResult> ObtenerPartidosMasPredicciones()
        {
            var reporte = await _context.Predicciones
                .AsNoTracking()
                .GroupBy(p => p.PartidoId)
                .Select(g => new
                {
                    PartidoId = g.Key,
                    TotalPredicciones = g.Count(),
                    TotalApostado = g.Sum(p => p.MontoApostado),
                    PrediccionesPendientes = g.Count(p => p.Estado == GolCoinConfig.EstadoPendiente),
                    PrediccionesGanadas = g.Count(p => p.Estado == GolCoinConfig.EstadoGanada),
                    PrediccionesPerdidas = g.Count(p => p.Estado == GolCoinConfig.EstadoPerdida),
                    TotalPremiosPagados = g.Sum(p => p.PremioPagado)
                })
                .OrderByDescending(r => r.TotalPredicciones)
                .ThenByDescending(r => r.TotalApostado)
                .ToListAsync();

            return Ok(reporte);
        }
    }
}
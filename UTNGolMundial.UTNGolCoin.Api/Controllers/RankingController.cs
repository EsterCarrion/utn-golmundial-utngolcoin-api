using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UTNGolMundial.UTNGolCoin.Api.Config;
using UTNGolMundial.UTNGolCoin.Api.Data;

namespace UTNGolMundial.UTNGolCoin.Api.Controllers
{
    [Route("api/ranking")]
    [ApiController]
    public class RankingController : ControllerBase
    {
        private readonly GolCoinDbContext _context;

        public RankingController(GolCoinDbContext context)
        {
            _context = context;
        }

        // GET: api/ranking
        // Muestra el ranking de usuarios según saldo actual, predicciones ganadas y premios acumulados.
        [HttpGet]
        public async Task<IActionResult> ObtenerRanking()
        {
            var billeteras = await _context.Billeteras
                .AsNoTracking()
                .Where(b => b.Activa)
                .ToListAsync();

            var estadisticasPredicciones = await _context.Predicciones
                .AsNoTracking()
                .GroupBy(p => p.UsuarioId)
                .Select(g => new
                {
                    UsuarioId = g.Key,
                    TotalPredicciones = g.Count(),
                    PrediccionesGanadas = g.Count(p => p.Estado == GolCoinConfig.EstadoGanada),
                    PrediccionesPerdidas = g.Count(p => p.Estado == GolCoinConfig.EstadoPerdida),
                    PremiosAcumulados = g.Sum(p => p.PremioPagado)
                })
                .ToListAsync();

            var rankingBase = billeteras
                .Select(b =>
                {
                    var stats = estadisticasPredicciones
                        .FirstOrDefault(e => e.UsuarioId == b.UsuarioId);

                    return new
                    {
                        b.UsuarioId,
                        b.Saldo,
                        TotalPredicciones = stats?.TotalPredicciones ?? 0,
                        PrediccionesGanadas = stats?.PrediccionesGanadas ?? 0,
                        PrediccionesPerdidas = stats?.PrediccionesPerdidas ?? 0,
                        PremiosAcumulados = stats?.PremiosAcumulados ?? 0
                    };
                })
                .OrderByDescending(r => r.Saldo)
                .ThenByDescending(r => r.PrediccionesGanadas)
                .ThenByDescending(r => r.PremiosAcumulados)
                .ToList();

            var ranking = rankingBase
                .Select((r, index) => new
                {
                    posicion = index + 1,
                    r.UsuarioId,
                    moneda = GolCoinConfig.NombreMoneda,
                    simbolo = GolCoinConfig.SimboloMoneda,
                    r.Saldo,
                    r.TotalPredicciones,
                    r.PrediccionesGanadas,
                    r.PrediccionesPerdidas,
                    r.PremiosAcumulados
                })
                .ToList();

            return Ok(ranking);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using UTNGolMundial.UTNGolCoin.Api.Config;
using UTNGolMundial.UTNGolCoin.Api.Data;
using UTNGolMundial.UTNGolCoin.Api.DTOs;

namespace UTNGolMundial.UTNGolCoin.Api.Services
{
    public class LiquidacionService
    {
        private readonly GolCoinDbContext _context;
        private readonly BilleteraService _billeteraService;

        public LiquidacionService(
            GolCoinDbContext context,
            BilleteraService billeteraService)
        {
            _context = context;
            _billeteraService = billeteraService;
        }

        public async Task<object> LiquidarPartidoAsync(LiquidarPartidoDto dto)
        {
            // Calcular resultado oficial del partido: LOCAL, EMPATE o VISITANTE.
            var resultadoOficial = CalcularResultadoOficial(
                dto.GolesLocal,
                dto.GolesVisitante
            );

            // Buscar todas las predicciones del partido.
            var prediccionesDelPartido = await _context.Predicciones
                .Where(p => p.PartidoId == dto.PartidoId)
                .ToListAsync();

            if (!prediccionesDelPartido.Any())
            {
                return new
                {
                    mensaje = "No existen predicciones registradas para este partido.",
                    partidoId = dto.PartidoId,
                    resultadoOficial,
                    totalPredicciones = 0,
                    prediccionesGanadas = 0,
                    prediccionesPerdidas = 0,
                    totalPagado = 0m
                };
            }

            // Evitar doble liquidación.
            var yaFueLiquidado = prediccionesDelPartido
                .Any(p => p.Estado != GolCoinConfig.EstadoPendiente
                       || p.FechaLiquidacion != null
                       || p.ResultadoOficial != null);

            if (yaFueLiquidado)
            {
                throw new InvalidOperationException(
                    "Este partido ya fue liquidado anteriormente."
                );
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var fechaLiquidacion = DateTime.UtcNow;

                int prediccionesGanadas = 0;
                int prediccionesPerdidas = 0;
                decimal totalPagado = 0;

                foreach (var prediccion in prediccionesDelPartido)
                {
                    prediccion.ResultadoOficial = resultadoOficial;
                    prediccion.FechaLiquidacion = fechaLiquidacion;

                    // Si la predicción coincide con el resultado oficial, se marca como ganada.
                    if (prediccion.ResultadoPronosticado == resultadoOficial)
                    {
                        var premio = prediccion.MontoApostado * prediccion.Cuota;

                        prediccion.Estado = GolCoinConfig.EstadoGanada;
                        prediccion.PremioPagado = premio;

                        prediccionesGanadas++;
                        totalPagado += premio;

                        // Sumar premio a la billetera y registrar transacción PREMIO.
                        await _billeteraService.SumarSaldoAsync(
                            prediccion.UsuarioId,
                            premio,
                            GolCoinConfig.TipoPremio,
                            $"Premio por predicción ganada en el partido {dto.PartidoId}. Resultado oficial: {resultadoOficial}",
                            prediccion.Id
                        );
                    }
                    else
                    {
                        // Si no coincide, se marca como perdida.
                        prediccion.Estado = GolCoinConfig.EstadoPerdida;
                        prediccion.PremioPagado = 0;

                        prediccionesPerdidas++;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new
                {
                    mensaje = "Partido liquidado correctamente.",
                    partidoId = dto.PartidoId,
                    golesLocal = dto.GolesLocal,
                    golesVisitante = dto.GolesVisitante,
                    resultadoOficial,
                    totalPredicciones = prediccionesDelPartido.Count,
                    prediccionesGanadas,
                    prediccionesPerdidas,
                    totalPagado
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static string CalcularResultadoOficial(int golesLocal, int golesVisitante)
        {
            if (golesLocal > golesVisitante)
            {
                return GolCoinConfig.ResultadoLocal;
            }

            if (golesLocal < golesVisitante)
            {
                return GolCoinConfig.ResultadoVisitante;
            }

            return GolCoinConfig.ResultadoEmpate;
        }
    }
}
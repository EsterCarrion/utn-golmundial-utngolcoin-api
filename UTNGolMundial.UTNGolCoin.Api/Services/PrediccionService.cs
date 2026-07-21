using Microsoft.EntityFrameworkCore;
using UTNGolMundial.UTNGolCoin.Api.Config;
using UTNGolMundial.UTNGolCoin.Api.Data;
using UTNGolMundial.UTNGolCoin.Api.DTOs;
using UTNGolMundial.UTNGolCoin.Api.Models;

namespace UTNGolMundial.UTNGolCoin.Api.Services
{
    public class PrediccionService
    {
        private readonly GolCoinDbContext _context;
        private readonly BilleteraService _billeteraService;
        private readonly EstadisticasApiService _estadisticasApiService;

        public PrediccionService(
            GolCoinDbContext context,
            BilleteraService billeteraService,
            EstadisticasApiService estadisticasApiService)
        {
            _context = context;
            _billeteraService = billeteraService;
            _estadisticasApiService = estadisticasApiService;
        }

        public async Task<PrediccionResponseDto> CrearPrediccionAsync(CrearPrediccionDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (dto.UsuarioId <= 0)
            {
                throw new ArgumentException(
                    "El identificador del usuario debe ser mayor a cero."
                );
            }
            if (dto.PartidoId <= 0)
            {
                throw new ArgumentException(
                    "El identificador del partido debe ser mayor a cero."
                );
            }
            if (string.IsNullOrWhiteSpace(dto.ResultadoPronosticado))
            {
                throw new ArgumentException(
                    "El resultado pronosticado es obligatorio."
                );
            }

            var resultado = dto.ResultadoPronosticado.Trim().ToUpperInvariant();

            // Validar resultado permitido en predicción 1X2.
            if (!EsResultadoValido(resultado))
            {
                throw new ArgumentException("El resultado debe ser LOCAL, EMPATE o VISITANTE.");
            }

            // Validar monto apostado.
            if (dto.MontoApostado <= 0)
            {
                throw new ArgumentException("El monto apostado debe ser mayor a cero.");
            }

            // Consultar el partido en la API de Estadísticas.
            var partido = await _estadisticasApiService
                .ObtenerPartidoPorIdAsync(dto.PartidoId);

            // Validar que el partido exista.
            if (partido == null)
            {
                throw new InvalidOperationException(
                    "El partido indicado no existe en el Servicio de Estadísticas."
                );
            }

            var estadoPartido = partido.Estado?
                .Trim()
                .ToUpperInvariant();

            if (estadoPartido == "FINALIZADO")
            {
                throw new InvalidOperationException(
                    "No se puede realizar una predicción porque el partido ya está finalizado."
                );
            }

            // Validar que todavía no haya llegado la fecha y hora de inicio.
            if (DateTimeOffset.UtcNow >= partido.FechaHoraUtc.ToUniversalTime())
            {
                throw new InvalidOperationException(
                    "Las predicciones están cerradas porque el partido ya inició."
                );
            }

            // Validar que el usuario tenga billetera en GolCoin.
            var billetera = await _billeteraService.ObtenerBilleteraPorUsuarioAsync(dto.UsuarioId);

            if (billetera == null)
            {
                throw new InvalidOperationException("El usuario no tiene billetera registrada.");
            }

            if (!billetera.Activa)
            {
                throw new InvalidOperationException("La billetera del usuario no está activa.");
            }

            // Validar saldo suficiente.
            if (billetera.Saldo < dto.MontoApostado)
            {
                throw new InvalidOperationException("Saldo insuficiente para realizar la predicción.");
            }

            // Validar una sola predicción por usuario y partido.
            var yaExistePrediccion = await _context.Predicciones
                .AnyAsync(p => p.UsuarioId == dto.UsuarioId && p.PartidoId == dto.PartidoId);

            if (yaExistePrediccion)
            {
                throw new InvalidOperationException("El usuario ya realizó una predicción para este partido.");
            }


            var cuota = ObtenerCuotaPorResultado(resultado);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var prediccion = new Prediccion
                {
                    UsuarioId = dto.UsuarioId,
                    PartidoId = dto.PartidoId,
                    ResultadoPronosticado = resultado,
                    MontoApostado = dto.MontoApostado,
                    Cuota = cuota,
                    Estado = GolCoinConfig.EstadoPendiente,
                    ResultadoOficial = null,
                    PremioPagado = 0,
                    FechaCreacion = DateTime.UtcNow,
                    FechaLiquidacion = null
                };

                _context.Predicciones.Add(prediccion);
                await _context.SaveChangesAsync();

                // Descuenta saldo y registra la transacción tipo PREDICCION.
                var saldoRestante = await _billeteraService.DescontarSaldoAsync(
                    dto.UsuarioId,
                    dto.MontoApostado,
                    GolCoinConfig.TipoPrediccion,
                    $"Predicción registrada para el partido {dto.PartidoId}. Resultado: {resultado}",
                    prediccion.Id
                );

                await transaction.CommitAsync();

                return MapearPrediccion(prediccion, saldoRestante);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<PrediccionResponseDto>> ObtenerPrediccionesPorUsuarioAsync(int usuarioId)
        {
            var predicciones = await _context.Predicciones
                .AsNoTracking()
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return predicciones
                .Select(p => MapearPrediccion(p))
                .ToList();
        }

        public async Task<List<PrediccionResponseDto>> ObtenerPrediccionesPorPartidoAsync(int partidoId)
        {
            var predicciones = await _context.Predicciones
                .AsNoTracking()
                .Where(p => p.PartidoId == partidoId)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return predicciones
                .Select(p => MapearPrediccion(p))
                .ToList();
        }

        private static bool EsResultadoValido(string resultado)
        {
            return resultado == GolCoinConfig.ResultadoLocal
                || resultado == GolCoinConfig.ResultadoEmpate
                || resultado == GolCoinConfig.ResultadoVisitante;
        }

        private static decimal ObtenerCuotaPorResultado(string resultado)
        {
            return resultado switch
            {
                GolCoinConfig.ResultadoLocal => GolCoinConfig.CuotaLocal,
                GolCoinConfig.ResultadoEmpate => GolCoinConfig.CuotaEmpate,
                GolCoinConfig.ResultadoVisitante => GolCoinConfig.CuotaVisitante,
                _ => throw new ArgumentException("Resultado inválido.")
            };
        }

        private static PrediccionResponseDto MapearPrediccion(
            Prediccion prediccion,
            decimal? saldoRestante = null)
        {
            return new PrediccionResponseDto
            {
                Id = prediccion.Id,
                UsuarioId = prediccion.UsuarioId,
                PartidoId = prediccion.PartidoId,
                ResultadoPronosticado = prediccion.ResultadoPronosticado,
                MontoApostado = prediccion.MontoApostado,
                Cuota = prediccion.Cuota,
                Estado = prediccion.Estado,
                ResultadoOficial = prediccion.ResultadoOficial,
                PremioPagado = prediccion.PremioPagado,
                FechaCreacion = prediccion.FechaCreacion,
                FechaLiquidacion = prediccion.FechaLiquidacion,
                SaldoRestante = saldoRestante
            };
        }
    }
}
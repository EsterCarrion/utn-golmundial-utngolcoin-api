using System.Net;
using System.Net.Http.Json;
using UTNGolMundial.UTNGolCoin.Api.DTOs;

namespace UTNGolMundial.UTNGolCoin.Api.Services
{
    public class EstadisticasApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EstadisticasApiService> _logger;

        public EstadisticasApiService(
            HttpClient httpClient,
            ILogger<EstadisticasApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PartidoEstadisticasDto?> ObtenerPartidoPorIdAsync(
            int partidoId,
            CancellationToken cancellationToken = default)
        {
            if (partidoId <= 0)
            {
                throw new ArgumentException(
                    "El identificador del partido debe ser mayor a cero."
                );
            }

            try
            {
                using var response = await _httpClient.GetAsync(
                    $"partidos/{partidoId}",
                    cancellationToken
                );

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var detalle = await response.Content.ReadAsStringAsync(
                        cancellationToken
                    );

                    _logger.LogError(
                        "La API de Estadísticas respondió con código {StatusCode}. Respuesta: {Detalle}",
                        response.StatusCode,
                        detalle
                    );

                    throw new InvalidOperationException(
                        "No fue posible consultar el partido en la API de Estadísticas."
                    );
                }

                var partido = await response.Content
                    .ReadFromJsonAsync<PartidoEstadisticasDto>(
                        cancellationToken: cancellationToken
                    );

                if (partido == null)
                {
                    throw new InvalidOperationException(
                        "La API de Estadísticas devolvió una respuesta vacía."
                    );
                }

                return partido;
            }
            catch (TaskCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    ex,
                    "La consulta a la API de Estadísticas excedió el tiempo de espera."
                );

                throw new InvalidOperationException(
                    "La API de Estadísticas tardó demasiado en responder.",
                    ex
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No se pudo conectar con la API de Estadísticas."
                );

                throw new InvalidOperationException(
                    "No se pudo establecer comunicación con la API de Estadísticas.",
                    ex
                );
            }
        }
    }
}
using System.Text.Json.Serialization;

namespace UTNGolMundial.UTNGolCoin.Api.DTOs
{
    public class PartidoEstadisticasDto
    {
        /*
         * Estos nombres deben coincidir con el JSON que devuelve
         * la API Jakarta de Estadísticas.
         */

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fechaHoraUtc")]
        public DateTimeOffset FechaHoraUtc { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;

        [JsonPropertyName("golesLocal")]
        public int? GolesLocal { get; set; }

        [JsonPropertyName("golesVisitante")]
        public int? GolesVisitante { get; set; }
    }
}
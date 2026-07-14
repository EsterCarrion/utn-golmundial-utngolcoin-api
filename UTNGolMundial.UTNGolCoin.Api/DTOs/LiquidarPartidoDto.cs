using System.ComponentModel.DataAnnotations;

namespace UTNGolMundial.UTNGolCoin.Api.DTOs
{
    public class LiquidarPartidoDto
    {
        // ID del partido que viene desde la API de estadísticas.
        [Required(ErrorMessage = "El partido es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El partido debe ser válido.")]
        public int PartidoId { get; set; }

        // Goles del equipo local registrados por el backend de estadísticas.
        [Required(ErrorMessage = "Los goles del local son obligatorios.")]
        [Range(0, int.MaxValue, ErrorMessage = "Los goles del local no pueden ser negativos.")]
        public int GolesLocal { get; set; }

        // Goles del equipo visitante registrados por el backend de estadísticas.
        [Required(ErrorMessage = "Los goles del visitante son obligatorios.")]
        [Range(0, int.MaxValue, ErrorMessage = "Los goles del visitante no pueden ser negativos.")]
        public int GolesVisitante { get; set; }
    }
}
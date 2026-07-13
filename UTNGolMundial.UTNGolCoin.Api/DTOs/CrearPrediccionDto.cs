using System.ComponentModel.DataAnnotations;

namespace UTNGolMundial.UTNGolCoin.Api.DTOs
{
    public class CrearPrediccionDto
    {
        // ID del usuario que viene desde la API de usuarios/estadísticas.
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El usuario debe ser válido.")]
        public int UsuarioId { get; set; }

        // ID del partido que viene desde la API de estadísticas.
        [Required(ErrorMessage = "El partido es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El partido debe ser válido.")]
        public int PartidoId { get; set; }

        // Valores permitidos: LOCAL, EMPATE o VISITANTE.
        [Required(ErrorMessage = "El resultado pronosticado es obligatorio.")]
        [MaxLength(20, ErrorMessage = "El resultado pronosticado no puede superar los 20 caracteres.")]
        public string ResultadoPronosticado { get; set; } = string.Empty;

        // Monto de UTNGolCoin que el usuario apuesta.
        [Required(ErrorMessage = "El monto apostado es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto apostado debe ser mayor a 0.")]
        public decimal MontoApostado { get; set; }
    }
}
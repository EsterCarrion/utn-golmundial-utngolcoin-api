using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UTNGolMundial.UTNGolCoin.Api.Models
{
    public class Prediccion
    {
        public int Id { get; set; }

        // ID del usuario externo
        public int UsuarioId { get; set; }

        // ID del partido externo
        public int PartidoId { get; set; }

        // Valores permitidos: LOCAL, EMPATE, VISITANTE
        [Required]
        [MaxLength(20)]
        public string ResultadoPronosticado { get; set; } = string.Empty;

        public decimal MontoApostado { get; set; }

        public decimal Cuota { get; set; }

        // PENDIENTE, GANADA, PERDIDA
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "PENDIENTE";

        // Se llena cuando se liquida el partido: LOCAL, EMPATE o VISITANTE
        [MaxLength(20)]
        public string? ResultadoOficial { get; set; }

        public decimal PremioPagado { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaLiquidacion { get; set; }

        // Relación interna con transacciones de esta predicción
        public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    }
}
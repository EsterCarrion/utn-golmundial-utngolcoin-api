using System.ComponentModel.DataAnnotations;

namespace UTNGolMundial.UTNGolCoin.Api.Models
{
    public class Transaccion
    {
        public int Id { get; set; }

        public int BilleteraId { get; set; }

        // Se guarda también para consultar historial por usuario más fácil
        public int UsuarioId { get; set; }

        // Ejemplo: BONO_BIENVENIDA, PREDICCION, PREMIO, BONO_DIARIO
        [Required]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        // Puede ser positivo o negativo según la operación
        public decimal Monto { get; set; }

        public decimal SaldoResultante { get; set; }

        [MaxLength(250)]
        public string? Descripcion { get; set; }

        // Si la transacción está relacionada con una predicción
        public int? PrediccionId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // Relación interna
        public Billetera? Billetera { get; set; }

        public Prediccion? Prediccion { get; set; }
    }
}
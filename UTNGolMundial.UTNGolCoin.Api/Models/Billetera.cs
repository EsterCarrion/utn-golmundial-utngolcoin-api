using System.Collections.Generic;

namespace UTNGolMundial.UTNGolCoin.Api.Models
{
    public class Billetera
    {
        public int Id { get; set; }

        // ID del usuario que viene desde el backend de usuarios/estadísticas
        public int UsuarioId { get; set; }

        public decimal Saldo { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        public bool Activa { get; set; } = true;

        // Relación interna con transacciones de esta misma base GolCoin
        public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    }
}
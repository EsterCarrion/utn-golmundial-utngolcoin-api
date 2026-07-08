namespace UTNGolMundial.UTNGolCoin.Api.Models
{
    public class BonoDiario
    {
        public int Id { get; set; }

        // ID del usuario externo
        public int UsuarioId { get; set; }

        // Fecha del bono. Se usará para evitar más de un bono por día.
        public DateTime FechaBono { get; set; } = DateTime.UtcNow.Date;

        public decimal Monto { get; set; }

        public int? TransaccionId { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Relación interna opcional con la transacción que registró el bono
        public Transaccion? Transaccion { get; set; }
    }
}
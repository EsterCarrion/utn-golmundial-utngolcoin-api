namespace UTNGolMundial.UTNGolCoin.Api.DTOs
{
    public class PrediccionResponseDto
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int PartidoId { get; set; }

        public string ResultadoPronosticado { get; set; } = string.Empty;

        public decimal MontoApostado { get; set; }

        public decimal Cuota { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string? ResultadoOficial { get; set; }

        public decimal PremioPagado { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaLiquidacion { get; set; }

        public decimal? SaldoRestante { get; set; }
    }
}
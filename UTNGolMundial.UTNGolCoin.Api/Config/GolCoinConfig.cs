namespace UTNGolMundial.UTNGolCoin.Api.Config
{
    public static class GolCoinConfig
    {
        public const string NombreMoneda = "UTNGolCoin";
        public const string SimboloMoneda = "UGC";

        public const decimal BonoBienvenida = 10;
        public const decimal BonoDiarioSaldoCero = 1;

        public const decimal CuotaLocal = 1.8m;
        public const decimal CuotaEmpate = 3.0m;
        public const decimal CuotaVisitante = 2.5m;

        public const int PrediccionesPorPartidoPorUsuario = 1;

        // Resultados permitidos
        public const string ResultadoLocal = "LOCAL";
        public const string ResultadoEmpate = "EMPATE";
        public const string ResultadoVisitante = "VISITANTE";

        // Estados de predicción
        public const string EstadoPendiente = "PENDIENTE";
        public const string EstadoGanada = "GANADA";
        public const string EstadoPerdida = "PERDIDA";

        // Tipos de transacción
        public const string TipoBonoBienvenida = "BONO_BIENVENIDA";
        public const string TipoPrediccion = "PREDICCION";
        public const string TipoPremio = "PREMIO";
        public const string TipoBonoDiario = "BONO_DIARIO";
    }
}
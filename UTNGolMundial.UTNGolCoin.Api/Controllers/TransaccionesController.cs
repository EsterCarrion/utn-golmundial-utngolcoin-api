using Microsoft.AspNetCore.Mvc;
using UTNGolMundial.UTNGolCoin.Api.Services;

namespace UTNGolMundial.UTNGolCoin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransaccionesController : ControllerBase
    {
        private readonly TransaccionService _transaccionService;

        public TransaccionesController(TransaccionService transaccionService)
        {
            _transaccionService = transaccionService;
        }

        // GET: api/Transacciones/usuario/1
        // Obtiene el historial de transacciones de un usuario.
        // Incluye bonos, predicciones, premios y otros movimientos de saldo.
        [HttpGet("usuario/{usuarioId:int}")]
        public async Task<IActionResult> ObtenerHistorialPorUsuario(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El usuarioId debe ser mayor a 0."
                });
            }

            var transacciones = await _transaccionService.ObtenerHistorialPorUsuarioAsync(usuarioId);

            var respuesta = transacciones.Select(t => new
            {
                t.Id,
                t.UsuarioId,
                t.BilleteraId,
                t.Tipo,
                t.Monto,
                t.SaldoResultante,
                t.Descripcion,
                t.PrediccionId,
                t.Fecha
            });

            return Ok(respuesta);
        }
    }
}
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using UTNGolMundial.UTNGolCoin.Api.DTOs;
using UTNGolMundial.UTNGolCoin.Api.Services;

namespace UTNGolMundial.UTNGolCoin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiquidacionesController : ControllerBase
    {
        private readonly LiquidacionService _liquidacionService;

        public LiquidacionesController(LiquidacionService liquidacionService)
        {
            _liquidacionService = liquidacionService;
        }

        // POST: api/Liquidaciones
        // Liquida un partido usando el resultado oficial enviado por la API de estadísticas.
        // Calcula si ganó LOCAL, EMPATE o VISITANTE.
        // Marca las predicciones como GANADA o PERDIDA.
        // Paga premio = monto apostado * cuota a las predicciones ganadas.
        [HttpPost]
        public async Task<IActionResult> LiquidarPartido([FromBody] LiquidarPartidoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await _liquidacionService.LiquidarPartidoAsync(dto);

                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = "Ocurrió un error al liquidar el partido.",
                    detalle = ex.Message
                });
            }
        }
    }
}
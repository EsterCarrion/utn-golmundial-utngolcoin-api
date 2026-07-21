using Microsoft.AspNetCore.Mvc;
using UTNGolMundial.UTNGolCoin.Api.Services;

namespace UTNGolMundial.UTNGolCoin.Api.Controllers
{
    [Route("api/bonos-diarios")]
    [ApiController]
    public class BonosDiariosController : ControllerBase
    {
        private readonly BonoDiarioService _bonoDiarioService;

        public BonosDiariosController(BonoDiarioService bonoDiarioService)
        {
            _bonoDiarioService = bonoDiarioService;
        }

        // POST: api/bonos-diarios/1
        // Otorga 1 UTNGolCoin diario solo si el usuario tiene saldo 0
        // y todavía no recibió el bono diario en la fecha actual.
        [HttpPost("{usuarioId:int}")]
        public async Task<IActionResult> OtorgarBonoDiario(int usuarioId)
        {
            try
            {
                var resultado = await _bonoDiarioService.OtorgarBonoDiarioAsync(usuarioId);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
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
                    mensaje = "Ocurrió un error al otorgar el bono diario.",
                    detalle = ex.Message
                });
            }
        }
        // POST: api/bonos-diarios/admin/masivo
        [HttpPost("admin/masivo")]
        public async Task<IActionResult> OtorgarBonoMasivoAdmin()
        {
            var usuariosBeneficiados =
                await _bonoDiarioService.OtorgarBonoMasivoAdminAsync();

            return Ok(new
            {
                mensaje = usuariosBeneficiados > 0
                    ? "El bono fue otorgado correctamente."
                    : "No existen usuarios con saldo en cero.",
                usuariosBeneficiados
            });
        }
    }
}
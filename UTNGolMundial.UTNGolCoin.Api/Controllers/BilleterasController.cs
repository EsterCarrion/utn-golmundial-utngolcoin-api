using Microsoft.AspNetCore.Mvc;
using UTNGolMundial.UTNGolCoin.Api.Config;
using UTNGolMundial.UTNGolCoin.Api.Services;

namespace UTNGolMundial.UTNGolCoin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BilleterasController : ControllerBase
    {
        private readonly BilleteraService _billeteraService;

        public BilleterasController(BilleteraService billeteraService)
        {
            _billeteraService = billeteraService;
        }

        // POST: api/Billeteras/crear/1
        // Crea una billetera para el usuario indicado.
        // Si la billetera ya existe, no crea una nueva y retorna la existente.
        [HttpPost("crear/{usuarioId:int}")]
        public async Task<IActionResult> CrearBilletera(int usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje = "El usuarioId debe ser mayor a 0."
                    });
                }

                var billeteraExistente = await _billeteraService.ObtenerBilleteraPorUsuarioAsync(usuarioId);

                if (billeteraExistente != null)
                {
                    return Ok(new
                    {
                        mensaje = "El usuario ya tiene una billetera registrada.",
                        billetera = new
                        {
                            billeteraExistente.Id,
                            billeteraExistente.UsuarioId,
                            billeteraExistente.Saldo,
                            billeteraExistente.FechaCreacion,
                            billeteraExistente.FechaActualizacion,
                            billeteraExistente.Activa
                        }
                    });
                }

                var billetera = await _billeteraService.CrearBilleteraAsync(usuarioId);

                return Ok(new
                {
                    mensaje = "Billetera creada correctamente con bono de bienvenida.",
                    billetera = new
                    {
                        billetera.Id,
                        billetera.UsuarioId,
                        billetera.Saldo,
                        billetera.FechaCreacion,
                        billetera.FechaActualizacion,
                        billetera.Activa
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        // GET: api/Billeteras/1/saldo
        // Consulta el saldo actual de la billetera del usuario indicado.
        [HttpGet("{usuarioId:int}/saldo")]
        public async Task<IActionResult> ConsultarSaldo(int usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje = "El usuarioId debe ser mayor a 0."
                    });
                }

                var saldo = await _billeteraService.ConsultarSaldoAsync(usuarioId);

                return Ok(new
                {
                    usuarioId,
                    moneda = GolCoinConfig.NombreMoneda,
                    simbolo = GolCoinConfig.SimboloMoneda,
                    saldo
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }
    }
}
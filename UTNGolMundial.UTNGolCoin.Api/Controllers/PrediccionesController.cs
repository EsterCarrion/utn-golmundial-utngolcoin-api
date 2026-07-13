using Microsoft.AspNetCore.Mvc;
using UTNGolMundial.UTNGolCoin.Api.DTOs;
using UTNGolMundial.UTNGolCoin.Api.Services;

namespace UTNGolMundial.UTNGolCoin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrediccionesController : ControllerBase
    {
        private readonly PrediccionService _prediccionService;

        public PrediccionesController(PrediccionService prediccionService)
        {
            _prediccionService = prediccionService;
        }

        // POST: api/Predicciones
        // Registra una predicción 1X2 para un partido.
        // Valida saldo, resultado permitido y que no exista una predicción previa del mismo usuario para el mismo partido.
        [HttpPost]
        public async Task<IActionResult> CrearPrediccion([FromBody] CrearPrediccionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var prediccion = await _prediccionService.CrearPrediccionAsync(dto);

                return Ok(new
                {
                    mensaje = "Predicción registrada correctamente.",
                    prediccion
                });
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
                    mensaje = "Ocurrió un error al registrar la predicción.",
                    detalle = ex.Message
                });
            }
        }

        // GET: api/Predicciones/usuario/1
        // Obtiene todas las predicciones realizadas por un usuario.
        [HttpGet("usuario/{usuarioId:int}")]
        public async Task<IActionResult> ObtenerPrediccionesPorUsuario(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El usuarioId debe ser mayor a 0."
                });
            }

            var predicciones = await _prediccionService.ObtenerPrediccionesPorUsuarioAsync(usuarioId);

            return Ok(predicciones);
        }

        // GET: api/Predicciones/partido/10
        // Obtiene todas las predicciones realizadas para un partido.
        [HttpGet("partido/{partidoId:int}")]
        public async Task<IActionResult> ObtenerPrediccionesPorPartido(int partidoId)
        {
            if (partidoId <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El partidoId debe ser mayor a 0."
                });
            }

            var predicciones = await _prediccionService.ObtenerPrediccionesPorPartidoAsync(partidoId);

            return Ok(predicciones);
        }
    }
}   
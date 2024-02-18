using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ILogger<VillaController> _logger;
        private readonly ApplicationDbContext _db;
        public VillaController(ILogger<VillaController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDto>> GetVillas()
        {
            _logger.LogInformation("Obtener las Villas");
            return Ok(_db.Villas.ToList());
        }

        [HttpGet("{id:int}", Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDto> GetVilla(int id)
        {
            if(id==0)
            {
                _logger.LogError("Error al traer Villa con Id " + id);
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            var villa = _db.Villas.FirstOrDefault(v => v.Id == id);

            if(villa==null)
            {
                _logger.LogError("Error No existe Villa con Id " + id);
                return NotFound();
            }
            _logger.LogInformation("Obtener Villa con ID " + id);
            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDto> CreateVilla([FromBody] VillaDto villaDto)
        {
            if(!ModelState.IsValid)
            {
                _logger.LogError("Error al crear nueva Villa - modelo no valido");
                return BadRequest(ModelState);
            }

            if(_db.Villas.FirstOrDefault(v =>v.Nombre.ToLower() == villaDto.Nombre.ToLower())!=null)
            {
                _logger.LogError("Error al crear nueva Villa - Villa existente");
                ModelState.AddModelError("Nombre Existe", "La Villa con ese Nombre ya existe");
                return BadRequest(ModelState);
            }

            if(villaDto ==null)
            {
                _logger.LogError("Error al enviar modelo de Villa");
                return BadRequest(villaDto);
            }

            if(villaDto.Id>0)
            {
                _logger.LogError("Error de Servidor Interno");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            //Buscar por ID en orden descendente
            //villaDto.Id = VillaStore.villaList.OrderByDescending(v => v.Id).FirstOrDefault().Id+1;
            //VillaStore.villaList.Add(villaDto);
            Villa modelo = new()
            {
                Id = villaDto.Id,
                Nombre = villaDto.Nombre,
                Detalle = villaDto.Detalle,
                Tarifa = villaDto.Tarifa,
                Ocupantes = villaDto.Ocupantes,
                ImagenUrl = villaDto.ImagenUrl,
                Amenidad = villaDto.Amenidad
            };
            _db.Villas.Add(modelo);
            _db.SaveChanges();
            //Para indicar la URL del recurso creado
            _logger.LogInformation("Creación de Villa");
            return CreatedAtRoute("GetVilla", new {id = villaDto.Id}, villaDto);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVilla(int id)
        {
            if(id==0)
            {
                _logger.LogError("Error al traer Villa con Id " + id);
                return BadRequest();
            }
            var villa = _db.Villas.FirstOrDefault(v => v.Id == id);
            
            if(villa==null)
            {
                _logger.LogError("Error No existe Villa con Id " + id);
                return NotFound();
            }
            //Eliminamos registro en la DB
            _db.Villas.Remove(villa);
            _db.SaveChanges();

            _logger.LogInformation("Eliminación de Villa ID " + id);
            return NoContent();
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateVilla(int id, [FromBody] VillaDto villaDto)
        {
            if ((villaDto==null || id!=villaDto.Id))
            {
                _logger.LogError("Error al traer Villa con Id " + id);
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(v =>v.Id==id);
            //villa.Nombre = villaDto.Nombre;
            //villa.Ocupantes = villaDto.Ocupantes;
            Villa modelo = new Villa()
            {
                Id = villaDto.Id,
                Nombre = villaDto.Nombre,
                Detalle = villaDto.Detalle,
                Tarifa = villaDto.Tarifa,
                Ocupantes = villaDto.Ocupantes,
                ImagenUrl = villaDto.ImagenUrl,
                Amenidad = villaDto.Amenidad
            };

            _db.Villas.Update(modelo);
            _db.SaveChanges();
            _logger.LogInformation("Actualización PUT de Villa");
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDto> patchDto)
        {
            if ((patchDto == null || id == 0))
            {
                _logger.LogError("Error al traer Villa con Id " + id);
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            var villa = _db.Villas.AsNoTracking().FirstOrDefault(v => v.Id == id);
            VillaDto villaDto = new()
            {
                Id = villa.Id,
                Nombre = villa.Nombre,
                Detalle = villa.Detalle,
                Tarifa = villa.Tarifa,
                Ocupantes = villa.Ocupantes,
                ImagenUrl = villa.ImagenUrl,
                Amenidad = villa.Amenidad
            };

            if (villa == null)
            {
                _logger.LogError("Error Villa Inexistente");
                return BadRequest();
            }

            patchDto.ApplyTo(villaDto, ModelState);

            if(!ModelState.IsValid)
            {
                _logger.LogError("Error Modelo de Villa no valido");
                return BadRequest(ModelState);
            }

            Villa modelo = new()
            {
                Id = villaDto.Id,
                Nombre = villaDto.Nombre,
                Detalle = villaDto.Detalle,
                Tarifa = villaDto.Tarifa,
                Ocupantes = villaDto.Ocupantes,
                ImagenUrl = villaDto.ImagenUrl,
                Amenidad = villaDto.Amenidad
            };

            _db.Villas.Update(modelo);
            _db.SaveChanges();

            _logger.LogInformation("Actualización PATCH de Villa");
            return NoContent();
        }

    }
}

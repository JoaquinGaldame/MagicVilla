using AutoMapper;
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
        // Inyectamos el servicio de Logger para mostrar info por consola
        private readonly ILogger<VillaController> _logger;
        // Variable privada para la Conexión de la Base de Datos
        private readonly ApplicationDbContext _db;
        // Agregamos Mapper
        private readonly IMapper _mapper;
        public VillaController(ILogger<VillaController> logger, ApplicationDbContext db, IMapper mapper)
        {
            _logger = logger;
            _db = db;
            _mapper = mapper;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDto>>> GetVillas()
        {
            _logger.LogInformation("Obtener las Villas");

            //Creamos una lista de tipo Villa extraida de la DB
            IEnumerable<Villa> villaList = await _db.Villas.ToListAsync();

            //Retorna un IEnumerable del tipo VillaDto obteniendo los datos de villaList
            return Ok(_mapper.Map<IEnumerable<VillaDto>>(villaList));
        }

        [HttpGet("{id:int}", Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VillaDto>> GetVilla(int id)
        {
            if(id==0)
            {
                _logger.LogError("Error al traer Villa con Id " + id);
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);

            if(villa==null)
            {
                _logger.LogError("Error No existe Villa con Id " + id);
                return NotFound();
            }
            _logger.LogInformation("Obtener Villa con ID " + id);

            //Solo aplicamos el AutoMapper
            //Retornamos un Objeto del tipo VillaDto
            //Obtenemos los datos de la variable villa 
            return Ok(_mapper.Map<VillaDto>(villa));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VillaDto>> CreateVilla([FromBody] VillaCreateDto createDto)
        {
            if(!ModelState.IsValid)
            {
                _logger.LogError("Error al crear nueva Villa - modelo no valido");
                return BadRequest(ModelState);
            }

            if(await _db.Villas.FirstOrDefaultAsync(v =>v.Nombre.ToLower() == createDto.Nombre.ToLower())!=null)
            {
                _logger.LogError("Error al crear nueva Villa - Villa existente");
                ModelState.AddModelError("Nombre Existe", "La Villa con ese Nombre ya existe");
                return BadRequest(ModelState);
            }

            if(createDto ==null)
            {
                _logger.LogError("Error al enviar modelo de Villa");
                return BadRequest(createDto);
            }

            //Buscar por ID en orden descendente
            //villaDto.Id = VillaStore.villaList.OrderByDescending(v => v.Id).FirstOrDefault().Id+1;
            //VillaStore.villaList.Add(villaDto);

            Villa modelo = _mapper.Map<Villa>(createDto);
            
            await _db.Villas.AddAsync(modelo);
            await _db.SaveChangesAsync();
            //Para indicar la URL del recurso creado
            _logger.LogInformation("Creación de Villa");
            return CreatedAtRoute("GetVilla", new {id = modelo.Id}, modelo);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVilla(int id)
        {
            //El método Delete no necesita Mapeo
            if(id==0)
            {
                _logger.LogError("Error al traer Villa con Id " + id);
                return BadRequest();
            }
            var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);
            
            if(villa==null)
            {
                _logger.LogError("Error No existe Villa con Id " + id);
                return NotFound();
            }
            //Eliminamos registro en la DB - No existe Remove asíncrono
            _db.Villas.Remove(villa);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Eliminación de Villa ID " + id);
            return NoContent();
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDto updateDto)
        {
            if ((updateDto==null || id!=updateDto.Id))
            {
                _logger.LogError("Error al traer Villa con Id " + id);
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(v =>v.Id==id);
            //villa.Nombre = villaDto.Nombre;
            //villa.Ocupantes = villaDto.Ocupantes;

            Villa modelo = _mapper.Map<Villa>(updateDto);

            //No existe update asíncrono
            _db.Villas.Update(modelo);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Actualización PUT de Villa");
            return NoContent();
        }


        //Para utilizar PATCH necesitamos instalar las dependencias:
        // 1 - Microsoft.AspNetCore.JsonPatch: Se utiliza 'JsonPatchDocument'
        // 2 - Microsoft.AspNetCore.Mvc.NewtonsoftJson
        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto> patchDto)
        {
            if ((patchDto == null || id == 0))
            {
                _logger.LogError("Error al traer Villa con Id " + id);
                return BadRequest();
            }
            //Capturamos el registro actual para modificar
            //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            
            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);

            if (villa == null)
            {
                _logger.LogError("Error Villa Inexistente");
                return BadRequest();
            }

            //Método ApplyTo de PATCH: Enviamos el registro capturado
            // y verificamos que su modelo sea válido
            patchDto.ApplyTo(villaDto, ModelState);

            if(!ModelState.IsValid)
            {
                _logger.LogError("Error Modelo de Villa no valido");
                return BadRequest(ModelState);
            }

            Villa modelo = _mapper.Map<Villa>(villaDto);

            //No existe update asíncrono
            _db.Villas.Update(modelo);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Actualización PATCH de Villa");
            return NoContent();
        }

    }
}

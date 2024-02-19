using AutoMapper;
using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.Dto;
using MagicVilla_API.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        // Inyectamos el servicio de Logger para mostrar info por consola
        private readonly ILogger<VillaController> _logger;
        // Variable privada para la Conexión de la Base de Datos
        // private readonly ApplicationDbContext _db; YA NO LO USAMOS
        private readonly IVillaRepositorio _villaRepo;
        // Agregamos Mapper
        private readonly IMapper _mapper;
        protected APIResponse _response;
        public VillaController(ILogger<VillaController> logger, IVillaRepositorio villaRepo, IMapper mapper)
        {
            _logger = logger;
            _villaRepo = villaRepo;
            _mapper = mapper;
            _response = new();
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            try
            {
                _logger.LogInformation("Obtener las Villas");

                //Creamos una lista de tipo Villa extraida de la DB
                //IEnumerable<Villa> villaList = await _db.Villas.ToListAsync();
                IEnumerable<Villa> villaList = await _villaRepo.ObtenerTodos();

                _response.Resultado = _mapper.Map<IEnumerable<VillaDto>>(villaList);
                _response.statusCode = HttpStatusCode.OK;
                //Retorna un IEnumerable del tipo VillaDto obteniendo los datos de villaList
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("{id:int}", Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer Villa con Id " + id);
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
                //var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);

                //Enviamos la Expresion LINQ entre ()
                var villa = await _villaRepo.Obtener(v => v.Id == id);

                if (villa == null)
                {
                    _logger.LogError("Error No existe Villa con Id " + id);
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _logger.LogInformation("Obtener Villa con ID " + id);

                //Solo aplicamos el AutoMapper
                //Retornamos un Objeto del tipo VillaDto
                //Obtenemos los datos de la variable villa 
                _response.Resultado = _mapper.Map<VillaDto>(villa);
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {

                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
           
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Error al crear nueva Villa - modelo no valido");
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                // await _db.Villas.FirstOrDefaultAsync(v =>v.Nombre.ToLower() == createDto.Nombre.ToLower())!=null
                if (await _villaRepo.Obtener(v => v.Nombre.ToLower() == createDto.Nombre.ToLower()) != null)
                {
                    _logger.LogError("Error al crear nueva Villa - Villa existente");
                    ModelState.AddModelError("Nombre Existe", "La Villa con ese Nombre ya existe");
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                if (createDto == null)
                {
                    _logger.LogError("Error al enviar modelo de Villa");
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                //Buscar por ID en orden descendente
                //villaDto.Id = VillaStore.villaList.OrderByDescending(v => v.Id).FirstOrDefault().Id+1;
                //VillaStore.villaList.Add(villaDto);

                Villa modelo = _mapper.Map<Villa>(createDto);

                // await _db.Villas.AddAsync(modelo);
                // await _db.SaveChangesAsync(); NO lo usamos porque viene incorporado en el IRepository
                modelo.FechaActualizacion = DateTime.Now;
                modelo.FechaCreacion = DateTime.Now;
                await _villaRepo.Crear(modelo);

                //Para indicar la URL del recurso creado
                _logger.LogInformation("Creación de Villa");
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = modelo.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVilla(int id)
        {
            try
            {
                //El método Delete no necesita Mapeo
                if (id == 0)
                {
                    _logger.LogError("Error al traer Villa con Id " + id);
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var villa = await _villaRepo.Obtener(v => v.Id == id);

                if (villa == null)
                {
                    _logger.LogError("Error No existe Villa con Id " + id);
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                //Eliminamos registro en la DB - No existe Remove asíncrono
                // _db.Villas.Remove(villa);
                // await _db.SaveChangesAsync();
                await _villaRepo.Remover(villa);
                _logger.LogInformation("Eliminación de Villa ID " + id);
                _response.statusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return BadRequest(_response);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDto updateDto)
        {
            if ((updateDto==null || id!=updateDto.Id))
            {
                _logger.LogError("Error al traer Villa con Id " + id);
                _response.IsExitoso = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            //var villa = VillaStore.villaList.FirstOrDefault(v =>v.Id==id);
            //villa.Nombre = villaDto.Nombre;
            //villa.Ocupantes = villaDto.Ocupantes;

            Villa modelo = _mapper.Map<Villa>(updateDto);

            //No existe update asíncrono
            //_db.Villas.Update(modelo);
            //await _db.SaveChangesAsync();
            await _villaRepo.Actualizar(modelo);
            
            _logger.LogInformation("Actualización PUT de Villa");
            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
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
                _response.IsExitoso = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            //Capturamos el registro actual para modificar
            // 1) - var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            // 2) - var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            var villa = await _villaRepo.Obtener(v => v.Id == id, tracked:false);

            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);

            if (villa == null)
            {
                _logger.LogError("Error Villa Inexistente");
                _response.IsExitoso = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            //Método ApplyTo de PATCH: Enviamos el registro capturado
            // y verificamos que su modelo sea válido
            patchDto.ApplyTo(villaDto, ModelState);

            if(!ModelState.IsValid)
            {
                _logger.LogError("Error Modelo de Villa no valido");
                _response.IsExitoso = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            Villa modelo = _mapper.Map<Villa>(villaDto);

            // No existe update asíncrono
            // _db.Villas.Update(modelo);
            // await _db.SaveChangesAsync();
            await _villaRepo.Actualizar(modelo);

            _logger.LogInformation("Actualización PATCH de Villa");
            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }

    }
}

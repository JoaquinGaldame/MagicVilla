using AutoMapper;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.Dto;

namespace MagicVilla_API
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            //Esta es una forma
            CreateMap<Villa, VillaDto>();
            CreateMap<VillaDto, Villa>();

            //Esta es otra forma
            CreateMap<Villa, VillaUpdateDto>().ReverseMap();
            CreateMap<Villa, VillaCreateDto>().ReverseMap();
        }
    }
}

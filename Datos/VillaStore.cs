using MagicVilla_API.Modelos.Dto;

namespace MagicVilla_API.Datos
{
    public static class VillaStore
    {
        public static List<VillaDto> villaList = new List<VillaDto>
        {
            new VillaDto {Id=1, Nombre="Vista a la Piscina", Ocupantes=3},
            new VillaDto {Id=2, Nombre="Vista a la Playa", Ocupantes=5},
            new VillaDto {Id=3, Nombre="Vista a la Ciudad", Ocupantes=8}
        };
    }
}

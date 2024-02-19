using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicVilla_API.Modelos
{
    public class Dueños
    {
        //No se genera el ID automaticamente, lo generamos nosotros
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required]
        public required string Nombre { get; set; }
        [Required]
        public int VillaId { get; set; }
        [ForeignKey("VillaId")]
        public Villa villa { get; set; }


    }
}

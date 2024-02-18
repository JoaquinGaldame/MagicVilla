using MagicVilla_API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_API.Datos
{
    public class ApplicationDbContext :DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :base(options)
        {
            
        }

        public DbSet<Villa> Villas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Villa>().HasData(
                new Villa()
                {
                    Id = 1,
                    Nombre="Villa Langostura",
                    Detalle="450 kilometros",
                    ImagenUrl="",
                    Tarifa = 200,
                    Ocupantes = 5,
                    Amenidad="",
                    FechaCreacion= DateTime.Now,
                    FechaActualizacion= DateTime.Now
                },
                new Villa()
                {
                    Id = 2,
                    Nombre = "Villa Mercedes",
                    Detalle = "320 kilometros",
                    ImagenUrl = "",
                    Tarifa = 150,
                    Ocupantes = 4,
                    Amenidad = "",
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now
                }
                );
        }
    }
}

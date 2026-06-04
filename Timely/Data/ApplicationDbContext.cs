using Microsoft.EntityFrameworkCore;
using Timely.Models;

namespace Timely.Data
{
    // EF Core: el constructor ya no recibe un string con el nombre de la BD.
    // Recibe DbContextOptions, que se configura desde Program.cs vía AddDbContext.
    // Esto permite cambiar la conexión por entorno (dev, staging, producción)
    // sin tocar el código — solo cambia appsettings.json.
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Proyectos> Proyectos { get; set; }
        public DbSet<Nota> Notas { get; set; }
        public DbSet<Calendario> Calendario { get; set; }
    }
}

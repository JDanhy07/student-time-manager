using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Timely.Data
{
    // Esta clase SOLO la usan las herramientas de EF Core en tiempo de diseño
    // (Add-Migration, Update-Database, etc.). Nunca se ejecuta en producción.
    // Soluciona el error "Unable to resolve DbContextOptions at design time".
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Esta connection string es solo para migraciones — no afecta appsettings.json
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=Timely;Trusted_Connection=True;MultipleActiveResultSets=true"
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

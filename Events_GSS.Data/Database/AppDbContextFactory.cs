// ChatAndEvents.Data/Database/AppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatAndEvents.Data.Database;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseConfiguredDatabase(
            Environment.GetEnvironmentVariable("ConnectionStrings__ChatAndEventsDB")
            ?? "Server=.;Initial Catalog=ChatAndEventsDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;",
            Environment.GetEnvironmentVariable("DatabaseProvider"),
            Environment.GetEnvironmentVariable("DATABASE_URL"));
        return new AppDbContext(optionsBuilder.Options);
    }
}

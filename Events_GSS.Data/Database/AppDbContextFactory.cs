// ChatAndEvents.Data/Database/AppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatAndEvents.Data.Database;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=.;Initial Catalog=ChatAndEventsDB;" +
            "Integrated Security=True;Encrypt=True;TrustServerCertificate=True;");
        return new AppDbContext(optionsBuilder.Options);
    }
}
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ChatAndEvents.Data.Database;

public static class AppDatabaseConfiguration
{
    public static DbContextOptionsBuilder UseConfiguredDatabase(
        this DbContextOptionsBuilder options,
        string? connectionString,
        string? provider = null,
        string? databaseUrl = null)
    {
        var resolvedConnectionString = FirstNonEmpty(
            connectionString,
            databaseUrl,
            Environment.GetEnvironmentVariable("DATABASE_URL"));

        if (string.IsNullOrWhiteSpace(resolvedConnectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        var usePostgres = IsPostgresProvider(provider) || IsPostgresUrl(resolvedConnectionString);
        if (usePostgres)
        {
            options.UseNpgsql(ToNpgsqlConnectionString(resolvedConnectionString));
        }
        else
        {
            options.UseSqlServer(resolvedConnectionString);
        }

        return options;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static bool IsPostgresProvider(string? provider)
    {
        return provider?.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) == true
            || provider?.Equals("Postgres", StringComparison.OrdinalIgnoreCase) == true
            || provider?.Equals("Npgsql", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool IsPostgresUrl(string connectionString)
    {
        return connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);
    }

    private static string ToNpgsqlConnectionString(string connectionString)
    {
        if (!IsPostgresUrl(connectionString))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? string.Empty),
            Password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? string.Empty),
            SslMode = SslMode.Require,
            TrustServerCertificate = true,
        };

        return builder.ConnectionString;
    }
}

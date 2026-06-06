using Microsoft.AspNetCore.Hosting;

namespace ChatAndEvents.Web.Services;

public interface IUploadStorageService
{
    string GetUploadPath(params string[] segments);

    string GetUploadUrl(params string[] segments);
}

public class UploadStorageService : IUploadStorageService
{
    private readonly string _uploadsRoot;

    public UploadStorageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _uploadsRoot = configuration["Uploads:Root"]
            ?? Environment.GetEnvironmentVariable("UPLOADS_ROOT")
            ?? Path.Combine(environment.WebRootPath, "uploads");

        Directory.CreateDirectory(_uploadsRoot);
    }

    public string GetUploadPath(params string[] segments)
    {
        var pathSegments = new[] { _uploadsRoot }.Concat(segments).ToArray();
        return Path.Combine(pathSegments);
    }

    public string GetUploadUrl(params string[] segments)
    {
        return "/uploads/" + string.Join("/", segments.Select(Uri.EscapeDataString));
    }
}

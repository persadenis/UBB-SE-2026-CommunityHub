using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatModule
{
    public sealed partial class WebShellWindow : Window
    {
        private static readonly Uri WebAppUri = ResolveWebAppUri();
        private static readonly Uri ApiUri = new("http://localhost:5572/");
        private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(35);

        public WebShellWindow()
        {
            InitializeComponent();
            Title = "Community Hub";
            _ = LoadHubAsync();
        }

        private async Task LoadHubAsync()
        {
            if (IsLocalHost(WebAppUri))
            {
                await EnsureLocalServersAsync();
            }
            else
            {
                await WaitForEndpointAsync(WebAppUri);
            }

            HubWebView.Source = WebAppUri;
            HubWebView.Visibility = Visibility.Visible;
            StartupOverlay.Visibility = Visibility.Collapsed;
        }

        private static async Task EnsureLocalServersAsync()
        {
            var root = FindSolutionRoot();
            if (root == null)
            {
                return;
            }

            if (!await IsEndpointAliveAsync(ApiUri))
            {
                StartDotnetRun(root, @"ChatAndEvents.API\ChatAndEvents.API.Server\ChatAndEvents.API.Server.csproj");
                await WaitForEndpointAsync(ApiUri);
            }

            if (!await IsEndpointAliveAsync(WebAppUri))
            {
                StartDotnetRun(root, @"ChatAndEvents.Web\ChatAndEvents.Web.csproj");
                await WaitForEndpointAsync(WebAppUri);
            }
        }

        private static async Task<bool> IsEndpointAliveAsync(Uri uri)
        {
            try
            {
                using var client = new HttpClient { Timeout = ProbeTimeout };
                using var response = await client.GetAsync(uri);
                return response != null;
            }
            catch
            {
                return false;
            }
        }

        private static async Task WaitForEndpointAsync(Uri uri)
        {
            var deadline = DateTime.UtcNow + StartupTimeout;
            while (DateTime.UtcNow < deadline)
            {
                if (await IsEndpointAliveAsync(uri))
                {
                    return;
                }

                await Task.Delay(1000);
            }
        }

        private static void StartDotnetRun(string root, string projectRelativePath)
        {
            var projectPath = Path.Combine(root, projectRelativePath);
            if (!File.Exists(projectPath))
            {
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = ResolveDotnetPath(),
                Arguments = $"run --project \"{projectPath}\" --launch-profile https",
                WorkingDirectory = root,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
            _ = Process.Start(startInfo);
        }

        private static string ResolveDotnetPath()
        {
            var localDotnet = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft",
                "dotnet",
                "dotnet.exe");

            return File.Exists(localDotnet) ? localDotnet : "dotnet";
        }

        private static Uri ResolveWebAppUri()
        {
            var configuredUrl = Environment.GetEnvironmentVariable("COMMUNITYHUB_WEB_URL");
            if (!string.IsNullOrWhiteSpace(configuredUrl) &&
                Uri.TryCreate(configuredUrl, UriKind.Absolute, out var configuredUri))
            {
                return configuredUri;
            }

            return new Uri("https://communityhub-web-persadenis.onrender.com/Auth/Login");
        }

        private static bool IsLocalHost(Uri uri)
        {
            return uri.IsLoopback ||
                uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
        }

        private static string? FindSolutionRoot()
        {
            foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            {
                var root = FindSolutionRoot(start);
                if (root != null)
                {
                    return root;
                }
            }

            return null;
        }

        private static string? FindSolutionRoot(string startPath)
        {
            var directory = new DirectoryInfo(startPath);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ChatAndEvents.Web", "ChatAndEvents.Web.csproj")) &&
                    File.Exists(Path.Combine(directory.FullName, "ChatAndEvents.API", "ChatAndEvents.API.Server", "ChatAndEvents.API.Server.csproj")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }
}

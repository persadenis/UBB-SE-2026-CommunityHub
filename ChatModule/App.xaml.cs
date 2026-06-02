using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.Database;
using ChatModule.src.views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace ChatModule
{
    public partial class App : Application
    {
        public static Window? MainAppWindow { get; private set; }
        private Window? _window;
        private LoginWindow? _loginWindow;
        
        public static void SetMainWindow(Window window)
        {
            MainAppWindow = window;
        }

        [ExcludeFromCodeCoverage]
        public App()
        {
            InitializeComponent();
            UnhandledException += OnUnhandledException;
        }

        [ExcludeFromCodeCoverage]
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var services = new ServiceCollection();
            var baseAddress = new Uri("http://172.30.250.53/");
            
            services.AddHttpClient<IAuthenticationService, AuthenticationHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            var provider = services.BuildServiceProvider();
            var authService = provider.GetRequiredService<IAuthenticationService>();

            _loginWindow = new LoginWindow(authService);
            _loginWindow.LoginSucceeded += OnLoginSucceededAsync;

            _window = _loginWindow;
            MainAppWindow = _window;
            _window.Activate();
        }

        [ExcludeFromCodeCoverage]
        private Task OnLoginSucceededAsync(Guid userId, string username)
        {
            try
            {
                var mainWindow = new MainWindow(userId, username);
                MainAppWindow = mainWindow;
                _window = mainWindow;
                mainWindow.Activate();
                _loginWindow?.DispatcherQueue.TryEnqueue(() => _loginWindow.Close());
            }
            catch (Exception ex)
            {
                LogException("LoginSuccessTransition", ex.ToString());
                if (_loginWindow != null)
                {
                    _loginWindow.ViewModel.ErrorMessage = "Failed to open main window. See crash log.";
                }
            }

            return Task.CompletedTask;
        }

        [ExcludeFromCodeCoverage]
        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            LogException("UnhandledException", e.Exception?.ToString() ?? e.Message);
            e.Handled = true;
        }

        internal static string BuildCrashEntry(string source, string details, DateTime timestamp)
        {
            return $"[{timestamp:yyyy-MM-dd HH:mm:ss}] {source}{Environment.NewLine}{details}{Environment.NewLine}{new string('-', 80)}{Environment.NewLine}";
        }

        internal static bool TryAppendCrashLog(string directory, string source, string details)
        {
            try
            {
                Directory.CreateDirectory(directory);
                var filePath = Path.Combine(directory, "crash.log");
                var entry = BuildCrashEntry(source, details, DateTime.Now);
                File.AppendAllText(filePath, entry);
                Debug.WriteLine(entry);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void LogException(string source, string details)
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChatModule");
            _ = TryAppendCrashLog(directory, source, details);
        }
    }
}
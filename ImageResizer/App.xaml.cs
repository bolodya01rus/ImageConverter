using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Threading;

using CommunityToolkit.WinUI.Notifications;

using ImageResizer.Activation;
using ImageResizer.Contracts.Activation;
using ImageResizer.Contracts.Services;
using ImageResizer.Contracts.Views;
using ImageResizer.Models;
using ImageResizer.Properties;
using ImageResizer.Services;
using ImageResizer.ViewModels;
using ImageResizer.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ResourceManager = System.Resources.ResourceManager;

using Windows.ApplicationModel.Resources.Core;

namespace ImageResizer;

// For more information about application lifecycle events see https://docs.microsoft.com/dotnet/framework/wpf/app-development/application-management-overview

// WPF UI elements use language en-US by default.
// If you need to support other cultures make sure you add converters and review dates and numbers in your UI to ensure everything adapts correctly.
// Tracking issue for improving this is https://github.com/dotnet/wpf/issues/1946
public partial class App : System.Windows.Application
{
    public static List<string> PathForResize;
    private IHost _host;

    public T GetService<T>()
        where T : class
        => _host.Services.GetService(typeof(T)) as T;

    public App()
    {
        
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {

        var outputPath = $@"C:\Users\{Environment.UserName}\Pictures\ImageConverter";
        if (string.IsNullOrEmpty(ImageResizer.Properties.Settings.Default.PathSaveFile))
        {
            ImageResizer.Properties.Settings.Default.PathSaveFile = outputPath;
            ImageResizer.Properties.Settings.Default.Save();
        }

        if (e.Args.Length > 0)
        {
            PathForResize = new();
            foreach (var item in e.Args)
            {
                PathForResize.Add(item);               
            }
        }

        // https://docs.microsoft.com/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop
        ToastNotificationManagerCompat.OnActivated += (toastArgs) =>
        {
            Current.Dispatcher.Invoke(async () =>
            {
                var config = GetService<IConfiguration>();
                config[ToastNotificationActivationHandler.ActivationArguments] = toastArgs.Argument;
                await _host.StartAsync();
            });
        };

        // TODO: Register arguments you want to use on App initialization
        var activationArgs = new Dictionary<string, string>
        {
            { ToastNotificationActivationHandler.ActivationArguments, string.Empty }
        };
        var appLocation = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        // For more information about .NET generic host see  https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0
        _host = Host.CreateDefaultBuilder(e.Args)
                .ConfigureAppConfiguration(c =>
                {
                    c.SetBasePath(appLocation);
                    c.AddInMemoryCollection(activationArgs);
                })
                .ConfigureServices(ConfigureServices)

                .Build();

        if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
        {
            // ToastNotificationActivator code will run after this completes and will show a window if necessary.
            return;
        }

        await _host.StartAsync();
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // TODO: Register your services, viewmodels and pages here

        // App Host
        services.AddHostedService<ApplicationHostService>();

        // Activation Handlers
        services.AddSingleton<IActivationHandler, ToastNotificationActivationHandler>();

        // Core Services

        // Services
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton<ISystemService, SystemService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<IToastNotificationsService, ToastNotificationsService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IImageResizeServices, ImageResizeServices>();
        services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        services.AddSingleton<IFileService, FileService>();

        // Views and ViewModels
        services.AddTransient<IShellWindow, ShellWindow>();
        services.AddTransient<ShellViewModel>();

        services.AddTransient<ResizeViewModel>();
        services.AddTransient<ResizePage>();

        services.AddTransient<RotateViewModel>();
        services.AddTransient<RotatePage>();

        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();

        services.AddTransient<ConverterViewModel>();
        services.AddTransient<ConverterPage>();

        services.AddTransient<PdfConverterViewModel>();
        services.AddTransient<PdfConverterPage>();

        services.AddTransient<Convert2PdfViewModel>();
        services.AddTransient<Convert2PdfPage>();

        // Configuration
        services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        _host = null;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // TODO: Please log and handle the exception as appropriate to your scenario
        // For more info see https://docs.microsoft.com/dotnet/api/system.windows.application.dispatcherunhandledexception?view=netcore-3.0
    }
}

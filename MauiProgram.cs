using LocationHeatMap.Services;
using LocationHeatMap.ViewModels;
using LocationHeatMap;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Xaml;
using LocationHeatMap.Converters;

namespace LocationHeatMap
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiMaps() // Add Maps capability
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register services
            builder.Services.AddSingleton<DatabaseService>(s =>
                new DatabaseService(App.DatabasePath));

            builder.Services.AddSingleton<LocationService>();

            // Add navigation service with API key
            builder.Services.AddSingleton<NavigationService>(s =>
                new NavigationService("YOUR_API_KEY_HERE"));

            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();

            // Register converters as resources
            //Application.Current.Resources.Add("BoolToColorConverter", new BoolToColorConverter());

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
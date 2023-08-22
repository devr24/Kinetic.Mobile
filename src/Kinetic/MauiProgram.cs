using CommunityToolkit.Maui;
using Kinetic.Presentation.Views;
using Microsoft.Extensions.Logging;

namespace Kinetic;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
        	.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			}); 
        
        builder.Services.AddTransient<MainPage>();

#if __ANDROID__
		builder.Services.AddTransient<IServiceTest, DemoServices>();
#endif


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
	}
}

using Kinetic.Presentation.Controls;
using MauiDemo.Presentation.Data;

namespace Kinetic;

public partial class App : Application
{
    public App()
	{
		InitializeComponent();

        Task.Run(async () => { 
            Database.Instance = new Database();
            await Database.Instance.InitializeKinetictTables();
        });

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(CustomEntry),
			(handler, view) =>
			{
#if __ANDROID__
			handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif __IOS__
				handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
				handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
			} );
		
        MainPage = new AppShell();
    }
}

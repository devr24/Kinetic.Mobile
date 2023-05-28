using CommunityToolkit.Maui.Storage;
using Kinetic.Presentation.Data.Entities;
using Microsoft.Maui.Controls.PlatformConfiguration;
using static System.Net.Mime.MediaTypeNames;

namespace Kinetic.Presentation.Views;

public partial class DataExportPage : ContentPage
{
    private List<SessionEntity> _sessions;

	public DataExportPage()
	{
		InitializeComponent();

	}

    protected override async void OnAppearing()
    {
        _sessions = await Database.Instance.GetAsync<SessionEntity>();
        var failed = _sessions.Count(t => !t.Completed);
        var label = $"{_sessions.Count}";

        if (failed > 0)
        {

            label += $" ({failed} failed)";
        }

        label_session.Text = label;
        SetLastExportLabel();
        base.OnAppearing();
    }

    private void SetLastExportLabel()
    {
        label_last_exports.Text = Preferences.Get("LastExport", "N/A");
    }

    private async void button_expert_Tapped(object sender, TappedEventArgs e)
    {
        ActivityIndicator activityIndicator = new ActivityIndicator
        {
            IsRunning = true,
            Color = Colors.Turquoise
        };
        

        var sessionData = await Database.Instance.GetAsync<SessionDataEntity>();
        var path = FileSystem.Current.AppDataDirectory;

        var dt = DateTime.Now;

        // Write the file content to the app data directory  
        string targetFile = Path.Combine(path, $"kinetic_export_{dt:HHmmddMMyy}.csv");
        using FileStream outputStream = File.OpenWrite(targetFile);
        using StreamWriter streamWriter = new StreamWriter(outputStream);




        await streamWriter.WriteLineAsync("SessionId,DistanceMovedInKm,AccelerometerX,AccelerometerY,AccelerometerZ,GeoLongitude,GeoLatitude,GeoCourse" +
                                          ",GeoSpeed,GeoAltitude,GeoVerticalAccuracy,GeoAccuracy,GeoReducedAccuracy,GeoAltitudeReferenceSystem");
        var counter = 0;
        foreach (var item in sessionData)
        {
            if (counter > 100)
                await streamWriter.FlushAsync();

            var line =
                $"{item.SessionId},{item.DistanceMovedInKm},{item.AccelerometerX},{item.AccelerometerY},{item.AccelerometerZ}" +
                $",{item.GeoLongitude},{item.GeoLatitude},{item.GeoCourse},{item.GeoSpeed},{item.GeoAltitude},{item.GeoVerticalAccuracy}" +
                $",{item.GeoAccuracy},{item.GeoReducedAccuracy},{item.GeoAltitudeReferenceSystem}";
            
            await streamWriter.WriteLineAsync(line);
            counter++;
        }
        await streamWriter.FlushAsync();
        streamWriter.Close();
        outputStream.Close();

        activityIndicator.IsRunning = false;

        Preferences.Set("LastExport", DateTime.Now.ToString("dd/MM/yy HH:mm"));

        await DisplayAlert("Export Complete", $"Csv exported to {targetFile}", "OK");
        SetLastExportLabel();
    }

    private async void button_clear_data_Tapped(object sender, TappedEventArgs e)
    {

        var confirm = await DisplayAlert("Clear Data", "Please confirm you'd like to clear all device data?", "OK", "Cancel");

        if (confirm)
        {
            await Database.Instance.PurgeAsync<SessionDataEntity>();
            await Database.Instance.PurgeAsync<SessionEntity>();
        }


        label_session.Text = "0";
    }

    public async Task WriteFile(string filename, string content)
    {
        var downloadsDirectory = FileSystem.AppDataDirectory; // as a fallback

        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // as of iOS 10, Apple restricts access to certain directories.
            // for the purpose of this example, let's use the Documents directory.
            downloadsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library");
        }
        // for other platforms, you can add appropriate directory retrieval code

        var filePath = Path.Combine(downloadsDirectory, filename);

        using (var streamWriter = new StreamWriter(filePath, true))
        {
            await streamWriter.WriteLineAsync(content);
        }
    }

}
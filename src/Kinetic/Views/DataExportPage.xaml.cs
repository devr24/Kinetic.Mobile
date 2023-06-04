using Kinetic.Presentation.Data.Entities;

namespace Kinetic.Presentation.Views;

public partial class DataExportPage : ContentPage
{
    private List<SessionEntity> _sessions;
    private bool _isExporting;
    private bool _isClearing;

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

    private async void button_export_Tapped(object sender, TappedEventArgs e)
    {
        if (_isClearing)
        {
            await DisplayAlert("Export Data", "Can't export data will clearing", "OK");
            return;
        }

        if (_isExporting) return;

        _isExporting = true;

        ActivityIndicatorExport.IsRunning = true;
        var fileName = $"kinetic_export_{DateTime.Now:HHmmddMMyy}.csv";
        var targetFile = Path.Combine("/storage/emulated/0/Download", fileName);

        try
        {
            // IBlobStorage _storage = new BlobStorage(new ConnectionConfig("DefaultEndpointsProtocol=https;AccountName=eswplayground;AccountKey=KV0isNedMfhcRehXIEK9N8pSvdgSZgcYHwVu8Dmu5Cjr7lAQf5HyxH18reR7Pd4+bTD+b9EKk/Ky+AStFmfnMw==;EndpointSuffix=core.windows.net\r\n\r\n"));

            var sessionData = await Database.Instance.GetAsync<SessionDataEntity>();
            await using FileStream outputStream = File.OpenWrite(targetFile);
            await using StreamWriter streamWriter = new StreamWriter(outputStream);

            await streamWriter.WriteLineAsync("SessionId,DistanceMovedInKm,AccelerometerX,AccelerometerY,AccelerometerZ,AngularVelocityX,AngularVelocityY,AngularVelocityZ,GeoLongitude,GeoLatitude,GeoCourse," +
                                              "GeoSpeed,GeoAltitude,GeoVerticalAccuracy,GeoAccuracy,GeoReducedAccuracy,GeoAltitudeReferenceSystem");
            var counter = 0;
            foreach (var item in sessionData)
            {
                if (counter > 100)
                    await streamWriter.FlushAsync();

                var line =
                    $"{item.SessionId},{item.DistanceMovedInKm},{item.AccelerometerX},{item.AccelerometerY},{item.AccelerometerZ}" +
                    $",{item.AngularVelocityX},{item.AngularVelocityY},{item.AngularVelocityZ}" +
                    $",{item.GeoLongitude},{item.GeoLatitude},{item.GeoCourse},{item.GeoSpeed},{item.GeoAltitude},{item.GeoVerticalAccuracy}" +
                    $",{item.GeoAccuracy},{item.GeoReducedAccuracy},{item.GeoAltitudeReferenceSystem}";

                await streamWriter.WriteLineAsync(line);
                counter++;
            }
            await streamWriter.FlushAsync();
            streamWriter.Close();
            outputStream.Close();

            // await _storage.UploadBlob(targetFile,$"/POC/{fileName}");

            Preferences.Set("LastExport", DateTime.Now.ToString("dd/MM/yy HH:mm"));
            await DisplayAlert("Export Complete", $"Csv exported to {targetFile}", "OK");
            SetLastExportLabel();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Export Failed", $"Csv attempted to export to {targetFile}. Please try again: {ex.Message}", "OK");
        }
        finally
        {
            ActivityIndicatorExport.IsRunning = false;
            _isExporting = false;
        }
    }

    private async void button_clear_data_Tapped(object sender, TappedEventArgs e)
    {
        if (_isExporting)
        {
            await DisplayAlert("Clear Data", "Can't clear data will exporting", "OK");
            return;
        }

        if (_isClearing) return;
        _isClearing = true;

        var confirm = await DisplayAlert("Clear Data", "Please confirm you'd like to clear all device data?", "OK", "Cancel");

        if (confirm)
        {
            ActivityIndicatorClear.IsRunning = true;
            
            Task myTask = Task.Run(async () =>
            {
                try
                {
                    await Database.Instance.PurgeAsync<SessionDataEntity>();
                    await Database.Instance.PurgeAsync<SessionEntity>();

                    await MainThread.InvokeOnMainThreadAsync(() => { label_session.Text = "0"; });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occurred: " + ex.Message);
                    await MainThread.InvokeOnMainThreadAsync(() => DisplayAlert("Clear Failed", $"Please try again: {ex.Message}", "OK"));
                }
            }).ContinueWith(async task =>
            {
                Console.WriteLine("Task has completed");
                // Insert additional code to run after the Task completes here.
                await MainThread.InvokeOnMainThreadAsync(() => ActivityIndicatorClear.IsRunning = false);
                _isClearing = false;
            });

            await myTask;
        }

    }

    public async Task WriteFile(string filename, string content)
    {
        //var docsDirectory = Envi.GetExternalFilesDir(Environment.DirectoryDcim);
        var downloadsDirectory = "/storage/emulated/0/Download"; // FileSystem.AppDataDirectory; // as a fallback

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
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using Kinetic.Presentation.Data.Entities;
using Kinetic.Presentation.Models;
using Kinetic.Presentation.Services;
using Timer = System.Timers.Timer;

namespace Kinetic.Presentation.Views;

public partial class MainPage : ContentPage
{
    private const double IntervalMilliseconds = 500;
    private const int TrackingDataBatchSize = 100;

    private readonly Timer _sessionTimer = new(IntervalMilliseconds);
    private readonly Stopwatch _totalTime = new();
    private readonly DistanceTracker _tracker = new();
    private readonly Queue<TrackingData> _trackingData = new();

    private SessionDataModel _currentSession;
    private double _totalDistance;

    public MainPage()
	{
		InitializeComponent();
        
        _sessionTimer.Elapsed += OnSessionTimer_Elapsed;
        _tracker.TrackingCaptured += _tracker_TrackingCaptured;
    }


    private async Task SaveSessionInDb(SessionDataModel session)
    {
        await Database.Instance.SaveAsync(new SessionEntity
        {
            SessionId = session.SessionId,
            TotalTime = session.TotalTime,
            DistanceTravelledKm = session.DistanceTravelledKm,
            SessionStarted = session.SessionStarted,
            Completed = session.IsComplete
        });
    }

    private async Task StoreTrackedData(List<SessionDataEntity> data)
    {
        if (data.Any())
        {
            await Database.Instance.SaveBatchAsync(data);
        }
    }

    private async void _tracker_TrackingCaptured(object sender, DistanceTrackingEventArgs e)
    {
        _trackingData.Enqueue(e.TrackingData);
        _totalDistance += e.TrackingData.DistanceMoved;

        if (_trackingData.Count > TrackingDataBatchSize)
        {
            var events = GetTrackingDataBatch();

            // save data!
            await StoreTrackedData(events);
        }
    }

    private List<SessionDataEntity> GetTrackingDataBatch()
    {
        var batchSize = TrackingDataBatchSize;
        if (batchSize > _trackingData.Count)
        {
            batchSize = _trackingData.Count;
        }

        var trackedEvents = new List<SessionDataEntity>();
        for (int i = 0; i < batchSize; i++)
        {
            var record = _trackingData.Dequeue();

            var sde = new SessionDataEntity
            {
                Id = Guid.NewGuid(),
                SessionId = _currentSession.SessionId,
                DistanceMovedInKm = record.DistanceMoved,
                AccelerometerX = record.AccelerometerReading.X,
                AccelerometerY = record.AccelerometerReading.Y,
                AccelerometerZ = record.AccelerometerReading.Z
            };
            if (record.GeoLocation != null)
            {
                sde.GeoAccuracy = record.GeoLocation?.Accuracy;
                sde.GeoAltitude = record.GeoLocation?.Altitude;
                sde.GeoAltitudeReferenceSystem = record.GeoLocation?.AltitudeReferenceSystem.ToString();
                sde.GeoCourse = record.GeoLocation?.Course;
                sde.GeoLatitude = record.GeoLocation!.Latitude;
                sde.GeoLongitude = record.GeoLocation.Longitude;
                sde.GeoReducedAccuracy = record.GeoLocation.ReducedAccuracy;
                sde.GeoSpeed = record.GeoLocation?.Speed;
                sde.GeoVerticalAccuracy = record.GeoLocation?.VerticalAccuracy;
            }

            trackedEvents.Add(sde);
        }
        return trackedEvents;
    }

    private async void OnSessionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        var ts = _totalTime.Elapsed;
        var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}"; //,ts.Milliseconds / 10);

        _currentSession.TotalTime = ts;
        _currentSession.DistanceTravelledKm = _totalDistance;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            label_time.Text = elapsedTime;
            label_distance.Text = $"{_totalDistance:0.0} km";
        });
    }

    private async void SessionButtonClick(object sender, TappedEventArgs e)
    {
        if (!_sessionTimer.Enabled)
        {
            _currentSession = new SessionDataModel(DateTime.Now);
            _sessionTimer.Start();
            _totalTime.Start();
            _tracker.StartTracking();
            _totalDistance = 0;
            label_time.Text = "00:00:00";
            lbl_SessionStartStop.Text = "Stop Session";
            await SaveSessionInDb(_currentSession);
        }
        else
        {
            _sessionTimer.Stop();
            _totalTime.Stop();

            var confirm = await DisplayAlert("Finish Session?", "Please confirm you'd like to finish the session?", "OK", "Cancel");

            if (confirm)
            {
                _tracker.StopTracking();
                _totalTime.Reset();
                _currentSession.IsComplete = true;

                var events = GetTrackingDataBatch();

                // save data!
                await StoreTrackedData(events);
                await SaveSessionInDb(_currentSession);

                lbl_SessionStartStop.Text = "Start Session";
                this.ShowPopup(new SessionSummary(_currentSession));
            }
            else
            {
                _sessionTimer.Start();
                _totalTime.Start();
            }
        }
    }
}


//await Navigation.PushModalAsync(new SessionSummary(_currentSession), true);
//_ = Task.Delay(500).ContinueWith(async (_) => {
//    await MainThread.InvokeOnMainThreadAsync(() => );
//});
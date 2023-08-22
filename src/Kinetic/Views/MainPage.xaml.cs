using System.Diagnostics;
#if __ANDROID__
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.Annotations;
using AndroidX.Core.App;
//using SystemConfiguration;
#endif
using CommunityToolkit.Maui.Views;
using Kinetic.Presentation.Data.Entities;
using Kinetic.Presentation.Models;
using Kinetic.Presentation.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.PlatformConfiguration;
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

    private readonly IServiceTest _services;

    public MainPage(IServiceTest services)
    {
        InitializeComponent();
        _services = services;
        _sessionTimer.Elapsed += OnSessionTimer_Elapsed;
        _tracker.TrackingCaptured += _tracker_TrackingCaptured;

        GetUserData().GetAwaiter().GetResult();
    }

    private async Task GetUserData()
    {
        var userName = Preferences.Get("UserName", null);
        if (string.IsNullOrEmpty(userName))
        {
            var firstTimeUsePg = new LoginPage();
            await Navigation.PushModalAsync(firstTimeUsePg, true);
        }
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
        for (int i = 0; i < e.TrackingData.Count; i++)
        {
            var item = e.TrackingData[i];
            _trackingData.Enqueue(item);
            _totalDistance += item.DistanceMoved;
        }


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
                AccelerometerZ = record.AccelerometerReading.Z,
                AngularVelocityX = record.AngularVelocity.X,
                AngularVelocityY = record.AngularVelocity.Y,
                AngularVelocityZ = record.AngularVelocity.Z
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
            label_distance.Text = $"{_totalDistance:00.0} km";
        });
    }

    private async void SessionButtonClick(object sender, TappedEventArgs e)
    {
        if (!_sessionTimer.Enabled)
        {
            var sensorSpeed = (SensorSpeed)Enum.Parse(typeof(SensorSpeed), Preferences.Get("UserSensorSpeed", "Fastest"));
            _currentSession = new SessionDataModel(DateTime.Now);
            _sessionTimer.Start();
            _totalTime.Start();
            _tracker.StartTracking(sensorSpeed);
            _totalDistance = 0;
            label_time.Text = "00:00:00";
            lbl_SessionStartStop.Text = "Stop Session";
            _services.Start();
            await SaveSessionInDb(_currentSession);

            
        }
        else
        {
            _sessionTimer.Stop();
            _totalTime.Stop();

            var confirm = await DisplayAlert("Complete Session", "Please confirm you'd like to complete the session?", "OK", "Cancel");

            if (confirm)
            {
                _tracker.StopTracking();
                _totalTime.Reset();
                _currentSession.IsComplete = true;

                var events = GetTrackingDataBatch();

                // save data!
                await StoreTrackedData(events);
                await SaveSessionInDb(_currentSession);

                _services.Stop();
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
public interface IServiceTest
{
    void Start();
    void Stop();
}

#if __ANDROID__

[Service]
public class DemoServices : Service, IServiceTest //we implement our service (IServiceTest) and use Android Native Service Class
{
   public override IBinder OnBind(Intent intent)
   {
      throw new NotImplementedException();
   }
   
   [return: GeneratedEnum]//we catch the actions intents to know the state of the foreground service
   public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
   {
      if (intent.Action == "START_SERVICE")
      {
         RegisterNotification();//Proceed to notify
      }
      else if (intent.Action == "STOP_SERVICE")
      {
         StopForeground(true);//Stop the service
         StopSelfResult(startId);
      }
      
      return StartCommandResult.NotSticky;
    }
    
    //Start and Stop Intents, set the actions for the MainActivity to get the state of the foreground service
    //Setting one action to start and one action to stop the foreground service
    public void Start()
    {
        Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(DemoServices));
        startService.SetAction("START_SERVICE");
        MainActivity.ActivityCurrent.StartService(startService);
    }
    
    public void Stop()
    {
        Intent stopIntent = new Intent(MainActivity.ActivityCurrent, this.Class);
        stopIntent.SetAction("STOP_SERVICE");
        MainActivity.ActivityCurrent.StartService(stopIntent);
    }
    
    private void RegisterNotification()
    {
        NotificationChannel channel = new NotificationChannel("ServiceChannel", "ServiceDemo", NotificationImportance.Max);
        NotificationManager manager = (NotificationManager)MainActivity.ActivityCurrent.GetSystemService(Context.NotificationService);
        manager.CreateNotificationChannel(channel);
        Notification notification = new Notification.Builder(this, "ServiceChannel")
         .SetContentTitle("Kinetic Service Running")
         .SetSmallIcon(Resource.Drawable.abc_ab_share_pack_mtrl_alpha)
         .SetOngoing(true)
         .Build();

        StartForeground(100, notification);
    }
}


[Service(Label = nameof(ScreenOffService))]
[RequiresApi(Api = (int)BuildVersionCodes.R)]
public class ScreenOffService : Service
{
    private static readonly string TypeName = typeof(ScreenOffService).FullName;
    public static readonly string ActionStartScreenOffService = TypeName + ".action.START";

    internal const int NOTIFICATION_ID = 12345678;
    private const string NOTIFICATION_CHANNEL_ID = "screen_off_service_channel_01";
    private const string NOTIFICATION_CHANNEL_NAME = "screen_off_service_channel_name";
    private NotificationManager _notificationManager;

    private bool _isStarted;

    private readonly ScreenOffBroadcastReceiver _screenOffBroadcastReceiver;

    public ScreenOffService()
    {
        _screenOffBroadcastReceiver = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetService<ScreenOffBroadcastReceiver>();
    }

    public override void OnCreate()
    {
        base.OnCreate();

        _notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

        RegisterScreenOffBroadcastReceiver();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        UnregisterScreenOffBroadcastReceiver();
    }

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        CreateNotificationChannel(); // Elsewhere we must've prompted user to allow Notifications

        if (intent.Action == ActionStartScreenOffService)
        {
            try
            {
                StartForeground();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to start Screen On/Off foreground svc: " + ex);
            }
        }

        return StartCommandResult.Sticky;
    }

    private void RegisterScreenOffBroadcastReceiver()
    {
        var filter = new IntentFilter();
        filter.AddAction(Intent.ActionScreenOff);
        RegisterReceiver(_screenOffBroadcastReceiver, filter);
    }

    private void UnregisterScreenOffBroadcastReceiver()
    {
        try
        {
            if (_screenOffBroadcastReceiver != null)
            {
                UnregisterReceiver(_screenOffBroadcastReceiver);
            }
        }
        catch (Java.Lang.IllegalArgumentException ex)
        {
            Console.WriteLine($"Error while unregistering {nameof(ScreenOffBroadcastReceiver)}. {ex}");
        }
    }

    private void StartForeground()
    {
        if (!_isStarted)
        {
            Notification notification = BuildInitialNotification();
            StartForeground(NOTIFICATION_ID, notification);

            _isStarted = true;
        }
    }

    private Notification BuildInitialNotification()
    {
        var intentToShowMainActivity = BuildIntentToShowMainActivity();

        var notification = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID)
            .SetContentTitle("Kinetic")
            .SetContentText("Session Running")
            .SetSmallIcon(Resource.Drawable.timergray) // Android top bar icon and Notification drawer item LHS icon
            .SetLargeIcon(global::Android.Graphics.BitmapFactory.DecodeResource(Resources, Resource.Drawable.timergray)) // Notification drawer item RHS icon
            .SetContentIntent(intentToShowMainActivity)
            .SetOngoing(true)
            .Build();

        return notification;
    }

    private PendingIntent BuildIntentToShowMainActivity()
    {
        var mainActivityIntent = new Intent(this, typeof(MainActivity));
        mainActivityIntent.SetAction("MainView");
        mainActivityIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
       // mainActivityIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);

        PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, mainActivityIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        return pendingIntent;
    }

    private void CreateNotificationChannel()
    {
        NotificationChannel chan = new(NOTIFICATION_CHANNEL_ID, NOTIFICATION_CHANNEL_NAME, NotificationImportance.Default)
        {
            LightColor = Microsoft.Maui.Graphics.Color.FromRgba(0, 0, 255, 0).ToInt(),
            LockscreenVisibility = NotificationVisibility.Public
        };

        _notificationManager.CreateNotificationChannel(chan);
    }

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }
}

[BroadcastReceiver(Name = "com.kineticworks.MobileApp.ScreenOffBroadcastReceiver", Label = "ScreenOffBroadcastReceiver", Exported = true)]
[IntentFilter(new[] { Intent.ActionScreenOff }, Priority = (int)IntentFilterPriority.HighPriority)]
public class ScreenOffBroadcastReceiver : BroadcastReceiver
{
    private readonly ILogger<ScreenOffBroadcastReceiver> _logger;

    private PowerManager.WakeLock _wakeLock;

    public ScreenOffBroadcastReceiver()
    {
        _logger = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetService<ILogger<ScreenOffBroadcastReceiver>>();
    }

    public override void OnReceive(Context context, Intent intent)
    {
        if (intent.Action == Intent.ActionScreenOff)
        {
            AcquireWakeLock();
        }
    }

    private void AcquireWakeLock()
    {
        _wakeLock?.Release();

        WakeLockFlags wakeFlags = WakeLockFlags.Partial;

        PowerManager pm = (PowerManager)global::Android.App.Application.Context.GetSystemService(global::Android.Content.Context.PowerService);
        _wakeLock = pm.NewWakeLock(wakeFlags, typeof(ScreenOffBroadcastReceiver).FullName);
        _wakeLock.Acquire();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _wakeLock?.Release();
    }
}
#endif


//await Navigation.PushModalAsync(new SessionSummary(_currentSession), true);
//_ = Task.Delay(500).ContinueWith(async (_) => {
//    await MainThread.InvokeOnMainThreadAsync(() => );
//});
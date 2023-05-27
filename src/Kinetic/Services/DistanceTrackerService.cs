using System.Numerics;
using Kinetic.Presentation.Filters;

namespace Kinetic.Presentation.Services
{
    public class DistanceTrackingEventArgs 
    {
        public TrackingData TrackingData { get; set; }
    }

    public class TrackingData
    {
        public Vector3 AccelerometerReading { get; set; }
        public double DistanceMoved { get; set; }
        public Location GeoLocation { get; set; }
    }

    public class DistanceTracker
    {
        private Location _previousLocation;
        private double _distance;
        private Vector3 _accelerometerData;
        private bool _trackingEnabled;
        private bool _isTracking;
        private readonly double _accelerometerThreshold = 0.15;  // You might need to adjust this value for your needs
        private DateTime _lastReadingTime = DateTime.MinValue;
        private readonly TimeSpan _readingInterval = TimeSpan.FromMilliseconds(500);
        private readonly LowPassFilter _lowPassFilter = new (0.9);
        private Timer _timer;
        public event EventHandler<DistanceTrackingEventArgs> TrackingCaptured;

        public void StartTracking()
        {
            if (_trackingEnabled) return;
            
            // Register for accelerometer changes, speed = fastest
            _distance = 0;
            _trackingEnabled = true;
            _timer = new Timer(Callback,null,1000,1000);
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Start(SensorSpeed.Default);
        }

        private void Callback(object state)
        {
            TrackingCaptured?.Invoke(null, new DistanceTrackingEventArgs { TrackingData = LastTrackingData});
        }

        public void StopTracking()
        {
            if (! _trackingEnabled) return;

            // Register for accelerometer changes, speed = fastest
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            Accelerometer.Stop();
            _timer?.Dispose();
            _timer = null;
            _trackingEnabled = false;
            _isTracking = false;
        }

        public TrackingData LastTrackingData =>
            new()
            {
                GeoLocation = _previousLocation,
                DistanceMoved = _distance,
                AccelerometerReading = _accelerometerData
            };

        private async void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var now = DateTime.Now;

            // Check if enough time has passed since last reading
            if ((now - _lastReadingTime) > _readingInterval)
            {
                _lastReadingTime = now;

                var data = e.Reading;
                var filteredData = _lowPassFilter.Filter(new Vector3(data.Acceleration.X, data.Acceleration.Y, data.Acceleration.Z));

                // Check if device is in motion
                if (Math.Abs(filteredData.X) > _accelerometerThreshold ||
                    Math.Abs(filteredData.Y) > _accelerometerThreshold ||
                    Math.Abs(filteredData.Z) > _accelerometerThreshold)
                {
                    // Device in motion, start tracking if not already
                    if (!_isTracking)
                    {
                        _accelerometerData = filteredData;
                        _isTracking = true;
                        await TrackDistance();
                    }
                }
                else
                {
                    // Device not in motion, stop tracking
                    _isTracking = false;
                }
            }
        }

        private async Task TrackDistance()
        {
            while (_isTracking)
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Best);
                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {
                        Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

                        if (_previousLocation != null)
                        {
                            _distance = Location.CalculateDistance(_previousLocation, location, DistanceUnits.Kilometers);
                            Console.WriteLine($"Distance: {_distance} km");
                        }

                        _previousLocation = location;
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions here
                }

                // Delay for a bit before getting next reading to save power
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }


}
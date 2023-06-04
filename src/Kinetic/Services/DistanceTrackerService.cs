using System.Numerics;
using Kinetic.Presentation.Filters;

namespace Kinetic.Presentation.Services
{
    public class DistanceTrackingEventArgs 
    {
        public List<TrackingData> TrackingData { get; set; }
    }

    public class TrackingData
    {
        public Vector3 AccelerometerReading { get; set; }
        public double DistanceMoved { get; set; }
        public Location GeoLocation { get; set; }
        public Vector3 AngularVelocity { get; set; }
        public SensorSpeed SensorSpeed { get; set; }
    }

    public class DistanceTracker
    {
        private readonly Queue<TrackingData> _trackingDataQueue = new();
        private SensorSpeed _sensorSpeed;
        private INoiseFilter _filter;
        private Timer _trackingTimer;

        private double _lastDistanceData;
        private Location _lastLocationData;
        private Vector3 _lastAccelerometerData;
        private Vector3 _lastGyroData;

        private bool _isCapturingDistance;
        private bool _isCapturingTracking;

        // private readonly double _accelerometerThreshold = 0.15;  // need to adjust this value for your needs
        //private DateTime _lastLocationReadTime;
        private TimeSpan _locationReadIntervalMs;
        private readonly object _lockObject = new();

        public event EventHandler<DistanceTrackingEventArgs> TrackingCaptured;

        public TrackingData LastTrackingData => new()
        {
            GeoLocation = _lastLocationData,
            DistanceMoved = _lastDistanceData,
            AccelerometerReading = _lastAccelerometerData,
            AngularVelocity = _lastGyroData,
            SensorSpeed = _sensorSpeed
        };

        public void StartTracking(SensorSpeed sensorSpeed, double locationReadIntervalMs = 5000, INoiseFilter filter = null)
        {
            if (_isCapturingTracking) return;

            _filter = filter;
            _sensorSpeed = sensorSpeed;
            _locationReadIntervalMs = TimeSpan.FromMilliseconds(locationReadIntervalMs);

            InitialiseTrackingValues();
        }

        private void InitialiseTrackingValues()
        {
            _isCapturingTracking = true;
            _trackingTimer = new Timer(TrackingTimerCallback, null, 1000, 1000);

            if (Accelerometer.IsSupported && !Accelerometer.IsMonitoring)
            {
                Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Start(_sensorSpeed);
            }
            if (Gyroscope.IsSupported && !Gyroscope.IsMonitoring)
            {
                Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
                Gyroscope.Start(_sensorSpeed);
            }

            TrackDistance().ConfigureAwait(false);
        }

        public void StopTracking()
        {
            if (! _isCapturingTracking) return;

            ResetTrackingValues();
        }

        private void ResetTrackingValues()
        {
            // Register for accelerometer changes, speed = fastest
            if (Accelerometer.IsSupported && Accelerometer.IsMonitoring)
            {
                Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                Accelerometer.Stop();
            }
            if (Gyroscope.IsSupported && Gyroscope.IsMonitoring)
            {
                Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;
                Gyroscope.Stop();
            }

            _trackingTimer?.Dispose();
            _trackingTimer = null;
            _isCapturingTracking = false;
            _isCapturingDistance = false;
            _lastDistanceData = 0;
            _filter = null;
        }

        private void TrackingTimerCallback(object state)
        {
            lock (_lockObject)
            {
                var data = new List<TrackingData>();
                for (int i = 0; i < _trackingDataQueue.Count; i++)
                {
                    data.Add(_trackingDataQueue.Dequeue());
                }
                TrackingCaptured?.Invoke(null, new DistanceTrackingEventArgs { TrackingData = data });
            }
        }

        private void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
        {
            _lastGyroData = e.Reading.AngularVelocity;
            _trackingDataQueue.Enqueue(LastTrackingData);
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            _lastAccelerometerData = e.Reading.Acceleration;
            _trackingDataQueue.Enqueue(LastTrackingData);
        }

        //private async Task GetLocation() 
        //{
        //    var now = DateTime.Now;

        //    // Check if enough time has passed since last reading
        //    if ((now - _lastLocationReadTime) > _locationReadInterval)
        //    {
        //        _lastLocationReadTime = now;
                
        //        Vector3 filteredData = _filter != null ? _filter.Filter(_lastAccelerometerData) : _lastAccelerometerData;

        //        // Check if device is in motion
        //        if (Math.Abs(filteredData.X) > _accelerometerThreshold ||
        //            Math.Abs(filteredData.Y) > _accelerometerThreshold ||
        //            Math.Abs(filteredData.Z) > _accelerometerThreshold)
        //        {
        //            // Device in motion, start tracking if not already
        //            if (!_isCapturingDistance)
        //            {
        //                _lastAccelerometerData = filteredData;
        //                _isCapturingDistance = true;
        //                await TrackDistance();
        //            }
        //        }
        //        else
        //        {
        //            // Device not in motion, stop tracking
        //            _isCapturingDistance = false;
        //        }
        //    }
        //}

        private async Task TrackDistance()
        {
            _isCapturingDistance = true;
            while (_isCapturingDistance)
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Best);
                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null && _lastLocationData != null)
                    {
                        Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                        _lastDistanceData = Location.CalculateDistance(_lastLocationData, location, DistanceUnits.Kilometers);
                        Console.WriteLine($"Distance: {_lastDistanceData} km");
                    }
                    _lastLocationData = location;
                    _trackingDataQueue.Enqueue(LastTrackingData);
                }
                catch (Exception ex)
                {
                    // Handle exceptions here
                    Console.WriteLine($"Error Gathering GeoData: {ex.Message}");
                }

                // Delay for a bit before getting next reading to save power
                await Task.Delay(_locationReadIntervalMs);
            }
        }
    }
}
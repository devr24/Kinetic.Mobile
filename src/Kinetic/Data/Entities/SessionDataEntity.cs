using SQLite;

namespace Kinetic.Presentation.Data.Entities;

public class SessionDataEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public double DistanceMovedInKm { get; set; }
    public float AccelerometerX { get; set; }
    public float AccelerometerY { get; set; }
    public float AccelerometerZ { get; set; }
    public double GeoLatitude { get; set; }
    public double GeoLongitude { get; set; }
    public double? GeoAltitude { get; set; }
    public double? GeoAccuracy { get; set; }
    public double? GeoVerticalAccuracy { get; set; }
    public bool GeoReducedAccuracy { get; set; }
    public double? GeoSpeed { get; set; }
    public double? GeoCourse { get; set; }
    public string GeoAltitudeReferenceSystem { get; set; }

    public SessionDataEntity()
    {
        Id = Guid.NewGuid();
    }
}

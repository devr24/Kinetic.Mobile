using SQLite;

namespace Kinetic.Presentation.Data.Entities;

public class SessionEntity
{
    [PrimaryKey]
    public Guid SessionId { get; set; }

    public DateTime SessionStarted { get; set; }

    public double DistanceTravelledKm { get; set; }

    public TimeSpan TotalTime { get; set; }

    public bool Completed { get; set; }
}

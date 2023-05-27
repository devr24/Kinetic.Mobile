namespace Kinetic.Presentation.Models
{
    public class SessionDataModel
    {
        public Guid SessionId { get; private set; }

        public DateTime SessionStarted { get; set; }

        public double DistanceTravelledKm { get; set; }

        public TimeSpan TotalTime { get; set; }

        public bool IsComplete { get; set; }

        public SessionDataModel(DateTime? sessionStarted = null)
        {
            SessionId = Guid.NewGuid();

            if (sessionStarted != null)
                SessionStarted = sessionStarted.Value;
        }
    }
}

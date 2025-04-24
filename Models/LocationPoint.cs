using SQLite;

namespace LocationHeatMap.Models
{
    public class LocationPoint
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public int Count { get; set; } = 1; // For aggregating nearby points
    }
}
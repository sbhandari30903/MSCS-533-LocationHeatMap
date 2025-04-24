namespace LocationHeatMap.Models
{
    public class NavigationRoute
    {
        public Location StartPoint { get; set; }
        public Location EndPoint { get; set; }
        public string Name { get; set; }
        public List<Location> RoutePoints { get; set; } = new List<Location>();
    }
}
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using LocationHeatMap.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;

namespace LocationHeatMap.Services
{
    public class NavigationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey; // Your API key for mapping service (e.g., Google, MapBox)

        public NavigationService(string apiKey)
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
        }

        public async Task<NavigationRoute> GetRouteAsync(Location start, Location end)
        {
            try
            {
                // For a real implementation, you would call a routing API like Google Directions API
                // This is a simplified example that would need to be replaced with actual API calls
                string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={start.Latitude},{start.Longitude}&destination={end.Latitude},{end.Longitude}&key={_apiKey}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Parse the response - this is simplified and would need to be adjusted based on the actual API response
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                // Create a new route
                var route = new NavigationRoute
                {
                    StartPoint = start,
                    EndPoint = end,
                    Name = $"Route from ({start.Latitude:F2}, {start.Longitude:F2}) to ({end.Latitude:F2}, {end.Longitude:F2})"
                };

                // Extract route points from the response
                // This is just a placeholder for the actual implementation
                if (json.TryGetProperty("routes", out var routes) &&
                    routes.GetArrayLength() > 0 &&
                    routes[0].TryGetProperty("legs", out var legs) &&
                    legs.GetArrayLength() > 0 &&
                    legs[0].TryGetProperty("steps", out var steps))
                {
                    for (int i = 0; i < steps.GetArrayLength(); i++)
                    {
                        if (steps[i].TryGetProperty("start_location", out var startLoc) &&
                            startLoc.TryGetProperty("lat", out var lat) &&
                            startLoc.TryGetProperty("lng", out var lng))
                        {
                            route.RoutePoints.Add(new Location(lat.GetDouble(), lng.GetDouble()));
                        }
                    }
                }

                // For testing/fallback, create a simple straight line route if API didn't return points
                if (route.RoutePoints.Count == 0)
                {
                    // Create a simple route with 10 points from start to end
                    for (int i = 0; i <= 10; i++)
                    {
                        double factor = i / 10.0;
                        double lat = start.Latitude + factor * (end.Latitude - start.Latitude);
                        double lng = start.Longitude + factor * (end.Longitude - start.Longitude);
                        route.RoutePoints.Add(new Location(lat, lng));
                    }
                }

                return route;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting route: {ex.Message}");
                throw;
            }
        }
    }
}

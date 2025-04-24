using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Graphics;
using LocationHeatMap.Models;
using System.Collections.Generic;

namespace LocationHeatMap.Controls
{
    public class HeatMapRenderer : ContentView
    {
        private readonly Microsoft.Maui.Controls.Maps.Map _map;
        private List<LocationPoint> _points;
        private NavigationRoute _currentRoute;
        private Polyline _navigationPolyline;
        private Pin _startPin;
        private Pin _endPin;

        // Bindable property for location points
        public static readonly BindableProperty LocationPointsProperty =
            BindableProperty.Create(nameof(LocationPoints), typeof(List<LocationPoint>), typeof(HeatMapRenderer),
                defaultValue: null, propertyChanged: OnLocationPointsChanged);

        public List<LocationPoint> LocationPoints
        {
            get => (List<LocationPoint>)GetValue(LocationPointsProperty);
            set => SetValue(LocationPointsProperty, value);
        }

        private static void OnLocationPointsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is HeatMapRenderer renderer && newValue is List<LocationPoint> points)
            {
                renderer._points = points;
                renderer.UpdateHeatMap();
            }
        }

        // Bindable property for navigation route
        public static readonly BindableProperty CurrentRouteProperty =
            BindableProperty.Create(nameof(CurrentRoute), typeof(NavigationRoute), typeof(HeatMapRenderer),
                defaultValue: null, propertyChanged: OnCurrentRouteChanged);

        public NavigationRoute CurrentRoute
        {
            get => (NavigationRoute)GetValue(CurrentRouteProperty);
            set => SetValue(CurrentRouteProperty, value);
        }

        private static void OnCurrentRouteChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is HeatMapRenderer renderer && newValue is NavigationRoute route)
            {
                renderer._currentRoute = route;
                renderer.UpdateNavigation();
            }
        }

        public HeatMapRenderer()
        {
            _map = new Microsoft.Maui.Controls.Maps.Map();
            Content = _map;
        }

        public void UpdateHeatMap()
        {
            // Clear all existing heat map elements
            var elementsToRemove = _map.MapElements.Where(e => e != _navigationPolyline).ToList();
            foreach (var element in elementsToRemove)
            {
                _map.MapElements.Remove(element);
            }

            if (_points == null || _points.Count == 0)
                return;

            // Find maximum count for normalization
            int maxCount = _points.Max(p => p.Count);

            foreach (var point in _points)
            {
                // Normalize intensity based on count, 0.0 to 1.0
                double intensity = (double)point.Count / maxCount;

                // Create a circle with transparency and color based on intensity
                var color = GetHeatColor(intensity);
                var circle = new Circle
                {
                    Center = new Location(point.Latitude, point.Longitude),
                    Radius = new Distance(50 + (intensity * 100)), // Size based on intensity
                    StrokeColor = color,
                    StrokeWidth = 1,
                    FillColor = new Color(color.Red, color.Green, color.Blue, (float)0.6), // Semi-transparent
                };

                _map.MapElements.Add(circle);
            }

            // Set the visible region to include all points
            if (_points.Count > 0 && _currentRoute == null) // Only adjust view if not navigating
            {
                var minLat = _points.Min(p => p.Latitude);
                var maxLat = _points.Max(p => p.Latitude);
                var minLon = _points.Min(p => p.Longitude);
                var maxLon = _points.Max(p => p.Longitude);

                var center = new Location((minLat + maxLat) / 2, (minLon + maxLon) / 2);

                _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    center,
                    Distance.FromMeters(Math.Max(
                        Distance.BetweenPositions(
                            new Location(minLat, minLon),
                            new Location(maxLat, maxLon)
                        ).Meters,
                        500 // Minimum distance
                    ))
                ));
            }
        }

        // Helper method to get heat map colors (red for hot, blue for cold)
        private Color GetHeatColor(double intensity)
        {
            if (intensity < 0.25)
                return Colors.Blue;
            else if (intensity < 0.5)
                return Colors.Green;
            else if (intensity < 0.75)
                return Colors.Yellow;
            else
                return Colors.Red;
        }

        public void UpdateNavigation()
        {
            // Clear any existing navigation elements
            if (_navigationPolyline != null)
            {
                _map.MapElements.Remove(_navigationPolyline);
                _navigationPolyline = null;
            }

            if (_startPin != null)
            {
                _map.Pins.Remove(_startPin);
                _startPin = null;
            }

            if (_endPin != null)
            {
                _map.Pins.Remove(_endPin);
                _endPin = null;
            }

            if (_currentRoute == null || _currentRoute.RoutePoints.Count < 2)
                return;

            // Create a polyline for the navigation route
            _navigationPolyline = new Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 6
            };

            // Add all points to the polyline
            foreach (var point in _currentRoute.RoutePoints)
            {
                _navigationPolyline.Geopath.Add(point);
            }

            _map.MapElements.Add(_navigationPolyline);

            // Add start and end markers
            _startPin = new Pin
            {
                Type = PinType.Place,
                Label = "Start",
                Address = "Starting Point",
                Location = _currentRoute.StartPoint
            };

            _endPin = new Pin
            {
                Type = PinType.Place,
                Label = "Destination",
                Address = "End Point",
                Location = _currentRoute.EndPoint
            };

            _map.Pins.Add(_startPin);
            _map.Pins.Add(_endPin);

            // Adjust the map view to show the entire route
            var minLat = _currentRoute.RoutePoints.Min(p => p.Latitude);
            var maxLat = _currentRoute.RoutePoints.Max(p => p.Latitude);
            var minLon = _currentRoute.RoutePoints.Min(p => p.Longitude);
            var maxLon = _currentRoute.RoutePoints.Max(p => p.Longitude);

            var center = new Location((minLat + maxLat) / 2, (minLon + maxLon) / 2);

            // Create a span with some padding
            _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                center,
                Distance.FromKilometers(
                    Math.Max(
                        Distance.BetweenPositions(
                            new Location(minLat, minLon),
                            new Location(maxLat, maxLon)
                        ).Kilometers * 1.2, // Add 20% padding
                        1.0 // Minimum 1km radius
                    )
                )
            ));
        }
    }
}
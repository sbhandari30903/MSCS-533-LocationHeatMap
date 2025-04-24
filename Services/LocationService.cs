using Microsoft.Maui.Devices.Sensors;
using LocationHeatMap.Models;
using System;
using System.Threading.Tasks;

namespace LocationHeatMap.Services
{
    public class LocationService
    {
        private readonly DatabaseService _databaseService;
        private CancellationTokenSource _cancelTokenSource;
        private bool _isCheckingLocation;

        public LocationService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<Location> GetCurrentLocation()
        {
            try
            {
                _isCheckingLocation = true;

                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                _cancelTokenSource = new CancellationTokenSource();

                Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

                if (location != null)
                {
                    // Save location to database
                    await _databaseService.SaveLocationPointAsync(new LocationPoint
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        Timestamp = DateTime.UtcNow
                    });
                }

                return location;
            }
            // We need to catch FeatureNotSupportedException, FeatureNotEnabledException, etc.
            catch (Exception ex)
            {
                // Handle exceptions (log, alert user, etc.)
                Console.WriteLine($"Location error: {ex.Message}");
                return null;
            }
            finally
            {
                _isCheckingLocation = false;
            }
        }

        public void CancelRequest()
        {
            if (_isCheckingLocation && _cancelTokenSource != null && !_cancelTokenSource.IsCancellationRequested)
                _cancelTokenSource.Cancel();
        }
    }
}
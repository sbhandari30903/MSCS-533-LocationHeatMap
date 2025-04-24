using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using LocationHeatMap.Models;
using LocationHeatMap.Services;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.ObjectModel;

namespace LocationHeatMap.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        private readonly LocationService _locationService;
        private readonly DatabaseService _databaseService;

        private NavigationRoute _currentRoute;
        private Location _startLocation;
        private Location _endLocation;
        private bool _isNavigating;
        private bool _isLoading;
        private string _trackingButtonText;
        private bool _isTracking; // Added property backing field
        private ObservableCollection<LocationPoint> _locationPoints;

        public ObservableCollection<LocationPoint> LocationPoints
        {
            get => _locationPoints;
            set
            {
                _locationPoints = value;
                OnPropertyChanged();
            }
        }

        public NavigationRoute CurrentRoute
        {
            get => _currentRoute;
            set
            {
                _currentRoute = value;
                OnPropertyChanged();
            }
        }

        public Location StartLocation
        {
            get => _startLocation;
            set
            {
                _startLocation = value;
                OnPropertyChanged();
            }
        }

        public Location EndLocation
        {
            get => _endLocation;
            set
            {
                _endLocation = value;
                OnPropertyChanged();
            }
        }

        public bool IsNavigating
        {
            get => _isNavigating;
            set
            {
                _isNavigating = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NavigationButtonText));
            }
        }

        public string NavigationButtonText => IsNavigating ? "Stop Navigation" : "Start Navigation";

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string TrackingButtonText
        {
            get => _trackingButtonText;
            set
            {
                if (_trackingButtonText != value)
                {
                    _trackingButtonText = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTracking // Added property
        {
            get => _isTracking;
            set
            {
                if (_isTracking != value)
                {
                    _isTracking = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SetStartLocationCommand { get; }
        public ICommand SetEndLocationCommand { get; }
        public ICommand ToggleNavigationCommand { get; }
        public ICommand ToggleTrackingCommand { get; }

        public ICommand LoadDataCommand { get; }

        public ICommand ClearDataCommand { get; }

        public MainViewModel(LocationService locationService, DatabaseService databaseService, NavigationService navigationService)
        {
            _locationService = locationService;
            _databaseService = databaseService;
            _navigationService = navigationService;

            SetStartLocationCommand = new Command(async () => await SetStartLocation());
            SetEndLocationCommand = new Command(async () => await SetEndLocation());
            ToggleNavigationCommand = new Command(async () => await ToggleNavigation());
            ToggleTrackingCommand = new Command(ToggleTracking);
            LoadDataCommand = new Command(async () => await LoadDataAsync()); // Added LoadDataCommand
            ClearDataCommand = new Command(async () => await ClearDataAsync()); // Added ClearDataCommand

            TrackingButtonText = "Start Tracking";
        }

        private async Task ClearDataAsync()
        {
            try
            {
                IsLoading = true;
                await _databaseService.DeleteAllLocationPointsAsync();
                LocationPoints.Clear(); // Assuming LocationPoints is an ObservableCollection
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to clear data: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                var locationPoints = await _databaseService.GetAllLocationPointsAsync();
                // Assuming LocationPoints is a property in MainViewModel
                LocationPoints = new ObservableCollection<LocationPoint>(locationPoints);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SetStartLocation()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Permission Denied",
                        "Location permission is required for navigation.", "OK");
                    return;
                }

                var location = await Geolocation.Default.GetLocationAsync();
                if (location != null)
                {
                    StartLocation = location;
                    await Shell.Current.DisplayAlert("Start Location Set",
                        $"Start location set to: {location.Latitude:F5}, {location.Longitude:F5}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    $"Could not get current location: {ex.Message}", "OK");
            }
        }

        private async Task SetEndLocation()
        {
            try
            {
                if (StartLocation == null)
                {
                    await Shell.Current.DisplayAlert("Start Location Required",
                        "Please set a start location first.", "OK");
                    return;
                }

                string result = await Shell.Current.DisplayPromptAsync(
                    "Set Destination",
                    "Enter destination coordinates (lat,lon):",
                    initialValue: $"{StartLocation.Latitude + 0.01},{StartLocation.Longitude + 0.01}",
                    maxLength: 50);

                if (string.IsNullOrEmpty(result))
                    return;

                var parts = result.Split(',');
                if (parts.Length != 2 ||
                    !double.TryParse(parts[0], out double lat) ||
                    !double.TryParse(parts[1], out double lon))
                {
                    await Shell.Current.DisplayAlert("Invalid Format",
                        "Please enter coordinates in format: latitude,longitude", "OK");
                    return;
                }

                EndLocation = new Location(lat, lon);
                await Shell.Current.DisplayAlert("End Location Set",
                    $"Destination set to: {lat:F5}, {lon:F5}", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    $"Could not set destination: {ex.Message}", "OK");
            }
        }

        private async Task ToggleNavigation()
        {
            if (IsNavigating)
            {
                IsNavigating = false;
                CurrentRoute = null;
            }
            else
            {
                if (StartLocation == null || EndLocation == null)
                {
                    await Shell.Current.DisplayAlert("Navigation Error",
                        "Both start and end locations must be set.", "OK");
                    return;
                }

                try
                {
                    IsLoading = true;
                    var route = await _navigationService.GetRouteAsync(StartLocation, EndLocation);
                    CurrentRoute = route;
                    IsNavigating = true;
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Navigation Error",
                        $"Could not calculate route: {ex.Message}", "OK");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void ToggleTracking()
        {
            if (TrackingButtonText == "Start Tracking")
            {
                TrackingButtonText = "Stop Tracking";
                IsTracking = true; // Update IsTracking
                // Start tracking logic here
            }
            else
            {
                TrackingButtonText = "Start Tracking";
                IsTracking = false; // Update IsTracking
                // Stop tracking logic here
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

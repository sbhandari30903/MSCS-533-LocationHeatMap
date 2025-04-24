# Location Heat Map

A .NET MAUI application that visualizes location data on a heat map. The app allows users to track their location, set navigation routes, and view location points on an interactive map.

## Features

- **Location Tracking**: Start and stop location tracking to collect location points.
- **Heat Map Visualization**: Display collected location points as a heat map.
- **Navigation**: Set start and destination locations and view navigation routes.
- **Data Management**: Refresh and clear location data.
- **Cross-Platform**: Runs on Android, iOS, Windows, and macOS.

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or later with .NET MAUI workload installed
- Android Emulator or a physical Android device for testing

## Setup Instructions

1. Clone the repository:
    - git clone https://github.com/sbhandari30903/MSCS-533-LocationHeatMap.git cd LocationHeatMap   


2. Restore dependencies:
    - dotnet restore

 
3. Add your Google Maps API key:
   - Open `Platforms/Android/AndroidManifest.xml`.
   - Replace `API KEY` in the `<meta-data>` tag with your Google Maps API key.

4. Build and run the project:
   - dotnet build dotnet run


## Usage

1. Launch the app on your device or emulator.
2. Use the **Start Tracking** button to begin collecting location points.
3. View the heat map to see the collected data.
4. Set start and destination locations to calculate and display navigation routes.
5. Use the **Refresh** button to reload data or **Clear Data** to delete all location points.

## Project Structure

- **ViewModels**: Contains the `MainViewModel` for managing app state and logic.
- **Models**: Includes `LocationPoint` and `NavigationRoute` for data representation.
- **Services**: Provides services like `NavigationService` and `LocationService`.
- **Controls**: Custom controls like `HeatMapRenderer` for rendering the heat map.
- **Platforms**: Platform-specific configurations and code.

## Permissions

The app requires the following permissions:
- `ACCESS_FINE_LOCATION` and `ACCESS_COARSE_LOCATION` for location tracking.
- `ACCESS_BACKGROUND_LOCATION` for background location updates.
- `INTERNET` for fetching navigation routes.


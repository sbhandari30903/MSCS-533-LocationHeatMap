using LocationHeatMap.Converters;

namespace LocationHeatMap
{
    public partial class App : Application
    {
        public static string DatabasePath { get; private set; }

        public App()
        {
            InitializeComponent();
            Resources.Add("BoolToColorConverter", new BoolToColorConverter());

            // Set up the database path
            string dbName = "locationtracker.db3";
            DatabasePath = Path.Combine(FileSystem.AppDataDirectory, dbName);

            MainPage = new AppShell();
        }
    }
}
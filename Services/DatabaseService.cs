using SQLite;
using LocationHeatMap.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocationHeatMap.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<LocationPoint>().Wait();
        }

        public async Task<List<LocationPoint>> GetAllLocationPointsAsync()
        {
            return await _database.Table<LocationPoint>().ToListAsync();
        }

        public async Task<int> SaveLocationPointAsync(LocationPoint point)
        {
            // Check if we have a similar location already (within a small radius)
            var nearby = await _database.Table<LocationPoint>()
                .Where(p =>
                    p.Latitude > point.Latitude - 0.0001 &&
                    p.Latitude < point.Latitude + 0.0001 &&
                    p.Longitude > point.Longitude - 0.0001 &&
                    p.Longitude < point.Longitude + 0.0001)
                .FirstOrDefaultAsync();

            if (nearby != null)
            {
                // Update existing point count
                nearby.Count++;
                nearby.Timestamp = DateTime.UtcNow; // Update timestamp
                return await _database.UpdateAsync(nearby);
            }

            // Otherwise, save new point
            return await _database.InsertAsync(point);
        }

        public async Task<int> DeleteAllLocationPointsAsync()
        {
            return await _database.DeleteAllAsync<LocationPoint>();
        }
    }
}
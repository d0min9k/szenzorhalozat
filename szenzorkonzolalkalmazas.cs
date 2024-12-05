using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Newtonsoft.Json;
using SensorLibrary;
using System.Linq;

namespace SensorNetworkSimulation
{
    class Program
    {
        static List<SensorDataEventArgs> sensorDataList = new List<SensorDataEventArgs>();

        static void Main(string[] args)
        {
            // Adatbázis inicializálása
            InitializeDatabase();

            // Szenzorok inicializálása
            List<Sensor> sensors = new List<Sensor>
            {
                new Sensor("1", "Hőmérséklet", "°C"),
                new Sensor("2", "Páratartalom", "%"),
                new Sensor("3", "Vízmennyiség", "liter")
            };

            foreach (var sensor in sensors)
            {
                sensor.OnDataGenerated += Sensor_OnDataGenerated;
            }

            // Adatok generálása
            Console.WriteLine("Adatok generálása folyamatban...");
            for (int i = 0; i < 10; i++)
            {
                foreach (var sensor in sensors)
                {
                    sensor.GenerateData();
                }
                System.Threading.Thread.Sleep(1000); // Várakozás
            }

            // Adatok JSON fájlba írása
            WriteToJsonFile();

            // LINQ lekérdezések
            PerformLinqQueries();

            Console.WriteLine("Szimuláció vége.");
        }

        private static void Sensor_OnDataGenerated(object sender, SensorDataEventArgs e)
        {
            sensorDataList.Add(e);
            SaveToDatabase(e);
        }

        private static void InitializeDatabase()
        {
            using var connection = new SQLiteConnection("Data Source=sensordata.db;");
            connection.Open();

            string tableCreationQuery = @"
                CREATE TABLE IF NOT EXISTS SensorData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SensorId TEXT,
                    Timestamp TEXT,
                    Value REAL,
                    Unit TEXT
                );";

            using var command = new SQLiteCommand(tableCreationQuery, connection);
            command.ExecuteNonQuery();
        }

        private static void SaveToDatabase(SensorDataEventArgs data)
        {
            using var connection = new SQLiteConnection("Data Source=sensordata.db;");
            connection.Open();

            string insertQuery = @"
                INSERT INTO SensorData (SensorId, Timestamp, Value, Unit)
                VALUES (@SensorId, @Timestamp, @Value, @Unit);";

            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@SensorId", data.SensorId);
            command.Parameters.AddWithValue("@Timestamp", data.Timestamp.ToString("o"));
            command.Parameters.AddWithValue("@Value", data.Value);
            command.Parameters.AddWithValue("@Unit", data.Unit);

            command.ExecuteNonQuery();
        }

        private static void WriteToJsonFile()
        {
            string json = JsonConvert.SerializeObject(sensorDataList, Formatting.Indented);
            File.WriteAllText("sensordata.json", json);
        }

        private static void PerformLinqQueries()
        {
            // Példa LINQ lekérdezésekre
            Console.WriteLine("LINQ lekérdezések:");

            // 1. Átlagérték számítása szenzoronként
            var averageValues = sensorDataList
                .GroupBy(data => data.SensorId)
                .Select(group => new
                {
                    SensorId = group.Key,
                    AverageValue = group.Average(data => data.Value)
                });

            foreach (var result in averageValues)
            {
                Console.WriteLine($"Szenzor {result.SensorId} átlagértéke: {result.AverageValue:F2}");
            }

            // 2. Legmagasabb mért érték
            var maxValue = sensorDataList.Max(data => data.Value);
            Console.WriteLine($"Legmagasabb mért érték: {maxValue:F2}");

            // 3. Adatok időbélyeg szerinti rendezése
            var sortedData = sensorDataList.OrderBy(data => data.Timestamp);
            Console.WriteLine("Időbélyeg szerint rendezett adatok:");
            foreach (var data in sortedData.Take(5))
            {
                Console.WriteLine($"{data.Timestamp}: {data.Value} {data.Unit}");
            }
        }
    }
}

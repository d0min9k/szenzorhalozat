using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SensorLibrary;
using System.Linq;

namespace SensorNetworkSimulation
{
    class Program
    {
        static List<SensorDataEventArgs> SzenzorAdatLista = new List<SensorDataEventArgs>();

        static void Main(string[] args)
        {
            // Adatbázis inicializálása
            InitializeDatabase();

            // Szenzorok inicializálása
            List<Szenzor> sensors = new List<Szenzor>
            {
                new Szenzor("1", "Hőmérséklet", "°C"),
                new Szenzor("2", "Páratartalom", "%"),
                new Szenzor("3", "Vízmennyiség", "liter")
            };

            foreach (var sensor in sensors)
            {
                sensor.GenEsemeny += SzenzorokAdatai;
            }

            // Adatok generálása
            Console.WriteLine("Adatok generálása folyamatban...");
            for (int i = 0; i < 10; i++)
            {
                foreach (var sensor in sensors)
                {
                    sensor.Adatfeltoltes();
                }
                System.Threading.Thread.Sleep(1000); // Várakozás
            }

            // Adatok JSON fájlba írása
            JsonFile();

            // LINQ lekérdezések
            Linq();

            Console.WriteLine("Szimuláció vége.");
        }

        private static void SzenzorokAdatai(object sender, SensorDataEventArgs e)
        {
            SzenzorAdatLista.Add(e);
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
            command.Parameters.AddWithValue("@SensorId", data.Id);
            command.Parameters.AddWithValue("@Timestamp", data.Timestamp.ToString("o"));
            command.Parameters.AddWithValue("@Value", data.Value);
            command.Parameters.AddWithValue("@Unit", data.Unit);

            command.ExecuteNonQuery();
        }

        private static void JsonFile()
        {
            string json = JsonConvert.SerializeObject(SzenzorAdatLista, Formatting.Indented);
            File.WriteAllText("szenzoradatok.json", json);
        }

        private static void Linq()
        {
            // Példa LINQ lekérdezésekre
            Console.WriteLine("LINQ lekérdezések:");

            // 1. Átlagérték számítása szenzoronként
            var averageValues = SzenzorAdatLista
                .GroupBy(data => data.Id)
                .Select(group => new
                {
                    Id = group.Key,
                    AverageValue = group.Average(data => data.Value)
                });

            foreach (var result in averageValues)
            {
                Console.WriteLine($"Szenzor {result.Id} átlagértéke: {result.AverageValue:F2}");
            }

            // 2. Legmagasabb mért érték
            var maxValue = SzenzorAdatLista.Max(data => data.Value);
            Console.WriteLine($"Legmagasabb mért érték: {maxValue:F2}");

            // 3. Adatok időbélyeg szerinti rendezése
            var sortedData = SzenzorAdatLista.OrderBy(data => data.Timestamp);
            Console.WriteLine("Mérések id szerint rendezett adatok:");
            foreach (var data in sortedData.Take(5))
            {
                Console.WriteLine($"{data.Timestamp}: {data.Value} {data.Unit}");
            }
        }
    }
}

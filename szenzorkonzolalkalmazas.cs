using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.IO;
using Newtonsoft.Json;
using SensorLibrary;
using System.Linq;
using System.Runtime.InteropServices;

namespace SzenzorHalozat    
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        static List<SensorDataEvents> SzenzorAdatLista = new List<SensorDataEvents>();

        static void Main(string[] args)
        {
            AllocConsole();

            // Szenzorok inicializálása
            List<Szenzor> szenzorok = new List<Szenzor>
            {
                new Szenzor("1", "Vízszint", "m"),
                new Szenzor("2", "Vízszint", "m")         
            };
            
            foreach (var szenzor in szenzorok)  //Esemenykezeles
            {
                szenzor.GenEsemeny += SzenzorokAdatai;
                szenzor.NullErtekEsemeny += NullErtekGeneralva;     
            }

            // Adatok generálása
            Console.WriteLine("Adatok generálása folyamatban...");
            for (int i = 0; i < 2; i++)
            {
                foreach (var sensor in szenzorok)
                {
                    sensor.Adatfeltoltes();
                }
                System.Threading.Thread.Sleep(1000); // Várakozás
            }

            // Adatok JSON fájlba írása
            JsonFile();

            // LINQ lekérdezések
            Linq();

            Console.WriteLine("Szimuláció vége. Nyomj meg egy billentyűt a kilépéshez.");           
            Console.ReadKey();
        }

        private static void SzenzorokAdatai(object sender, SensorDataEvents e)
        {
            SzenzorAdatLista.Add(e);
            AdatbazisbaBeszur(e.SensorId, e.Timestamp, e.Value, e.Unit);
        }

        private static void NullErtekGeneralva(object sender, SensorDataEvents e)
        {
            Console.WriteLine($"Figyelem! A(z) {e.SensorId} szenzor 0 értéket mért: {e.Timestamp}");
        }

        public static void AdatbazisbaBeszur(string SzenzorId, DateTime Timestamp, double Value, string Unit)
        {
            string connectionString = "server=localhost;database=szenzorhalozat;user=root;password=root;"; // a localhoston root azonositoval es root jelszoval tud belepni a szenzorhalozat adatbazisba
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO szenzorokadatai (SensorId, Timestamp, Value, MeasurementUnit) VALUES (@SensorId, @Timestamp, @Value, @Unit)";


                using (var command = new MySqlCommand(query, connection))
                {
                command.Parameters.AddWithValue("@SensorId", SzenzorId);
                command.Parameters.AddWithValue("@Timestamp", Timestamp);
                command.Parameters.AddWithValue("@Value", Value);
                command.Parameters.AddWithValue("@Unit", Unit);

                command.ExecuteNonQuery();
                }
            }
        }
        
        //Json
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
                .GroupBy(x => x.SensorId)
                .Select(group => new
                {
                    Id = group.Key,
                    AverageValue = group.Average(x => x.Value)
                });

            foreach (var result in averageValues)
            {
                Console.WriteLine($"Szenzor {result.Id} átlagértéke: {result.AverageValue:F2}");
            }

            // 2. Legmagasabb mért érték
            var maxValue = SzenzorAdatLista.Max(x => x.Value);
            Console.WriteLine($"Legmagasabb mért érték: {maxValue:F2}");

            // 3. Legmagalacsonyabb mért érték
            var minValue = SzenzorAdatLista.Min(x => x.Value);
            Console.WriteLine($"Legmagalacsonyabb mért érték: {minValue:F2}");

            // 4. Adatok időbélyeg szerinti rendezése
            var sortedData = SzenzorAdatLista.OrderBy(x => x.Timestamp);
            Console.WriteLine("Mérések időbélyeg szerint rendezett adatok:");
            foreach (var x in sortedData.Take(5))
            {
                Console.WriteLine($"{x.Timestamp}: {x.Value} {x.Unit}");  
            }
        }
    }
}

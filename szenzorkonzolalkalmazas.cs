using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.IO;
using Newtonsoft.Json;
using SensorLibrary;
using System.Linq;

namespace SzenzorHalozat    
{
    class Program
    {
        static List<SensorDataEvents> SzenzorAdatLista = new List<SensorDataEvents>();

        static void Main(string[] args)
        {

            // Szenzorok inicializálása
            List<Szenzor> szenzorok = new List<Szenzor>
            {
                new Szenzor("1", "Vízmagasság", "m"),
                new Szenzor("2", "Vízmagasság", "m"),
                new Szenzor("3", "Vízmagasság", "m")
            };
            
            foreach (var szenzor in szenzorok)  //Esemenykezeles
            {
                szenzor.GenEsemeny += SzenzorokAdatai;
                szenzor.NullErtekEsemeny += NullErtekGeneralva;     
            }

            // Adatok generálása
            Console.WriteLine("Adatok generálása folyamatban...");
            for (int i = 0; i < 10; i++)
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

            Console.WriteLine("Szimuláció vége.");
            Console.ReadKey();
        }

        private static void SzenzorokAdatai(object sender, SensorDataEvents e)
        {
            SzenzorAdatLista.Add(e);           
           
        }

        private static void NullErtekGeneralva(object sender, SensorDataEvents e)
        {
            Console.WriteLine($"Figyelem! A(z) {e.SzenzorId} szenzor 0 értéket mért: {e.Timestamp}");
        }


        void AdatbazisbaBeszur(string SzenzorId, DateTime Timestamp, double Value, string ME)
        {
            string connectionString = "Server=localhost;Database=Szenzorhalozat;Uid=root;Pwd=root;"; // a localhoston root azonositoval es root jelszoval tud belepni a szenzorhalozat adatbazisba
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO SzenzorokAdatai (SensorId, Timestamp, Value, Unit) VALUES (@SzenzorId, @Timestamp, @Value, @ME)";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SzenzorId", SzenzorId);
                    command.Parameters.AddWithValue("@MeresIdeje", Timestamp);
                    command.Parameters.AddWithValue("@Ertek", Value);
                    command.Parameters.AddWithValue("@Mertekegyseg", ME);

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
                .GroupBy(x => x.SzenzorId)
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
            var minValue = SzenzorAdatLista.Max(x => x.Value);
            Console.WriteLine($"Legmagalacsonyabb mért érték: {minValue:F2}");

            // 4. Adatok időbélyeg szerinti rendezése
            var sortedData = SzenzorAdatLista.OrderBy(x => x.Timestamp);
            Console.WriteLine("Mérések id szerint rendezett adatok:");
            foreach (var x in sortedData.Take(5))
            {
                Console.WriteLine($"{x.Timestamp}: {x.Value} {x.ME}");  
            }
        }
    }
}

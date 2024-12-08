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
                new Szenzor("1", "Vízszint", "m"),
                new Szenzor("2", "Vízszint", "m"),
                new Szenzor("3", "Vízszint", "m")
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

            try
            {
                AdatbazisInicializalasa();
                TablaInicializalas();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt: {ex.Message}");
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
            AdatbazisbaBeszur(e.SzenzorId, e.Timestamp, e.Value, e.Unit);
        }

        private static void NullErtekGeneralva(object sender, SensorDataEvents e)
        {
            Console.WriteLine($"Figyelem! A(z) {e.SzenzorId} szenzor 0 értéket mért: {e.Timestamp}");
        }

        public static void AdatbazisInicializalasa()
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=root;"; // Adatbázis nélkül csatlakozunk
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Adatbázis létrehozása, ha nem létezik
                string createDatabaseQuery = "CREATE DATABASE IF NOT EXISTS szenzorhalozat;";
                using (var command = new MySqlCommand(createDatabaseQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void TablaInicializalas()
        {
                            string connectionString = "Server=localhost;Database=szenzorhalozat;Uid=root;Pwd=root;";
                            using (var connection = new MySqlConnection(connectionString))
                            {
                                connection.Open();

                                // Tábla létrehozása, ha nem létezik
                                string createTableQuery = @"
                                CREATE TABLE IF NOT EXISTS szenzorokadatai (               
                                SzenzorId VARCHAR(255),
                                Timestamp DATETIME,
                                Value DOUBLE,
                                Unit VARCHAR(255)
                                );
                                ";
                                using (var command = new MySqlCommand(createTableQuery, connection))
                                {
                                        command.ExecuteNonQuery();
                                }
                            }
        }

        public static void AdatbazisbaBeszur(string SzenzorId, DateTime Timestamp, double Value, string Unit)
                      {
                            string connectionString = "Server=localhost;Database=szenzorhalozat;Uid=root;Pwd=root;"; // a localhoston root azonositoval es root jelszoval tud belepni a szenzorhalozat adatbazisba
                            using (var connection = new MySqlConnection(connectionString))
                            {
                                connection.Open();
                                string query = "INSERT INTO szenzorokadatai (SensorId, Timestamp, Value, Unit) VALUES (@SensorId, @Timestamp, @Value, @Unit)";

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
                Console.WriteLine($"{x.Timestamp}: {x.Value} {x.Unit}");  
            }
        }
    }
}

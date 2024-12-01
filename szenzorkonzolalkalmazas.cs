using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using szenzorhalozatDLL;

namespace SensorNetworkSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            // Hozzunk létre szenzorokat
            List<Szenzor> szenzorok = new List<Szenzor>
            {
                new Szenzor(1, "Hőmérséklet", 75.0),
                new Szenzor(2, "Páratartalom", 60.0),
                new Szenzor(3, "Vízszint", 80.0)
            };

            foreach (var szenzor in szenzorok)
            {
                szenzor.KuszobTullepve += Szenzor_KuszobotTullep;
            }

            // Generáljunk méréseket
            foreach (var sensor in szenzorok)
            {
                sensor.MereseredmenyGeneralas();
                Console.WriteLine($"Szenzor ID: {sensor.SensorId}, Típus: {sensor.SensorType}, Érték: {sensor.Value}, Idő: {sensor.Idopont}");
            }

            // LINQ példák
            Console.WriteLine("\nLINQ lekérdezések:");
            var highValues = szenzorok.Where(s => s.Value > 50).ToList();
            Console.WriteLine("50 feletti értékek:");
            highValues.ForEach(s => Console.WriteLine($"{s.SensorType}: {s.Value}"));

            // Adatok JSON fájlba írása
            string jsonOutput = JsonSerializer.Serialize(szenzorok);
            File.WriteAllText("sensor_data.json", jsonOutput);
            Console.WriteLine("\nMérési adatok JSON fájlba mentve: sensor_data.json");

            //Adatbazis
        }

        private static void Szenzor_KuszobotTullep(object kuld, EventArgs e)
        {
            var szenzor = kuld as Szenzor;
            Console.WriteLine($"FIGYELEM! A(z) {szenzor.SensorType} szenzor (ID: {szenzor.SensorId}) túllépte a küszöbértéket: {szenzor.Value}");
        }
    }
}

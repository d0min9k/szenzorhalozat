using System;

namespace SensorLibrary
{
    public class SensorDataEventArgs : EventArgs
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
    }

    public class Szenzor
    {
        public string Id { get; private set; }
        public string Parameter { get; private set; }
        public string Unit { get; private set; }

        public event EventHandler<SensorDataEventArgs> GenEsemeny;

        private Random _random;

        public Szenzor(string id, string parameter, string unit)
        {
            Id = id;
            Parameter = parameter;
            Unit = unit;
            _random = new Random();
        }

        public void Adatfeltoltes()
        {
            double value = _random.NextDouble() * 100; // Véletlenszerű érték 0-100 között
            GenEsemeny?.Invoke(this, new SensorDataEventArgs
            {
                Id = Id,
                Timestamp = DateTime.Now,
                Value = value,
                Unit = Unit
            });
        }
    }
}

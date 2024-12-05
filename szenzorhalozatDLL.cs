using System;

namespace SensorLibrary
{
    public class SensorDataEventArgs : EventArgs
    {
        public string SensorId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
    }

    public class Sensor
    {
        public string Id { get; private set; }
        public string Parameter { get; private set; }
        public string Unit { get; private set; }

        public event EventHandler<SensorDataEventArgs> OnDataGenerated;

        private Random _random;

        public Sensor(string id, string parameter, string unit)
        {
            Id = id;
            Parameter = parameter;
            Unit = unit;
            _random = new Random();
        }

        public void GenerateData()
        {
            double value = _random.NextDouble() * 100; // Véletlenszerű érték 0-100 között
            OnDataGenerated?.Invoke(this, new SensorDataEventArgs
            {
                SensorId = Id,
                Timestamp = DateTime.Now,
                Value = value,
                Unit = Unit
            });
        }
    }
}

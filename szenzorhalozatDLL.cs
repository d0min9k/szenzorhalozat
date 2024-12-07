using System;

namespace SensorLibrary
{
    public class SensorDataEvents : EventArgs
    {
        public string SzenzorId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string ME { get; set; }
    }

    public class Szenzor
    {
        public string Id { get; private set; }
        public string Parameter { get; private set; }
        public string Unit { get; private set; }

        public event EventHandler<SensorDataEvents> GenEsemeny;

        public event EventHandler<SensorDataEvents> NullErtekEsemeny;
     

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
            double value = _random.NextDouble() * 10; // Véletlenszerű érték 0-10 között
            if (value < 1e-6) // 0-hoz közeli érték esetén 0-nak tekintjük, a kódban az if (value < 1e-6) azt jelenti, hogy a feltétel igaz, ha a value változó értéke kisebb, mint 10 a -6-on
            {
                value = 0;
                NullErtekEsemeny?.Invoke(this, new SensorDataEvents
                {
                    SzenzorId = Id,
                    Timestamp = DateTime.Now,
                    Value = value,
                    ME = Unit
                });
            }
            else
            {
                GenEsemeny?.Invoke(this, new SensorDataEvents
                {
                    SzenzorId = Id,
                    Timestamp = DateTime.Now,
                    Value = value,
                    ME = Unit
                });
            }
        }
    }
}

using System;

namespace szenzorhalozatDLL
{
    public class Szenzor
    {
        public int SensorId { get; set; }
        public string SensorType { get; set; }
        public double Value { get; private set; }
        public DateTime Idopont { get; private set; }

        public delegate void ThresholdExceededEventHandler(object sender, EventArgs e);
        public event ThresholdExceededEventHandler KuszobTullepve;

        private double Kuszob;

        public Szenzor(int sensorId, string sensorType, double kuszob)
        {
            SensorId = sensorId;
            SensorType = sensorType;
            Kuszob = kuszob;
        }

        public void MereseredmenyGeneralas()
        {
            Random random = new Random();
            Value = Math.Round(random.NextDouble() * 100, 2); // Véletlenszám 0-100 között
            Idopont = DateTime.Now;

            if (Value > Kuszob)
            {
                KuszobTullepes();
            }
        }

        protected virtual void KuszobTullepes()
        {
            KuszobTullepve?.Invoke(this, EventArgs.Empty);
        }
    }
}

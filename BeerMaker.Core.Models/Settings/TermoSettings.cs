namespace BeerMaker.Core.Models.Settings
{
    public class TermoSettings
    {
        public int Betta { get; set; }
        public double T0 { get; set; }
        public double R0 { get; set; }
        public double ADCU { get; set; }
        public int Rp { get; set; }
        public int DeviceAddress { get; set; }
        public double Delta { get; set; }
    }
}
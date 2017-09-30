using System;

namespace RequestServiceImpl
{
    public class MetersDto
    {
        public DateTime Date { get; set; }
        public double Electro1 { get; set; }
        public double Electro2 { get; set; }
        public double ColdWater1 { get; set; }
        public double HotWater1 { get; set; }
        public double ColdWater2 { get; set; }
        public double HotWater2 { get; set; }
        public double Heating { get; set; }
    }
}
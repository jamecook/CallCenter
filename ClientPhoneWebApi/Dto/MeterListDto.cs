using System;

namespace ClientPhoneWebApi.Dto
{
    public class MeterListDto
    {
        public int Id { get; set; }
        public string PersonalAccount { get; set; }
        public string ServiceCompany { get; set; }
        public string StreetName { get; set; }
        public int StreetId { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public int HouseId { get; set; }
        public string Flat { get; set; }
        public int AddressId { get; set; }
        public DateTime Date { get; set; }
        public double Electro1 { get; set; }
        public double Electro2 { get; set; }
        public double ColdWater1 { get; set; }
        public double HotWater1 { get; set; }
        public double ColdWater2 { get; set; }
        public double HotWater2 { get; set; }
        public double ColdWater3 { get; set; }
        public double HotWater3 { get; set; }
        public double Heating { get; set; }
        public double? Heating2 { get; set; }
        public double? Heating3 { get; set; }
        public double? Heating4 { get; set; }
        public string FullAddress
        {
            get
            {
                return string.IsNullOrEmpty(Corpus) ? $"{StreetName}, {Building}, {Flat}"
                    : $"{StreetName}, {Building}/{Corpus}, {Flat}";
            }
        }
    }
}
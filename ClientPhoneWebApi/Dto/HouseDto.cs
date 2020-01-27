using System;

namespace ClientPhoneWebApi.Dto
{
    public class HouseDto
    {
        public int Id { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string StreetName { get; set; }
        public int StreetId { get; set; }
        public int? RegionId { get; set; }
        public string RegionName { get; set; }
        public int? ServiceCompanyId { get; set; }
        public string ServiceCompanyName { get; set; }
        public int? EntranceCount { get; set; }
        public int? FlatCount { get; set; }
        public int? FloorCount { get; set; }
        public int? ElevatorCount { get; set; }
        public bool HaveParking { get; set; }
        public DateTime? CommissioningDate { get; set; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Corpus))
                    return Building;
                return Building + "ê." + Corpus;
            }
        }
    }
}
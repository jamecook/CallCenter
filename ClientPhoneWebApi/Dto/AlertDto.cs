using System;

namespace ClientPhoneWebApi.Dto
{
    public class SaveAlertDto
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public int StreetId { get; set; }
        public int HouseId { get; set; }

        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AlertTypeDto Type { get; set; }
        public AlertServiceTypeDto ServiceType { get; set; }
        public string Description { get; set; }
        public RequestUserDto User { get; set; }
        public DateTime CreateDate { get; set; }
        public string FullAddress
        {
            get
            {
                var address = StreetName;
                if (string.IsNullOrEmpty(Corpus))
                    address += " " + Building;
                else
                    address += " " + Building + "/" + Corpus;
                return address;
            }
        }
    }

    public class AlertDto
    {
        public int Id { get; set; }
        public int StreetId { get; set; }
        public int HouseId { get; set; }

        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AlertTypeDto Type { get; set; }
        public AlertServiceTypeDto ServiceType { get; set; }
        public string Description { get; set; }
        public RequestUserDto User { get; set; }
        public DateTime CreateDate { get; set; }
        public string FullAddress
        {
            get
            {
                var address = StreetName;
                if (string.IsNullOrEmpty(Corpus))
                    address += " " + Building;
                else
                    address += " " + Building + "/" + Corpus;
                return address;
            }
        }
    }
    public class AlertTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class AlertServiceTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class AlertTimeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AddMinutes { get; set; }
    }
}
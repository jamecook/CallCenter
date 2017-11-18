using System;

namespace RequestServiceImpl.Dto
{
    public class AppRequestDto
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string Flat { get; set; }
        public string PrimaryType { get; set; }
        public string ServiceType { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
    }
}
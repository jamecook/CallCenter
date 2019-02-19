using System;

namespace RequestServiceImpl.Dto
{
    public class FondDto
    {
        public int Id { get; set; }
        public string Flat { get; set; }
        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string Name { get; set; }
        public string Phones { get; set; }
        public DateTime? KeyDate { get; set; }
    }
}
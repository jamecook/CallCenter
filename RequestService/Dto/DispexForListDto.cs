using System;

namespace RequestServiceImpl.Dto
{
    public class DispexForListDto
    {
        public int Id { get; set; }
        public string BitrixId { get; set; }
        public string FromPhone { get; set; }
        public DateTime CreateDate { get; set; }
        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string Flat { get; set; }
        public int? BitrixServiceId { get; set; }
        public string BitrixServiceName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
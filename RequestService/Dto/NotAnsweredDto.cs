using System;

namespace RequestServiceImpl.Dto
{
    public class NotAnsweredDto
    {
        public string UniqueId { get; set; }

        public string CallerId { get; set; }
        public string ServiceCompany { get; set; }
        public string Prefix { get; set; }

        public DateTime? CreateTime { get; set; }
    }
}
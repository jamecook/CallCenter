using System;

namespace ClientPhoneWebApi.Dto
{
    public class NotAnsweredDto
    {
        public string UniqueId { get; set; }

        public string CallerId { get; set; }
        public string ServiceCompany { get; set; }
        public string Prefix { get; set; }
        public int? IvrDtmf { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
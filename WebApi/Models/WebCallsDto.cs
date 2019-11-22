using System;

namespace WebApi.Models
{
    public class WebCallsDto
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Direction { get; set; }
        public string Extension { get; set; }
        public int Duration { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
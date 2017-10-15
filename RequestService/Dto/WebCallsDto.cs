using System;

namespace RequestServiceImpl.Dto
{
    public class WebCallsDto
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreateTime { get; set; }
        public string MonitorFile { get; set; }
    }
}
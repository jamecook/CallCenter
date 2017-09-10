using System;

namespace RequestServiceImpl.Dto
{
    public class WebCallsDto
    {
        public int RequestId { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreateTime { get; set; }
        public string MonitorFile { get; set; }
    }
}
using System;

namespace RequestServiceImpl.Dto
{
    public class RingUpInfoDto
    {
        public string Phone { get; set; }
        public DateTime? LastCallTime { get; set; }
        public int? LastCallLength { get; set; }
        public int? CalledCount { get; set; }
        public string DoneCalls { get; set; }
       
    }
}
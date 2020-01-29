using System;

namespace ClientPhoneWebApi.Dto
{
    public class SetRequestWorkingTimesDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
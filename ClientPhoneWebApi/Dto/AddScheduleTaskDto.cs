using System;

namespace ClientPhoneWebApi.Dto
{
    public class AddScheduleTaskDto
    {
        public int UserId { get; set; }
        public int? RequestId { get; set; }
        public int WorkerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string EventDescription { get; set; }
    }
}
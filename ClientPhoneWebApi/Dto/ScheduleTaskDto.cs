using System;

namespace ClientPhoneWebApi.Dto
{
    public class ScheduleTaskDto
    {
        public int Id { get; set; }
        public int? RequestId { get; set; }
        public WorkerDto Worker { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string EventDescription { get; set; }
    }
}
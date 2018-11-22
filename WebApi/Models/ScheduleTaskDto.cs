using System;

namespace WebApi.Models
{
    public class ScheduleTaskDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ScheduleWorkerDto Worker { get; set; }
    }

    public class ScheduleWorkerDto
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public string FullName => $"{SurName} {FirstName} {PatrName}".TrimEnd();
    }
}
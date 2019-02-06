using System;

namespace WebApi.Models
{
    public class AddTaskDto
    {
        public int WorkerId { get; set; }
        public int RequestId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
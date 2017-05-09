using System;

namespace CRMPhone.Dto
{
    public class ExecuteDateHistoryDto
    {
        public DateTime CreateTime { get; set; }
        public DateTime ExecuteTime { get; set; }
        public string ExecutePeriod { get; set; }
        public RequestUserDto CreateUser { get; set; }
        public string Note { get; set; }
    }
}
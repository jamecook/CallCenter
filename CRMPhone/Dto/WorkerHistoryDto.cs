using System;

namespace CRMPhone.Dto
{
    public class WorkerHistoryDto
    {
        public DateTime CreateTime { get; set; }
        public RequestUserDto Worker { get; set; }
        public RequestUserDto CreateUser { get; set; }
    }
}
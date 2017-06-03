using System;

namespace RequestServiceImpl.Dto
{
    public class StatusHistoryDto
    {
        public DateTime CreateTime { get; set; }
        public StatusDto Status { get; set; }
        public RequestUserDto CreateUser { get; set; }
    }

}
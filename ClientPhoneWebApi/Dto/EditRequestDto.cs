using System;

namespace ClientPhoneWebApi.Dto
{
    public class EditRequestDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public int RequestTypeId { get; set; }
        public string RequestMessage { get; set; }
        public bool Immediate { get; set; }
        public bool Chargeable { get; set; }
        public bool IsBadWork { get; set; }
        public int Warranty { get; set; }
        public bool IsRetry { get; set; }
        public DateTime? AlertTime { get; set; }
        public DateTime? TermOfExecution { get; set; }
    }
}
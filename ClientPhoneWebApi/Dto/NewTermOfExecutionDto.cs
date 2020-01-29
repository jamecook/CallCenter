using System;

namespace ClientPhoneWebApi.Dto
{
    public class NewTermOfExecutionDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public DateTime TermOfExecution { get; set; }
        public string Note { get; set; }
    }
}
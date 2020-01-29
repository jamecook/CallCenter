using System;

namespace ClientPhoneWebApi.Dto
{
    public class NewExecuteDateDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public DateTime ExecuteDate { get; set; }
        public PeriodDto Period { get; set; }
        public string Note { get; set; }
    }
}
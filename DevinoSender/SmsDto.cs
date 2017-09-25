using System;

namespace DevinoSender
{
    public class SmsDto
    {
        public int Id;
        public int RequestId;
        public string Sender;
        public string Phone;
        public string Message;
        public string StateDescription;
        public int? StateId;
        public decimal? Price;
        public string  DevinoMessageId;
        public DateTime? UtcDate;
        public DateTime CreateDate;
    }
}
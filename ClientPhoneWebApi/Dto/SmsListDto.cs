using System;

namespace ClientPhoneWebApi.Dto
{
    public class SmsListDto
    {
        public int Id { get; set; }
        public DateTime SendTime { get; set; }
        public string Phone { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public string State { get; set; }
        public double? Price { get; set; }
        public string ClientOrWorker { get; set; }

    }
}
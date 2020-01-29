namespace ClientPhoneWebApi.Dto
{
    public class SendSmsDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public string Sender { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public bool IsClient { get; set; }
    }
}
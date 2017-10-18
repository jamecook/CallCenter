namespace RequestServiceImpl.Dto
{
    public class SmsSettingDto
    {
        public string Sender { get; set; }
        public bool SendToClient { get; set; }
        public bool SendToWorker { get; set; }
    }
}
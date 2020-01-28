namespace ClientPhoneWebApi.Dto
{
    public class AddCallHistoryDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public string CallUniqueId { get; set; }
        public string CallId { get; set; }
        public string MethodName { get; set; }
    }
}
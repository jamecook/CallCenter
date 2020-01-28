namespace ClientPhoneWebApi.Dto
{
    public class AddCallToRequestDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public string CallUniqueId { get; set; }
    }
}
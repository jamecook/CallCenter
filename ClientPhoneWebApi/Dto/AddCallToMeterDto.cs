namespace ClientPhoneWebApi.Dto
{
    public class AddCallToMeterDto
    {
        public int UserId { get; set; }
        public int? MeterId { get; set; }
        public string CallUniqueId { get; set; }
    }
}
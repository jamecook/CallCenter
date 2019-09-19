namespace WebApi.Models
{
    public class PushIdAndAddressDto
    {
        public AddressDto Address { get; set; }
        public string PushId { get; set; }
        public string SipPhone { get; set; }
    }
}
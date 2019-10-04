namespace WebApi.Models
{
    public class PushIdAndAddressDto
    {
        public AddressDto Address { get; set; }
        public string PushId { get; set; }
        public string DeviceId { get; set; }
        public string SipPhone { get; set; }
        public string SipId { get; set; }
        public string Secret { get; set; }
    }
}
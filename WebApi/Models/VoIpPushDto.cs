using System;

namespace WebApi.Models
{
    public class VoIpPushDto
    {
        public string Addr { get; set; }
        public int AddrId { get; set; }
        public string PushId { get; set; }
        public string DeviceId { get; set; }
        public string SipId { get; set; }
    }
}
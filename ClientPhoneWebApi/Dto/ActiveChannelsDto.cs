using System;
using System.ComponentModel;
using System.Windows.Media;

namespace ClientPhoneWebApi.Dto
{
    public class ActiveChannelsDto
    {

        public string UniqueId { get; set; }

        public string Channel { get; set; }
        public int? RequestId { get; set; }

        public string CallerIdNum { get; set; }
        public string PhoneOrName => Master?.ShortName ?? CallerIdNum;
        public string ServiceCompany { get; set; }
        public RequestUserDto Master { get; set; }

        public string ChannelState { get; set; }

        public DateTime? CreateTime { get; set; }

        public DateTime? AnswerTime { get; set; }

        public int? IvrDtmf { get; set; }

        public int WaitSecond { get; set; }

        public string WaitSecondText => $"{WaitSecond} c.";
    }
}
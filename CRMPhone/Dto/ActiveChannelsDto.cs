using System;
using System.ComponentModel;
using CRMPhone.Annotations;

namespace CRMPhone.Dto
{
    public class ActiveChannelsDto
    {
        public string UniqueId { get; set; }

        public string Channel { get; set; }

        public string CallerIdNum { get; set; }

        public string ChannelState { get; set; }

        public DateTime? CreateTime { get; set; }

        public DateTime? AnswerTime { get; set; }
    }
}
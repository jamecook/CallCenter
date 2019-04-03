using System;

namespace RequestServiceImpl.Dto
{
    public class StatIvrCallListDto
    {
        public string LinkedId { get; set; }
        public string CallerIdNum { get; set; }
        public DateTime InCreateTime { get; set; }
        public DateTime InEndTime { get; set; }
        public DateTime? InBridgedTime { get; set; }
        public string Phone { get; set; }
        public string HangupCause { get; set; }
        public string Result { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? BridgedTime { get; set; }
        public int ClientWaitSec { get; set; }
        public int? TalkDuration { get; set; }
        public int CallDuration { get; set; }
    }
}
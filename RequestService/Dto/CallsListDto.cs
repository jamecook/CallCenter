using System;

namespace RequestServiceImpl.Dto
{
    public class CallsListDto
    {
        public string UniqueId { get; set; }

        public string CallerId { get; set; }

        public string Direction { get; set; }

        public string MonitorFileName { get; set; }

        public DateTime? CreateTime { get; set; }

        public DateTime? AnswerTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime? BridgedTime { get; set; }

        public int? WaitingTime { get; set; }

        public int? TalkTime { get; set; }

        public bool EnablePlayButton
        {
            get
            {
                if (!string.IsNullOrEmpty(MonitorFileName))
                {
                    return true;
                }
                return false;
            }
        }

        public string ImagePath => Direction == "in" ? "pack://application:,,,/Images/incalls.png" : "pack://application:,,,/Images/outcalls.png";
    }
}
using System;

namespace RequestServiceImpl.Dto
{
    public class CallsListDto
    {
        public int Id { get; set; }
        public string UniqueId { get; set; }

        public string CallerId { get; set; }

        public string Direction { get; set; }
        public string ServiceCompany { get; set; }

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
        public bool CanDeleteRecord => AppSettings.CurrentUser != null && AppSettings.CurrentUser.Roles.Exists(r => r.Name == "admin"); 
        
        public RequestUserDto User { get; set; }
        public string Requests { get; set; }
        public string RedirectPhone { get; set; }
        public string ImagePath => Direction == "in" ? "pack://application:,,,/Images/incalls.png" : "pack://application:,,,/Images/outcalls.png";
    }
}
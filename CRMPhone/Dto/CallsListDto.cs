using System;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace CRMPhone.Dto
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

        public Visibility PlayButtonVisibility
        {
            get
            {
                if (!string.IsNullOrEmpty(MonitorFileName))
                {
                    return Visibility.Visible;
                }
                else
                    return Visibility.Collapsed;
            }
        }

        public string ImagePath
        {
            get { return Direction == "in" ? "pack://application:,,,/Images/incalls.png" : "pack://application:,,,/Images/outcalls.png"; }
        }

        private bool _canPlay = true;
        private ICommand _playCommand;
        public ICommand PlayCommand { get { return _playCommand ?? (_playCommand = new CommandHandler(PlayRecord, _canPlay)); } }

        private void PlayRecord()
        {
            var serverIpAddress = ConfigurationManager.AppSettings["CallCenterIP"]; ;
            var localFileName = MonitorFileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\");
            Process.Start(localFileName);
        }
    }
}
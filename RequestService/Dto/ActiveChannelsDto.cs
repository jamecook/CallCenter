using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using NLog;

namespace RequestServiceImpl.Dto
{
    public class ActiveChannelsDto : INotifyPropertyChanged
    {
        private int _waitSecond;
        private int? _ivrDtmf;
        public string UniqueId { get; set; }

        public string Channel { get; set; }
        public int? RequestId { get; set; }
        public Visibility VisibleRequest => RequestId.HasValue ? Visibility.Visible : Visibility.Collapsed;

        public string CallerIdNum { get; set; }
        public string PhoneOrName => Master?.ShortName ?? CallerIdNum;
        public string ServiceCompany { get; set; }
        public RequestUserDto Master { get; set; }

        public string ChannelState { get; set; }

        public DateTime? CreateTime { get; set; }

        public DateTime? AnswerTime { get; set; }

        public int? IvrDtmf
        {
            get { return _ivrDtmf; }
            set
            {
                _ivrDtmf = value; OnPropertyChanged(nameof(IvrDtmf));
                OnPropertyChanged(nameof(Color));
            }
        }

        public Brush Color
        {
            get
            {
                if(!IvrDtmf.HasValue)
                    return new SolidColorBrush(Colors.Blue);
                switch (IvrDtmf.Value)
                {
                    case 1:
                        return new SolidColorBrush(Colors.Red);
                    case 3:
                        return new SolidColorBrush(Colors.Green);
                    case 5:
                        return new SolidColorBrush(Colors.DarkOrange);
                }
                return new SolidColorBrush(Colors.Blue);
            }
        }
        public int WaitSecond
        {
            get { return _waitSecond; }
            set
            {
                _waitSecond = value;
                OnPropertyChanged(nameof(WaitSecond));
                OnPropertyChanged(nameof(WaitSecondText));
            }
        }

        public string WaitSecondText => $"{WaitSecond} c.";
        public event PropertyChangedEventHandler PropertyChanged;

        [Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
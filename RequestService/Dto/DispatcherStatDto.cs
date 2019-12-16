using System;
using System.ComponentModel;
using System.Windows.Media;

namespace RequestServiceImpl.Dto
{
    public class DispatcherStatDto : INotifyPropertyChanged
    {
        private int? _talkTime;
        private int? _waitingTime;
        private string _phoneNumber;
        private string _direction;
        private string _uniqueId;
        private bool? _onLine;

        public int Id { get; set; }
        //public int? ServiceCompanyId { get; set; }
        //public string ServiceCompanyName { get; set; }
        public string SurName { get; set; }
        public string IpAddress { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public string SipNumber { get; set; }
        public string Version { get; set; }

        public bool? OnLine
        {
            get => _onLine;
            set
            {
                _onLine = value;
                OnPropertyChanged(nameof(OnLine));
                OnPropertyChanged(nameof(OnLineText));
                OnPropertyChanged(nameof(Color));
            }
        }
        public string OnLineText => OnLine.HasValue? OnLine.Value?"מםכאים":"מפפכאים" : "";
        public Brush Color
        {
            get
            {
                if (!OnLine.HasValue)
                    return new SolidColorBrush(Colors.Black);
                switch (OnLine.Value)
                {
                    case true:
                        return new SolidColorBrush(Colors.Blue);
                    case false:
                        return new SolidColorBrush(Colors.Red);
                }
                return new SolidColorBrush(Colors.Black);
            }
        }

        public string Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        public string UniqueId
        {
            get => _uniqueId;
            set
            {
                _uniqueId = value;
                OnPropertyChanged(nameof(UniqueId));
            }
        }

        public DateTime AliveTime { get; set; }

        public int? TalkTime
        {
            get => _talkTime;
            set
            {
             _talkTime = value;
             OnPropertyChanged(nameof(TalkTime));
            }
        }

        public int? WaitingTime
        {
            get => _waitingTime;
            set
            {
                _waitingTime = value;
                OnPropertyChanged(nameof(WaitingTime));
            }
        }

        public string FullName => $"{SurName} {FirstName} {PatrName}".TrimEnd();
        public event PropertyChangedEventHandler PropertyChanged;

        [Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
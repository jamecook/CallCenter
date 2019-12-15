using System;
using System.ComponentModel;

namespace RequestServiceImpl.Dto
{
    public class DispatcherStatDto : INotifyPropertyChanged
    {
        private int? _talkTime;
        private int? _waitingTime;
        private string _phoneNumber;
        private string _direction;
        private string _uniqueId;

        public int Id { get; set; }
        //public int? ServiceCompanyId { get; set; }
        //public string ServiceCompanyName { get; set; }
        public string SurName { get; set; }
        public string IpAddress { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public string SipNumber { get; set; }

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
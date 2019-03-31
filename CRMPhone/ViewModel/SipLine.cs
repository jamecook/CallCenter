using System;
using System.ComponentModel;
using System.Windows.Media;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class SipLine : INotifyPropertyChanged
    {
        private string _state;
        private string _uri;
        private string _phone;
        private string _callTime;
        private Brush _callTimeColor;

        public int Id { get; set; }
        public string Name { get; set; }
        public string State
        {
            get { return _state; }
            set { _state = value; OnPropertyChanged(nameof(State));}
        }

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; OnPropertyChanged(nameof(Phone)); }
        }

        public string Uri
        {
            get { return _uri; }
            set { _uri = value; OnPropertyChanged(nameof(Uri));}
        }
        public DateTime? LastAnswerTime { get; set; }

        public string CallTime
        {
            get { return _callTime; }
            set { _callTime = value; OnPropertyChanged(nameof(CallTime)); }
        }

        public Brush CallTimeColor
        {
            get { return _callTimeColor; }
            set { _callTimeColor = value; OnPropertyChanged(nameof(CallTimeColor));}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
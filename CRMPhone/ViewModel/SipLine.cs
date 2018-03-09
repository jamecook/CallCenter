using System.ComponentModel;
using CRMPhone.Annotations;

namespace CRMPhone.ViewModel
{
    public class SipLine : INotifyPropertyChanged
    {
        private string _state;
        private string _uri;
        private string _phone;

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
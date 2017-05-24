using System;
using System.ComponentModel;
using System.Windows.Input;
using CRMPhone.Annotations;

namespace CRMPhone.Dto
{
    public class ContactDto : INotifyPropertyChanged
    {
        private bool _isMain;
        private bool _isOwner;
        public int Id { get; set; }

        public bool IsMain
        {
            get { return _isMain; }
            set { _isMain = value; OnPropertyChanged(nameof(IsMain));}
        }

        public bool IsOwner
        {
            get { return _isOwner; }
            set { _isOwner = value; OnPropertyChanged(nameof(IsOwner));}
        }

        public string PhoneNumber { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
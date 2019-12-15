using System.ComponentModel;

namespace RequestServiceImpl.Dto
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
        public string Name { get; set; }
        public string Email { get; set; }
        public string AdditionInfo { get; set; }
        public bool CanEditPhone { get { return Id == 0; } }
        public bool ReadOnlyPhone => !CanEditPhone;


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ContactDto Copy()
        {
            return new ContactDto()
            {
                IsMain = this.IsMain,
                Id = this.Id,
                IsOwner = this.IsOwner,
                PhoneNumber = this.PhoneNumber != null ? string.Copy(this.PhoneNumber) : null,
                Name = this.Name != null ? string.Copy(this.Name) : null,
                AdditionInfo = this.AdditionInfo != null ? string.Copy(this.AdditionInfo) : null,
                Email = this.Email != null ? string.Copy(this.Email) : null
            };
        }
    }
}
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;

namespace CRMPhone.ViewModel.Admins
{
    public class PhoneDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private ICommand _saveCommand;
        private string _phoneNumber;

        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber));}
        }

        public PhoneDialogViewModel(RequestServiceImpl.RequestService requestService)
        {
            _requestService = requestService;
            PhoneNumber = _requestService.GetRedirectPhone();
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            _requestService.SaveRedirectPhone(PhoneNumber);
            _view.DialogResult = true;
        }

      
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
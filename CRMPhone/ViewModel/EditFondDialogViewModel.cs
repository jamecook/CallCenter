using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class EditFondDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;


        public EditFondDialogViewModel(string fullAddress, string abonentName, string phoneNumbers)
        {
            FullAddress = fullAddress;
            AbonentName = abonentName;
            PhoneNumbers = phoneNumbers;
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }
        private ICommand _cancelCommand;
        public ICommand CancelCommand { get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(Cancel)); } }

        private void Save(object sender)
        {
            _view.DialogResult = true;
        }
        private void Cancel(object sender)
        {
            _view.DialogResult = false;
        }
        public string FullAddress { get; set; }
        public string AbonentName { get; set; }
        public string PhoneNumbers { get; set; }
      
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
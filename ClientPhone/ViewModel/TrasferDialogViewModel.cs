using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class TrasferDialogViewModel : INotifyPropertyChanged
    {
        private List<TransferIntoDto> _phonesList;
        private TransferIntoDto _clientPhone;
        private Window _view;

        private bool _canExecute = true;
        private ICommand _transferCommand;
        private string _transferPhone;
        public ICommand TransferCommand { get { return _transferCommand ?? (_transferCommand = new CommandHandler(Transfer, _canExecute)); } }

        private void Transfer()
        {
            _view.DialogResult = true;
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public TrasferDialogViewModel(List<TransferIntoDto> phonesList)
        {
            _phonesList = phonesList;
            ClientPhone = _phonesList.FirstOrDefault();
        }

        public string TransferPhone
        {
            get { return _transferPhone; }
            set { _transferPhone = value; OnPropertyChanged(nameof(TransferPhone));}
        }

        public TransferIntoDto ClientPhone
        {
            get { return _clientPhone; }
            set
            {
                _clientPhone = value;
                OnPropertyChanged(nameof(ClientPhone));
            }
        }

        public List<TransferIntoDto> PhonesList
        {
            get
            {
                return _phonesList;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
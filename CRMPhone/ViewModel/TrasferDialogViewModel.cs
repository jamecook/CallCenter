using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Resources;
using conaito;
using CRMPhone.Annotations;

namespace CRMPhone
{
    public class TrasferDialogViewModel : INotifyPropertyChanged
    {
        private List<string> _phonesList;
        private string _clientPhone;
        private Window _view;

        private bool _canExecute = true;
        private ICommand _transferCommand;
        public ICommand TransferCommand { get { return _transferCommand ?? (_transferCommand = new CommandHandler(Transfer, _canExecute)); } }

        private void Transfer()
        {
            _view.DialogResult = true;
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public TrasferDialogViewModel(List<string> phonesList)
        {
            _phonesList = phonesList;
            ClientPhone = _phonesList.FirstOrDefault();
        }

        public string ClientPhone
        {
            get { return _clientPhone; }
            set
            {
                _clientPhone = value;
                OnPropertyChanged(nameof(ClientPhone));
            }
        }

        public List<string> PhonesList
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
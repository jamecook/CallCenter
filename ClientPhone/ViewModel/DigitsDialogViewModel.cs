using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using SIPEVOActiveXLib;

namespace CRMPhone.ViewModel
{
    public class DigitsDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;
        public DigitsDialogViewModel()
        {
        }

        private ICommand _digit1Command;
        private ICommand _digitCommand;

        public ICommand DigitCommand { get { return _digitCommand ?? (_digitCommand = new RelayCommand(Digit)); } }
        private void Digit(object digit)
        {
            SendDigit(digit.ToString());
        }

        private void SendDigit(string digit)
        {
            var lines = AppSettings.SipLines as ObservableCollection<SipLine>;
            var activeLine = lines?.FirstOrDefault(l => l.State == "Connect");
            if(activeLine == null)
                return;
            var sipAgent = AppSettings.NewSipAgent as SIPClientCtl;
            sipAgent?.SendDTMF(digit);
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class AlertForHouseDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;


 
        private ICommand _closeCommand;
        private ObservableCollection<AlertDto> _alertList;
        public ICommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new CommandHandler(Close, true)); } }

        private void Close()
        {
            _view.DialogResult = false;
        }
        public void SetView(Window view)
        {
            _view = view;
        }

        public ObservableCollection<AlertDto> AlertList
        {
            get { return _alertList; }
            set { _alertList = value; OnPropertyChanged(nameof(AlertList));}
        }

        public AlertForHouseDialogViewModel(List<AlertDto> alertList)
        {
            AlertList = new ObservableCollection<AlertDto>(alertList);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
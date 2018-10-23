using System;
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
    public class SelectPeriodDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; OnPropertyChanged(nameof(FromDate));}
        }

        public DateTime ToDate
        {
            get { return _toDate; }
            set { _toDate = value; OnPropertyChanged(nameof(ToDate)); }
        }

 private ICommand _okCommand;
        public ICommand OkCommand { get { return _okCommand ?? (_okCommand = new RelayCommand(Ok)); } }

        private void Ok(object sender)
        {
            _view.DialogResult = true;
        }

        private DateTime _fromDate;
        private DateTime _toDate;

        private ICommand _cancelCommand;
        public ICommand CancelCommand { get { return _cancelCommand ?? (_cancelCommand = new CommandHandler(Cancel, true)); } }

        private void Cancel()
        {
            _view.DialogResult = false;
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
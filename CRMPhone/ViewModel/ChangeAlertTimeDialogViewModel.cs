using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class ChangeAlertTimeDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        //private ObservableCollection<WorkerHistoryDto> _workerHistoryList;

        private ObservableCollection<AlertTimeDto> _alertTimes;
        private AlertTimeDto _selectedTime;

        public ObservableCollection<AlertTimeDto> AlertTimes
        {
            get { return _alertTimes; }
            set { _alertTimes = value; OnPropertyChanged(nameof(AlertTimes));}
        }

        public ChangeAlertTimeDialogViewModel(AlertTimeDto[] alertTimes)
        {
            AlertTimes = new ObservableCollection<AlertTimeDto>(alertTimes);
            SelectedTime = AlertTimes.FirstOrDefault();
        }


        public AlertTimeDto SelectedTime
        {
            get { return _selectedTime; }
            set { _selectedTime = value; OnPropertyChanged(nameof(SelectedTime));}
        }

        public void SetView(Window view)
        {
            _view = view;
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new RelayCommand(Refresh)); } }
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
        //public ObservableCollection<WorkerHistoryDto> WorkerHistoryList
        //{
        //    get { return _workerHistoryList; }
        //    set { _workerHistoryList = value; OnPropertyChanged(nameof(WorkerHistoryList));}
        //}

        public void Refresh(object sender)
        {
            //    //WorkerHistoryList = new ObservableCollection<WorkerHistoryDto>(_requestService.GetMasterHistoryByRequest(_requestId));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
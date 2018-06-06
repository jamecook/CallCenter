using System;
using System.Collections.Generic;
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
            var times = new List<AlertTimeDto>();
            for (int i = 9; i < 21; i++)
            {
                times.Add(new AlertTimeDto {Id=i,Name = string.Format("{0}:00",i), AddMinutes = i*60});
            }
            AlertDateTimes = new ObservableCollection<AlertTimeDto>(times);
            SelectedDateTime = AlertDateTimes.FirstOrDefault();
            SelectedDate = DateTime.Now;
            ByTime = true;
        }


        public AlertTimeDto SelectedTime
        {
            get { return _selectedTime; }
            set { _selectedTime = value; OnPropertyChanged(nameof(SelectedTime));}
        }

        public ObservableCollection<AlertTimeDto> AlertDateTimes
        {
            get { return _alertDateTimes; }
            set { _alertDateTimes = value; OnPropertyChanged(nameof(AlertDateTimes));}
        }

        public AlertTimeDto SelectedDateTime
        {
            get { return _selectedDateTime; }
            set { _selectedDateTime = value; OnPropertyChanged(nameof(SelectedDateTime));}
        }

        public bool ByTime
        {
            get { return _byTime; }
            set { _byTime = value; OnPropertyChanged(nameof(ByTime));}
        }

        public bool ByDate
        {
            get { return _byDate; }
            set { _byDate = value; OnPropertyChanged(nameof(ByDate));}
        }

        public DateTime? SelectedDate
        {
            get { return _selectedDate; }
            set { _selectedDate = value; OnPropertyChanged(nameof(SelectedDate));}
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
        private bool _byTime;
        private bool _byDate;
        private DateTime? _selectedDate;
        private ObservableCollection<AlertTimeDto> _alertDateTimes;
        private AlertTimeDto _selectedDateTime;
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class SetWorkingTimesDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private string _spentTime;
        private string _fromHour;
        private string _fromMinute;
        private string _toHour;
        private string _toMinute;

        public SetWorkingTimesDialogViewModel(DateTime? fromTime,DateTime? toTime)
        {
            FromHoursList = new List<string>();
            FromMinutesList = new List<string>();
            ToHoursList = new List<string>();
            ToMinutesList = new List<string>();
            for (int i = 0; i <= 23; i++)
            {
                FromHoursList.Add(i.ToString().PadLeft(2, '0'));
                ToHoursList.Add(i.ToString().PadLeft(2, '0'));
                
            }
            for (int i = 0; i <= 55; i += 5)
            {
                FromMinutesList.Add(i.ToString().PadLeft(2, '0'));
                ToMinutesList.Add(i.ToString().PadLeft(2, '0'));
            }
            if (fromTime.HasValue && toTime.HasValue)
            {
                _fromHour = FromHoursList.FirstOrDefault(x => x == fromTime.Value.Hour.ToString().PadLeft(2, '0'));
                _toHour = ToHoursList.FirstOrDefault(x => x == toTime.Value.Hour.ToString().PadLeft(2, '0'));
                _fromMinute =
                    FromMinutesList.FirstOrDefault(x => x == fromTime.Value.Minute.ToString().PadLeft(2, '0'));
                ToMinute = ToMinutesList.FirstOrDefault(x => x == toTime.Value.Minute.ToString().PadLeft(2, '0'));
            }
            else
            {
                _fromHour = FromHoursList.FirstOrDefault();
                _toHour = ToHoursList.FirstOrDefault();
                _fromMinute = FromMinutesList.FirstOrDefault();
                _toMinute = ToMinutesList.FirstOrDefault();
            }

        }
        public List<string> FromHoursList { get; set; }
        public List<string> FromMinutesList { get; set; }
        public List<string> ToHoursList { get; set; }
        public List<string> ToMinutesList { get; set; }

        public string FromHour
        {
            get { return _fromHour; }
            set
            {
                _fromHour = value;
                SpentTime = CalculateTimes();
            }
        }


        public string FromMinute
        {
            get { return _fromMinute; }
            set { _fromMinute = value; SpentTime = CalculateTimes(); }
        }

        public string ToHour
        {
            get { return _toHour; }
            set { _toHour = value; SpentTime = CalculateTimes(); }
        }

        public string ToMinute
        {
            get { return _toMinute; }
            set { _toMinute = value; SpentTime = CalculateTimes(); }
        }

        private string CalculateTimes()
        {
            var fromTime = DateTime.ParseExact($"01.01.0001 {_fromHour}:{_fromMinute}", "dd.MM.yyyy HH:mm", null);
            var toTime = DateTime.ParseExact($"01.01.0001 {_toHour}:{_toMinute}", "dd.MM.yyyy HH:mm", null);
            if (toTime < fromTime)
                toTime = toTime.AddDays(1);
            var spentTime = (toTime - fromTime);
            return $"{spentTime.Hours:00}:{spentTime.Minutes:00}";
        }

        public string SpentTime
        {
            get { return _spentTime; }
            set { _spentTime = value; OnPropertyChanged(nameof(SpentTime));}
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object obj)
        {
                _view.DialogResult = true;
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
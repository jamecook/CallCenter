using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.ViewModel;
using RequestServiceImpl.Dto;

namespace CRMPhone
{
    public class ChangeExecuteDateDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _requestId;
        private ObservableCollection<PeriodDto> _periodList;
        private PeriodDto _selectedPeriod;
        private string _description;
        private DateTime? _selectedDateTime;


        public ChangeExecuteDateDialogViewModel(RequestServiceImpl.RequestService requestService, int requestId)
        {
            _requestService = requestService;
            _requestId = requestId;
            PeriodList = new ObservableCollection<PeriodDto>(_requestService.GetPeriods());
            var request = _requestService.GetRequest(_requestId);
            if (request.ExecuteDate.HasValue && request.ExecuteDate.Value.Date > DateTime.MinValue)
            {
                SelectedDateTime = request.ExecuteDate.Value.Date;
                SelectedPeriod = PeriodList.SingleOrDefault(i => i.Id == request.PeriodId);
                OldDateTime = SelectedDateTime;
                OldPeriod = SelectedPeriod;
            }
            Refresh(null);
        }

        public ObservableCollection<PeriodDto> PeriodList
        {
            get { return _periodList; }
            set { _periodList = value; OnPropertyChanged(nameof(PeriodList)); }
        }

        public PeriodDto SelectedPeriod
        {
            get { return _selectedPeriod; }
            set { _selectedPeriod = value; OnPropertyChanged(nameof(SelectedPeriod)); }
        }

        public DateTime? SelectedDateTime
        {
            get { return _selectedDateTime; }
            set { _selectedDateTime = value; OnPropertyChanged(nameof(SelectedDateTime)); }
        }

        public DateTime? OldDateTime { get; set; }

        public PeriodDto OldPeriod { get; set; }

        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public void SetView(Window view)
        {
            _view = view;
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new RelayCommand(Refresh)); } }
        private ICommand _saveCommand;
        private ObservableCollection<ExecuteDateHistoryDto> _dateHistoryList;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        public ObservableCollection<ExecuteDateHistoryDto> DateHistoryList
        {
            get { return _dateHistoryList; }
            set { _dateHistoryList = value; OnPropertyChanged(nameof(DateHistoryList));}
        }

        private void Save(object sender)
        {
            if (SelectedPeriod != null && SelectedDateTime.HasValue && (SelectedPeriod?.Id != (OldPeriod?.Id ?? 0) || SelectedDateTime != OldDateTime))
            {
                _requestService.AddNewExecuteDate(_requestId, SelectedDateTime.Value, SelectedPeriod, Description);
                OldDateTime = SelectedDateTime;
                OldPeriod = SelectedPeriod;
                _view.DialogResult = true;
            }
        }

        private void Refresh(object sender)
        {
            DateHistoryList = new ObservableCollection<ExecuteDateHistoryDto>(_requestService.GetExecuteDateHistoryByRequest(_requestId));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
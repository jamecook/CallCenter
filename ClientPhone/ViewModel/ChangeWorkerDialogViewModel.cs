using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ClientPhone.Services;
using CRMPhone.Annotations;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class ChangeWorkerDialogViewModel :INotifyPropertyChanged
    {
        private Window _view;

        private int _requestId;
        private ObservableCollection<WorkerDto> _workerList;
        private WorkerDto _selectedWorker;
        private ObservableCollection<WorkerHistoryDto> _workerHistoryList;
        private int? _oldMasterId;

        public ChangeWorkerDialogViewModel(int requestId)
        {
            _requestId = requestId;
            WorkerList = new ObservableCollection<WorkerDto>();
            WorkerList.Add(new WorkerDto()
            {
                Id = -1,
                SurName = "Нет мастера"
            });
            foreach (var master in RestRequestService.GetMasters(AppSettings.CurrentUser.Id,null, true))
            {
                WorkerList.Add(master);
            }
            var request = RestRequestService.GetRequest(AppSettings.CurrentUser.Id, _requestId);
            _oldMasterId = request.MasterId;
            SelectedWorker = WorkerList.SingleOrDefault(w => w.Id == (request.MasterId??-1));
            Refresh(null);
        }
        public string WorkerTitle { get; set; }
        public void SetView(Window view)
        {
            _view = view;
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new RelayCommand(Refresh)); } }
        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        public DateTime FromTime { get; set; }

        private void Save(object sender)
        {
            var t = FromTime;
            if (_oldMasterId == SelectedWorker.Id)
                return;
            RestRequestService.AddNewMaster(AppSettings.CurrentUser.Id, _requestId, (SelectedWorker.Id == -1) ? (int?)null : SelectedWorker.Id);
            _oldMasterId = SelectedWorker.Id;
            _view.DialogResult = true;
        }

        public int? MasterId => _oldMasterId;

        public ObservableCollection<WorkerHistoryDto> WorkerHistoryList
        {
            get { return _workerHistoryList; }
            set { _workerHistoryList = value; OnPropertyChanged(nameof(WorkerHistoryList));}
        }

        public ObservableCollection<WorkerDto> WorkerList
        {
            get { return _workerList; }
            set { _workerList = value; OnPropertyChanged(nameof(WorkerList)); }
        }

        public void Refresh(object sender)
        {
            WorkerHistoryList = new ObservableCollection<WorkerHistoryDto>(RestRequestService.GetMasterHistoryByRequest(AppSettings.CurrentUser.Id, _requestId));
        }

        public WorkerDto SelectedWorker
        {
            get { return _selectedWorker; }
            set { _selectedWorker = value; OnPropertyChanged(nameof(SelectedWorker)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
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
    public class ChangeWorkerDialogViewModel :INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _requestId;
        private ObservableCollection<WorkerDto> _workerList;
        private WorkerDto _selectedWorker;
        private ObservableCollection<WorkerHistoryDto> _workerHistoryList;
        private int? _oldExecuterId;

        public ChangeWorkerDialogViewModel(RequestServiceImpl.RequestService requestService,int requestId)
        {
            _requestService = requestService;
            _requestId = requestId;
            WorkerList = new ObservableCollection<WorkerDto>(_requestService.GetWorkers(null));
            var request = _requestService.GetRequest(_requestId);
            _oldExecuterId = request.ExecutorId;
            SelectedWorker = WorkerList.SingleOrDefault(w => w.Id == request.ExecutorId);
            Refresh(null);
        }

        public void SetView(Window view)
        {
            _view = view;
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new RelayCommand(Refresh)); } }
        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            if(_oldExecuterId == SelectedWorker.Id)
                return;
            _requestService.AddNewWorker(_requestId,SelectedWorker.Id);
            _oldExecuterId = SelectedWorker.Id;
            _view.DialogResult = true;
        }

        public int? ExecuterId => _oldExecuterId;

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
            WorkerHistoryList = new ObservableCollection<WorkerHistoryDto>(_requestService.GetWorkerHistoryByRequest(_requestId));
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
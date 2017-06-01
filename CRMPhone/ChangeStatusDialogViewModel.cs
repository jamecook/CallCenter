using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dto;
using CRMPhone.ViewModel;
using System.Linq;

namespace CRMPhone
{
    public class ChangeStatusDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestService _requestService;
        private int _requestId;
        private ObservableCollection<StatusDto> _statusList;
        private StatusDto _selectedStatus;
        private ObservableCollection<StatusHistoryDto> _statusHistoryList;
        private int? _oldStatusId;

        public ChangeStatusDialogViewModel(RequestService requestService, int requestId)
        {
            _requestService = requestService;
            _requestId = requestId;
            StatusList = new ObservableCollection<StatusDto>(_requestService.GetRequestStatuses());
            var request = _requestService.GetRequest(_requestId);
            _oldStatusId = request.State.Id;
            SelectedStatus = StatusList.SingleOrDefault(s => s.Id == request.State.Id);
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
            if (_oldStatusId == SelectedStatus.Id)
                return;
            _requestService.AddNewState(_requestId, SelectedStatus.Id);
            _oldStatusId = SelectedStatus.Id;
            _view.DialogResult = true;
        }

        public int? StatusId => _oldStatusId;

        public ObservableCollection<StatusHistoryDto> StatusHistoryList
        {
            get { return _statusHistoryList; }
            set { _statusHistoryList = value; OnPropertyChanged(nameof(StatusHistoryList)); }
        }

        public ObservableCollection<StatusDto> StatusList
        {
            get { return _statusList; }
            set { _statusList = value; OnPropertyChanged(nameof(StatusList)); }
        }

        public void Refresh(object sender)
        {
            StatusHistoryList = new ObservableCollection<StatusHistoryDto>(_requestService.GetStatusHistoryByRequest(_requestId));
        }

        public StatusDto SelectedStatus
        {
            get { return _selectedStatus; }
            set { _selectedStatus = value; OnPropertyChanged(nameof(SelectedStatus)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
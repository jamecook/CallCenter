using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class BindRequestTypeToAddressDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _houseId;
        private ICommand _addCommand;
        private ICommand _deleteCommand;
        private ICommand _deleteSelectedCommand;
        private ObservableCollection<ServiceWithCheckDto> _requestTypeList;
        private ServiceWithCheckDto _selectedRequestType;
        private ObservableCollection<ServiceWithCheckDto> _bindedTypesList;
        private ServiceWithCheckDto _selectedBindedTypes;


        public BindRequestTypeToAddressDialogViewModel(RequestServiceImpl.RequestService requestService, int houseId)
        {
            _requestService = requestService;
            _houseId = houseId;
            RequestTypeList = new ObservableCollection<ServiceWithCheckDto>(_requestService.GetServices(null).Select(s => new ServiceWithCheckDto()
            {
                Id = s.Id,
                Name = s.Name,
                Immediate = s.Immediate,
                CanSendSms = s.CanSendSms,
                Checked = false
            }).ToList());
            RefreshList();
            if (RequestTypeList.Count > 0)
            {
                SelectedRequestType = RequestTypeList.FirstOrDefault();
            }

        }

        public void SetView(Window view)
        {
            _view = view;
        }

        public ObservableCollection<ServiceWithCheckDto> RequestTypeList
        {
            get { return _requestTypeList; }
            set { _requestTypeList = value; OnPropertyChanged(nameof(RequestTypeList)); }
        }

        public void RefreshList()
        {
            BindedTypesList = new ObservableCollection<ServiceWithCheckDto>(_requestService.GetBindedTypeToHouse(_houseId).Select(s => new ServiceWithCheckDto()
            {
                Id = s.Id,
                Name = s.Name,
                Immediate = s.Immediate,
                CanSendSms = s.CanSendSms,
                Checked = false
            }).ToList());
        }
        public ServiceWithCheckDto SelectedRequestType
        {
            get { return _selectedRequestType; }
            set
            {
                _selectedRequestType = value;
                OnPropertyChanged(nameof(SelectedRequestType));
            }
        }

        public ObservableCollection<ServiceWithCheckDto> BindedTypesList
        {
            get { return _bindedTypesList; }
            set { _bindedTypesList = value; OnPropertyChanged(nameof(BindedTypesList)); }
        }

        public ServiceWithCheckDto SelectedBindedTypes
        {
            get { return _selectedBindedTypes; }
            set { _selectedBindedTypes = value; OnPropertyChanged(nameof(SelectedBindedTypes)); }
        }

        private void Delete(object sender)
        {
            var item = sender as ServiceWithCheckDto;
            if (item is null)
                return;
            if (MessageBox.Show(_view, $"Вы уверены что хотите удалить запись?", "Привязка", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _requestService.DeleteBindedTypeToHouse(_houseId, item.Id);
                RefreshList();
            }
        }
        private void DeleteSelected(object sender)
        {
            foreach (var item in BindedTypesList.Where(i=>i.Checked))
            {
                _requestService.DeleteBindedTypeToHouse(_houseId, item.Id);
            }
            RefreshList();
        }
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(Delete)); } }

        public ICommand DeleteSelectedCommand { get { return _deleteSelectedCommand ?? (_deleteSelectedCommand = new RelayCommand(DeleteSelected)); } }

        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(AddCompany)); } }
        private void AddCompany(object obj)
        {
            try
            {
                _requestService.AddBindedTypeToHouse(_houseId, SelectedRequestType.Id);
            }
            catch(Exception ex)
            {
                MessageBox.Show(_view, ex.Message);
            }
            RefreshList();
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
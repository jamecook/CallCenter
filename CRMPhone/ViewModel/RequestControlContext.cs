using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using CRMPhone.Annotations;
using CRMPhone.Dto;
using Microsoft.Win32;
using MySql.Data.MySqlClient;

namespace CRMPhone.ViewModel
{
    public class RequestControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<RequestForListDto> _requestList;
        private RequestService _requestService;

        private ICommand _addRequestCommand;
        public ICommand AddRequestCommand { get { return _addRequestCommand ?? (_addRequestCommand = new CommandHandler(AddRequest, true)); } }
        private ICommand _refreshRequestCommand;
        public ICommand RefreshRequestCommand { get { return _refreshRequestCommand ?? (_refreshRequestCommand = new CommandHandler(RefreshRequest, true)); } }
        private ICommand _exportRequestCommand;
        public ICommand ExportRequestCommand { get { return _exportRequestCommand ?? (_exportRequestCommand = new CommandHandler(ExportRequest, true)); } }

        private void ExportRequest()
        {
            if (RequestList.Count == 0)
            {
                MessageBox.Show("Нельзя экспортировать пустой список!","Ошибка");
                return;
            }
            try
            {

                var saveDialog = new SaveFileDialog();
                saveDialog.AddExtension = true;
                saveDialog.DefaultExt = ".xml";
                saveDialog.Filter = "XML Файл|*.xml";
                if (saveDialog.ShowDialog() == true)
                {
                    var fileName = saveDialog.FileName;


                    XElement root = new XElement("Records");
                    foreach (var request in RequestList)
                    {
                        root.AddFirst(
                            new XElement("Record",
                                new []
                                {
                                    new XElement("Заявка", request.Id),
                                    new XElement("ДатаСоздания", request.CreateTime.ToString("dd.MM.yyyy HH:mm")),
                                    new XElement("Создатель", request.CreateUser.ShortName),
                                    new XElement("Улица", request.StreetName),
                                    new XElement("Дом", request.Building),
                                    new XElement("Корпус", request.Corpus),
                                    new XElement("Квартира", request.Flat),
                                    new XElement("Телефоны", request.ContactPhones),
                                    new XElement("Услуга", request.ParentService),
                                    new XElement("Причина", request.Service),
                                    new XElement("Примечание", request.Description),
                                    new XElement("Дата", request.ExecuteTime?.Date.ToString("dd.MM.yyyy") ?? ""),
                                    new XElement("Время", request.ExecutePeriod),
                                    new XElement("Исполнитель", request.Worker?.ShortName),
                                }));


                    }
                    var saver = new FileStream(fileName, FileMode.Create);
                    root.Save(saver);
                    saver.Close();
                    MessageBox.Show("Данные сохранены в файл\r\n" + fileName);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Произошла ошибка:\r\n" + exc.Message);
            }

        }

        private ICommand _openRequestCommand;
        private DateTime _fromDate;
        private DateTime _toDate;
        private ObservableCollection<StreetDto> _streetList;
        private StreetDto _selectedStreet;
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;
        private ObservableCollection<FlatDto> _flatList;
        private FlatDto _selectedFlat;
        private ObservableCollection<WorkerDto> _workerList;
        private WorkerDto _selectedWorker;
        private ObservableCollection<ServiceDto> _parentServiceList;
        private ServiceDto _selectedParentService;
        private ObservableCollection<ServiceDto> _serviceList;
        private ServiceDto _selectedService;
        private ObservableCollection<StatusDto> _statusList;
        private StatusDto _selectedList;

        public ICommand OpenRequestCommand { get { return _openRequestCommand ?? (_openRequestCommand = new RelayCommand(OpenRequest));} }

        public string RequestNum { get; set; }

        public ObservableCollection<StreetDto> StreetList
        {
            get { return _streetList; }
            set { _streetList = value; OnPropertyChanged(nameof(StreetList)); }
        }

        public StreetDto SelectedStreet
        {
            get { return _selectedStreet; }
            set
            {
                _selectedStreet = value;
                ChangeStreet(value?.Id);
                OnPropertyChanged(nameof(SelectedStreet));
            }
        }

        public ObservableCollection<HouseDto> HouseList
        {
            get { return _houseList; }
            set { _houseList = value; OnPropertyChanged(nameof(HouseList)); }
        }

        public HouseDto SelectedHouse
        {
            get { return _selectedHouse; }
            set
            {
                _selectedHouse = value;
                ChangeHouse(value?.Id);
                OnPropertyChanged(nameof(SelectedHouse));
            }
        }

        public ObservableCollection<FlatDto> FlatList
        {
            get { return _flatList; }
            set { _flatList = value; OnPropertyChanged(nameof(FlatList)); }
        }

        public FlatDto SelectedFlat
        {
            get { return _selectedFlat; }
            set { _selectedFlat = value; OnPropertyChanged(nameof(SelectedFlat)); }
        }

        private void ChangeStreet(int? streetId)
        {
            HouseList.Clear();
            if (!streetId.HasValue)
                return;
            foreach (var house in _requestService.GetHouses(streetId.Value).OrderBy(s => s.Building?.PadLeft(6, '0')).ThenBy(s => s.Corpus?.PadLeft(6, '0')))
            {
                HouseList.Add(house);
            }
            OnPropertyChanged(nameof(HouseList));
        }

        public ServiceDto SelectedParentService
        {
            get { return _selectedParentService; }
            set
            {
                _selectedParentService = value;
                ChangeParentService(value?.Id);
                OnPropertyChanged(nameof(SelectedParentService));
            }
        }
        public ObservableCollection<ServiceDto> ParentServiceList
        {
            get { return _parentServiceList; }
            set { _parentServiceList = value; OnPropertyChanged(nameof(ParentServiceList)); }
        }

        public ObservableCollection<ServiceDto> ServiceList
        {
            get { return _serviceList; }
            set { _serviceList = value; OnPropertyChanged(nameof(ServiceList)); }
        }

        public ServiceDto SelectedService
        {
            get { return _selectedService; }
            set { _selectedService = value; OnPropertyChanged(nameof(SelectedService)); }
        }

        public ObservableCollection<WorkerDto> WorkerList
        {
            get { return _workerList; }
            set { _workerList = value; OnPropertyChanged(nameof(WorkerList)); }
        }

        public WorkerDto SelectedWorker
        {
            get { return _selectedWorker; }
            set { _selectedWorker = value; OnPropertyChanged(nameof(SelectedWorker)); }
        }

        private void ChangeParentService(int? parentServiceId)
        {
            ServiceList.Clear();
            if (!parentServiceId.HasValue)
                return;
            foreach (var source in _requestService.GetServices(parentServiceId.Value).OrderBy(s => s.Name))
            {
                ServiceList.Add(source);
            }
            OnPropertyChanged(nameof(ServiceList));
        }

        private void ChangeHouse(int? houseId)
        {
            FlatList.Clear();
            if (!houseId.HasValue)
                return;
            foreach (var flat in _requestService.GetFlats(houseId.Value).OrderBy(s => s.TypeId).ThenBy(s => s.Flat?.PadLeft(6, '0')))
            {
                FlatList.Add(flat);
            }
            OnPropertyChanged(nameof(FlatList));
        }

        private void ChangeCity(int? cityId)
        {
            StreetList.Clear();
            if (!cityId.HasValue)
                return;
            foreach (var street in _requestService.GetStreets(cityId.Value).OrderBy(s => s.Name))
            {
                StreetList.Add(street);
            }
            OnPropertyChanged(nameof(StreetList));
        }

        private void OpenRequest(object sender)
        {
            var selectedItem = sender as RequestForListDto;
            if (selectedItem == null)
                return;
            if (_requestService == null)
                _requestService = new RequestService(AppSettings.DbConnection);

            var request = _requestService.GetRequest(selectedItem.Id);
            if (request == null)
            {
                MessageBox.Show("Произошла непредвиденная ошибка!");
                return;
            }

            var viewModel = new RequestDialogViewModel();
            var view = new RequestDialog(viewModel);
            viewModel.SetView(view);
            viewModel.SelectedCity = viewModel.CityList.SingleOrDefault(i=>i.Id == request.Address.CityId);
            viewModel.SelectedStreet = viewModel.StreetList.SingleOrDefault(i => i.Id == request.Address.StreetId);
            viewModel.SelectedHouse = viewModel.HouseList.SingleOrDefault(i=>i.Id == request.Address.HouseId);
            viewModel.SelectedFlat =  viewModel.FlatList.SingleOrDefault(i=>i.Id == request.Address.Id);
            viewModel.Floor = request.Floor;
            viewModel.Entrance = request.Entrance;
            var requestModel = viewModel.RequestList.FirstOrDefault();
            requestModel.SelectedParentService = requestModel.ParentServiceList.SingleOrDefault(i => i.Id == request.Type.ParentId);
            requestModel.SelectedService = requestModel.ServiceList.SingleOrDefault(i => i.Id == request.Type.Id);
            requestModel.Description = request.Description;
            requestModel.IsChargeable = request.IsChargeable;
            requestModel.IsImmediate = request.IsImmediate;
            requestModel.RequestCreator = request.CreateUser.ShortName;
            requestModel.RequestDate = request.CreateTime;
            requestModel.RequestState = request.State.Description;
            requestModel.SelectedWorker = requestModel.WorkerList.SingleOrDefault(w => w.Id == request.ExecutorId);
            requestModel.RequestId = request.Id;
            requestModel.Rating = request.Rating;
            if (request.ExecuteDate.HasValue && request.ExecuteDate.Value.Date > DateTime.MinValue)
            {
                requestModel.SelectedDateTime = request.ExecuteDate.Value.Date;
                requestModel.SelectedPeriod = requestModel.PeriodList.SingleOrDefault(i => i.Id == request.PeriodId);
            }
            viewModel.RequestId = request.Id;
            viewModel.ContactList = new ObservableCollection<ContactDto>(request.Contacts);
            var t = view.ShowDialog();

        }

        private void RefreshRequest()
        {
            if(_requestService == null)
                _requestService = new RequestService(AppSettings.DbConnection);
            RequestList.Clear();
            var requests = string.IsNullOrEmpty(RequestNum) ? _requestService.GetRequestList(FromDate,ToDate,SelectedStreet?.Id,_selectedHouse?.Id,SelectedFlat?.Id,SelectedParentService?.Id,SelectedService?.Id,SelectedStatus?.Id,SelectedWorker?.Id)
                :_requestService.GetRequestById(RequestNum);
            foreach (var request in requests)
            {
                RequestList.Add(request);
            }
            OnPropertyChanged(nameof(RequestList));
        }

        public RequestControlContext()
        {
            RequestList = new ObservableCollection<RequestForListDto>();
            FromDate = DateTime.Today;
            ToDate = DateTime.Today;
        }

        public void InitCollections()
        {
            _requestService = new RequestService(AppSettings.DbConnection);
            StreetList = new ObservableCollection<StreetDto>();
            HouseList = new ObservableCollection<HouseDto>();
            FlatList = new ObservableCollection<FlatDto>();
            ServiceList = new ObservableCollection<ServiceDto>();
            WorkerList = new ObservableCollection<WorkerDto>(_requestService.GetWorkers(null));
            StatusList = new ObservableCollection<StatusDto>(_requestService.GetRequestStatuses());
            ParentServiceList = new ObservableCollection<ServiceDto>(_requestService.GetServices(null));

            ChangeCity(_requestService.GetCities().FirstOrDefault().Id);
        }
        public ObservableCollection<RequestForListDto> RequestList
        {
            get { return _requestList; }
            set { _requestList = value; OnPropertyChanged(nameof(RequestList));}
        }

        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; OnPropertyChanged(nameof(FromDate));}
        }

        public DateTime ToDate
        {
            get { return _toDate; }
            set { _toDate = value; OnPropertyChanged(nameof(ToDate));}
        }

        public ObservableCollection<StatusDto> StatusList
        {
            get { return _statusList; }
            set { _statusList = value; OnPropertyChanged(nameof(StatusList)); }
        }

        public StatusDto SelectedStatus
        {
            get { return _selectedList; }
            set { _selectedList = value; OnPropertyChanged(nameof(SelectedStatus)); }
        }

        private void AddRequest()
        {
            var viewModel = new RequestDialogViewModel();
            var view = new RequestDialog(viewModel);
            viewModel.SetView(view);
            var t = view.ShowDialog();

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
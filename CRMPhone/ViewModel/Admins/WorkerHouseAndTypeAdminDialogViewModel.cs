using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class WorkerHouseAndTypeAdminDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _workerId;
        private ICommand _addCommand;
        private ICommand _deleteCommand;
        private ICommand _deleteAllCommand;
        private ObservableCollection<CityDto> _cityList;
        private CityDto _selectedCity;
        private ObservableCollection<StreetDto> _streetList;
        private StreetDto _selectedStreet;
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;
        private ObservableCollection<ServiceDto> _parentServiceList;
        private ServiceDto _selectedParentService;
        private int _weigth;
        private ObservableCollection<WorketHouseAndTypeListDto> _houseAndTypeList;
        private ObservableCollection<FieldForFilterDto> _filterHouseList;
        private ObservableCollection<FieldForFilterDto> _filterParentServiceList;

        public ObservableCollection<ServiceDto> ParentServiceList
        {
            get { return _parentServiceList; }
            set { _parentServiceList = value; OnPropertyChanged(nameof(ParentServiceList)); }
        }

        public ObservableCollection<CityDto> CityList
        {
            get { return _cityList; }
            set { _cityList = value; OnPropertyChanged(nameof(CityList)); }
        }


        public WorkerHouseAndTypeAdminDialogViewModel(RequestServiceImpl.RequestService requestService, int workerId)
        {
            _requestService = requestService;
            _workerId = workerId;
            Weigth = 100;
            var services = new[] {new ServiceDto {Id = 0, Name = "Все"}}.Concat(_requestService.GetServices(null));
            FilterParentServiceList = new ObservableCollection<FieldForFilterDto>(services.Select(w => new FieldForFilterDto()
            {
                Id = w.Id,
                Name = w.Name,
                Selected = false
            }));
            ServiceList = new ObservableCollection<FieldForFilterDto>();
            foreach (var service in FilterParentServiceList)
            {
                service.PropertyChanged += ServiceOnPropertyChanged;
            }

            StreetList = new ObservableCollection<StreetDto>();
            FilterHouseList = new ObservableCollection<FieldForFilterDto>();
            CityList = new ObservableCollection<CityDto>(_requestService.GetCities());
            RefreshList();
            if (CityList.Count > 0)
            {
                SelectedCity = CityList.FirstOrDefault();
            }
        }

        private void ServiceOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var item = sender as FieldForFilterDto;
            if (item != null && item.Selected)
            {
                if (item.Id == 0)
                {
                    foreach (var service in FilterParentServiceList.Where(s => s.Id > 0))
                    {
                        service.Selected = false;
                    }

                }
                else
                {
                    var service = FilterParentServiceList.FirstOrDefault(s => s.Id == 0);
                    if (service != null)
                        service.Selected = false;
                }


            }
            if (FilterParentServiceList.Count(f => f.Selected) == 1)
            {
                ChangeParentService(FilterParentServiceList.FirstOrDefault(f => f.Selected)?.Id);
            }
            else
            {
                ChangeParentService(null);
            }
        }

        public void RefreshList()
        {
            HouseAndTypeList = new ObservableCollection<WorketHouseAndTypeListDto>(_requestService.GetHouseAndTypesByWorkerId(_workerId));
        }
        public CityDto SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                _selectedCity = value;
                ChangeCity(value?.Id);
                OnPropertyChanged(nameof(SelectedCity));
            }
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

        private void ChangeStreet(int? streetId)
        {
            FilterHouseList.Clear();
            if (!streetId.HasValue)
                return;
            foreach (var house in _requestService.GetHouses(streetId.Value)
                .OrderBy(s => s.Building?.PadLeft(6, '0'))
                .ThenBy(s => s.Corpus?.PadLeft(6, '0'))
                .Select(w => new FieldForFilterDto()
                {
                    Id = w.Id,
                    Name = w.FullName,
                    Selected = false
                }))
            {
                FilterHouseList.Add(house);
            }
            OnPropertyChanged(nameof(FilterHouseList));
        }
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

        public ObservableCollection<FieldForFilterDto> FilterParentServiceList
        {
            get { return _filterParentServiceList; }
            set { _filterParentServiceList = value; OnPropertyChanged(nameof(FilterParentServiceList));}
        }
        private ObservableCollection<FieldForFilterDto> _serviceList;

        public ObservableCollection<FieldForFilterDto> ServiceList
        {
            get { return _serviceList; }
            set { _serviceList = value; OnPropertyChanged(nameof(ServiceList)); }
        }
        private void ChangeParentService(int? parentServiceId)
        {
            ServiceList.Clear();
            if (!parentServiceId.HasValue)
                return;

            ServiceList = new ObservableCollection<FieldForFilterDto>(_requestService.GetServices(parentServiceId.Value).OrderBy(s => s.Name).Select(w => new FieldForFilterDto()
            {
                Id = w.Id,
                Name = w.Name,
                Selected = false
            }));
            OnPropertyChanged(nameof(ServiceList));
        }

        public ObservableCollection<FieldForFilterDto> FilterHouseList
        {
            get { return _filterHouseList; }
            set { _filterHouseList = value; OnPropertyChanged(nameof(FilterHouseList));}
        }

        public int Weigth
        {
            get { return _weigth; }
            set { _weigth = value; OnPropertyChanged(nameof(Weigth)); }
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(Add)); } }
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(Delete)); } }
        public ICommand DeleteAllCommand { get { return _deleteAllCommand ?? (_deleteAllCommand = new RelayCommand(DeleteAll)); } }

        private void DeleteAll(object obj)
        {
            if (MessageBox.Show(Application.Current.MainWindow, $"Вы уверены что хотите удалить ВСЕ привязки?",
                    "Адреса и Услуги", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (var item in HouseAndTypeList)
                {
                    _requestService.DeleteHouseAndTypesByWorkerId(item.Id);
                }
                RefreshList();
            }
        }

        public ObservableCollection<WorketHouseAndTypeListDto> HouseAndTypeList
        {
            get { return _houseAndTypeList; }
            set { _houseAndTypeList = value; OnPropertyChanged(nameof(HouseAndTypeList));}
        }

        private void Add(object sender)
        {
            if(!FilterParentServiceList.Any(s => s.Selected) || !FilterHouseList.Any(s => s.Selected))
                return;
            foreach (var houseId in FilterHouseList.Where(h => h.Selected).Select(h => h.Id))
            {
                if (ServiceList.Any(h=>h.Selected))
                {
                    foreach (var serviceId in ServiceList.Where(h => h.Selected).Select(h => h.Id))
                    {
                        _requestService.AddHouseAndTypesForWorker(_workerId, houseId, serviceId, Weigth);
                    }
                }
                else
                {
                    foreach (var serviceId in FilterParentServiceList.Where(h => h.Selected).Select(h => h.Id))
                    {
                        _requestService.AddHouseAndTypesForWorker(_workerId, houseId, serviceId == 0 ? (int?)null : serviceId, Weigth);
                    }
                }
            }
            RefreshList();
        }
        private void Delete(object sender)
        {
            var item = sender as WorketHouseAndTypeListDto;
            if(item is null)
                return;
            if (MessageBox.Show(Application.Current.MainWindow, $"Вы уверены что хотите удалить запись?", "Адреса и Услуги", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _requestService.DeleteHouseAndTypesByWorkerId(item.Id);
                RefreshList();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
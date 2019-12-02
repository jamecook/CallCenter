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
        private ObservableCollection<FieldForFilterDto> _filterServiceList;

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
            var services = new[] {new ServiceDto {Id = 0, Name = "Все"}}.Concat(_requestService.GetServices(null));
            FilterServiceList = new ObservableCollection<FieldForFilterDto>(services.Select(w => new FieldForFilterDto()
            {
                Id = w.Id,
                Name = w.Name,
                Selected = false
            }));
            foreach (var service in FilterServiceList)
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
                    foreach (var service in FilterServiceList.Where(s=>s.Id > 0))
                    {
                        service.Selected = false;
                    }

                }
                else
                {
                    var service = FilterServiceList.FirstOrDefault(s => s.Id == 0);
                    if (service != null)
                        service.Selected = false;
                }

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

        public ObservableCollection<FieldForFilterDto> FilterServiceList
        {
            get { return _filterServiceList; }
            set { _filterServiceList = value; OnPropertyChanged(nameof(FilterServiceList));}
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

        public ObservableCollection<WorketHouseAndTypeListDto> HouseAndTypeList
        {
            get { return _houseAndTypeList; }
            set { _houseAndTypeList = value; OnPropertyChanged(nameof(HouseAndTypeList));}
        }

        private void Add(object sender)
        {
            if(!FilterServiceList.Any(s => s.Selected) || !FilterHouseList.Any(s => s.Selected))
                return;
            foreach (var houseId in FilterHouseList.Where(h => h.Selected).Select(h => h.Id))
            {
                foreach (var serviceId in FilterServiceList.Where(h => h.Selected).Select(h => h.Id))
                {
                    _requestService.AddHouseAndTypesForWorker(_workerId, houseId, serviceId == 0 ? (int?) null : serviceId, Weigth);
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
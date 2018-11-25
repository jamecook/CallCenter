using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class BindAddressToWorkerDialogViewModel : INotifyPropertyChanged
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
        private ObservableCollection<HouseDto> _bindedHouseList;
        private HouseDto _selectedBindedHouse;
        private ObservableCollection<FieldForFilterDto> _filterHouseList;


        public BindAddressToWorkerDialogViewModel(RequestServiceImpl.RequestService requestService, int workerId)
        {
            _requestService = requestService;
            _workerId = workerId;
            StreetList = new ObservableCollection<StreetDto>();
            FilterHouseList = new ObservableCollection<FieldForFilterDto>();
            CityList = new ObservableCollection<CityDto>(_requestService.GetCities());
            RefreshList();
            if (CityList.Count > 0)
            {
                SelectedCity = CityList.FirstOrDefault();
            }

        }

        public void SetView(Window view)
        {
            _view = view;
        }

        public ObservableCollection<CityDto> CityList
        {
            get { return _cityList; }
            set { _cityList = value; OnPropertyChanged(nameof(CityList)); }
        }

        public void RefreshList()
        {
            BindedHouseList = new ObservableCollection<HouseDto>(_requestService.GetBindedToWorkerHouse(_workerId));
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


        public ObservableCollection<FieldForFilterDto> FilterHouseList
        {
            get { return _filterHouseList; }
            set { _filterHouseList = value; OnPropertyChanged(nameof(FilterHouseList)); }
        }

        public ObservableCollection<HouseDto> BindedHouseList
        {
            get { return _bindedHouseList; }
            set { _bindedHouseList = value; OnPropertyChanged(nameof(BindedHouseList)); }
        }

        public HouseDto SelectedBindedHouse
        {
            get { return _selectedBindedHouse; }
            set { _selectedBindedHouse = value; OnPropertyChanged(nameof(SelectedBindedHouse)); }
        }

        private void Delete(object sender)
        {
            var item = sender as HouseDto;
            if (item is null)
                return;
            if (MessageBox.Show(_view, $"Вы уверены что хотите удалить запись?", "Адреса", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _requestService.DeleteBindedToWorkerHouse(_workerId, item.Id);
                RefreshList();
            }
        }
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(Delete)); } }


        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(AddHouse)); } }
        private void AddHouse(object obj)
        {
            if( !FilterHouseList.Any(s => s.Selected))
                return;
            foreach (var houseId in FilterHouseList.Where(h => h.Selected).Select(h => h.Id))
            {
                try
                {
                    _requestService.AddBindedToWorkerHouse(_workerId, houseId);
                }
                catch
                {
                }
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
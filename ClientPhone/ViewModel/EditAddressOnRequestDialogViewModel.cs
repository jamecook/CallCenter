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
    public class EditAddressOnRequestDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;
        private ObservableCollection<CityDto> _cityList;
        private CityDto _selectedCity;
        private ObservableCollection<StreetDto> _streetList;
        private StreetDto _selectedStreet;
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;

        private int _requestId;

        public ObservableCollection<CityDto> CityList
        {
            get { return _cityList; }
            set { _cityList = value; OnPropertyChanged(nameof(CityList));}
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
            foreach (var street in RestRequestService.GetStreets(AppSettings.CurrentUser.Id, cityId.Value).OrderBy(s => s.Name))
            {
                StreetList.Add(street);
            }
            OnPropertyChanged(nameof(StreetList));
        }

        private void ChangeStreet(int? streetId)
        {
            HouseList.Clear();
            if (!streetId.HasValue)
                return;
            foreach (var house in RestRequestService.GetHouses(AppSettings.CurrentUser.Id, streetId.Value).OrderBy(s => s.Building?.PadLeft(6,'0')).ThenBy(s=>s.Corpus?.PadLeft(6, '0')))
            {
                HouseList.Add(house);
            }
            OnPropertyChanged(nameof(HouseList));
        }

        public ObservableCollection<StreetDto> StreetList
        {
            get { return _streetList;}
            set { _streetList = value; OnPropertyChanged(nameof(StreetList));}
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

        public FlatDto SelectedFlat
        {
            get { return _selectedFlat; }
            set { _selectedFlat = value; OnPropertyChanged(nameof(SelectedFlat)); }
        }

        public ObservableCollection<FlatDto> FlatList
        {
            get { return _flatList; }
            set { _flatList = value; OnPropertyChanged(nameof(FlatList)); }
        }

        public ObservableCollection<HouseDto> HouseList
        {
            get { return _houseList; }
            set { _houseList = value; OnPropertyChanged(nameof(HouseList));}
        }

        private void ChangeHouse(int? houseId)
        {
            FlatList.Clear();
            if (!houseId.HasValue)
                return;
            foreach (var flat in RestRequestService.GetFlats(AppSettings.CurrentUser.Id, houseId.Value).OrderBy(s => s.TypeId).ThenBy(s => s.Flat?.PadLeft(6, '0')))
            {
                FlatList.Add(flat);
            }
            OnPropertyChanged(nameof(FlatList));
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

        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            //_requestService.SaveAlert(alert);
            _view.DialogResult = true;
        }

        private ICommand _closeCommand;
        private FlatDto _selectedFlat;
        private ObservableCollection<FlatDto> _flatList;

        public ICommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new CommandHandler(Close, true)); } }

        private void Close()
        {
            _view.DialogResult = false;
        }
        public void SetView(Window view)
        {
            _view = view;
        }
        public EditAddressOnRequestDialogViewModel(int requestId)
        {
            _requestId = requestId;

            StreetList = new ObservableCollection<StreetDto>();
            HouseList = new ObservableCollection<HouseDto>();
            FlatList = new ObservableCollection<FlatDto>();
            CityList = new ObservableCollection<CityDto>(RestRequestService.GetCities(AppSettings.CurrentUser.Id));
            SelectedCity = CityList.FirstOrDefault();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
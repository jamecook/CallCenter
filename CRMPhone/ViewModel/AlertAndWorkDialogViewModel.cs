using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class AlertAndWorkDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private readonly RequestService _requestService;
        private ObservableCollection<CityDto> _cityList;
        private CityDto _selectedCity;
        private ObservableCollection<StreetDto> _streetList;
        private StreetDto _selectedStreet;
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;

        private AlertDto _alert;

        public ObservableCollection<CityDto> CityList
        {
            get { return _cityList; }
            set { _cityList = value; OnPropertyChanged(nameof(CityList));}
        }

        public AlertDto Alert
        {
            get { return _alert; }
            set {
                _alert = value;
                OnPropertyChanged(nameof(Alert));
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(ReadOnly));
            }
        }

        public bool CanEdit { get { return Alert == null; } }
        public bool ReadOnly { get { return !CanEdit; } }
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
            HouseList.Clear();
            if (!streetId.HasValue)
                return;
            foreach (var house in _requestService.GetHouses(streetId.Value).OrderBy(s => s.Building?.PadLeft(6,'0')).ThenBy(s=>s.Corpus?.PadLeft(6, '0')))
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

        public ObservableCollection<HouseDto> HouseList
        {
            get { return _houseList; }
            set { _houseList = value; OnPropertyChanged(nameof(HouseList));}
        }

        public HouseDto SelectedHouse
        {
            get { return _selectedHouse; }
            set
            {
                _selectedHouse = value;
                OnPropertyChanged(nameof(SelectedHouse));
            }
        }

        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; OnPropertyChanged(nameof(FromDate));}
        }

        public DateTime? ToDate
        {
            get { return _toDate; }
            set { _toDate = value; OnPropertyChanged(nameof(ToDate)); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged(nameof(Description));}
        }

        public ObservableCollection<AlertTypeDto> TypeList
        {
            get { return _typeList; }
            set { _typeList = value; OnPropertyChanged(nameof(TypeList));}
        }

        public AlertTypeDto SelectedType
        {
            get { return _selectedType; }
            set { _selectedType = value; OnPropertyChanged(nameof(SelectedType)); }
        }

        public ObservableCollection<AlertServiceTypeDto> ServiceList
        {
            get { return _serviceList; }
            set { _serviceList = value; OnPropertyChanged(nameof(ServiceList));}
        }

        public AlertServiceTypeDto SelectedService
        {
            get { return _selectedService; }
            set { _selectedService = value; OnPropertyChanged(nameof(SelectedService));}
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            AlertDto alert;
            if (Alert != null)
            {
                alert = Alert;
            }
            else
            {
                alert = new AlertDto
                {
                    Id = 0,
                    HouseId = SelectedHouse.Id,
                    Type = new AlertTypeDto() { Id = SelectedType.Id},
                    ServiceType = new AlertServiceTypeDto() { Id = SelectedService.Id},
                };
            }
            alert.StartDate = StartTime.HasValue ? FromDate.Date.Add(StartTime.Value) : FromDate.Date;
            alert.EndDate = EndTime.HasValue ? ToDate?.Date.Add(EndTime.Value) : ToDate?.Date;
            alert.Description = Description;

            _requestService.SaveAlert(alert);
            _view.DialogResult = true;
        }

        private ICommand _closeCommand;
        private DateTime _fromDate;
        private DateTime? _toDate;
        private string _description;
        private ObservableCollection<AlertTypeDto> _typeList;
        private AlertTypeDto _selectedType;
        private ObservableCollection<AlertServiceTypeDto> _serviceList;
        private AlertServiceTypeDto _selectedService;

        public ICommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new CommandHandler(Close, true)); } }

        private void Close()
        {
            _view.DialogResult = false;
        }
        public void SetView(Window view)
        {
            _view = view;
        }
        public TimeSpan? EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(nameof(EndTime)); }
        }

        public TimeSpan? StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); }
        }

        private TimeSpan? _endTime;
        private TimeSpan? _startTime;


        public AlertAndWorkDialogViewModel(AlertDto alert)
        {
            Alert = alert;
            _requestService = new RequestService(AppSettings.DbConnection);

            StreetList = new ObservableCollection<StreetDto>();
            HouseList = new ObservableCollection<HouseDto>();
            CityList = new ObservableCollection<CityDto>(_requestService.GetCities());
            TypeList = new ObservableCollection<AlertTypeDto>(_requestService.GetAlertTypes());
            ServiceList = new ObservableCollection<AlertServiceTypeDto>(_requestService.GetAlertServiceTypes());
            FromDate = _requestService.GetCurrentDate().Date;
            StartTime = Alert?.StartDate.TimeOfDay;
            EndTime = Alert?.EndDate?.TimeOfDay;

            SelectedType = TypeList.FirstOrDefault();
            SelectedService = ServiceList.FirstOrDefault();
            SelectedCity = CityList.FirstOrDefault();
            if (Alert != null)
            {
                SelectedStreet = StreetList.FirstOrDefault(s => s.Id == Alert.StreetId);
                SelectedHouse = HouseList.FirstOrDefault(s => s.Id == Alert.HouseId);
                SelectedType = TypeList.FirstOrDefault(t => t.Id == Alert.Type.Id);
                SelectedService = ServiceList.FirstOrDefault(s => s.Id == Alert.ServiceType.Id);
                Description = Alert.Description;
                FromDate = Alert.StartDate;
                ToDate = Alert.EndDate;
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
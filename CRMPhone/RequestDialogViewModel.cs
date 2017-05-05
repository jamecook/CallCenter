using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dto;
using CRMPhone.ViewModel;

namespace CRMPhone
{
    public class RequestDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private readonly RequestService _requestService;
        private ObservableCollection<CityDto> _cityList;
        private CityDto _selectedCity;
        private ObservableCollection<StreetDto> _streetList;
        private StreetDto _selectedStreet;
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;
        private ObservableCollection<FlatDto> _flatList;
        private FlatDto _selectedFlat;
        private ObservableCollection<ServiceDto> _parentServiceList;
        private ServiceDto _selectedParentService;
        private ObservableCollection<ServiceDto> _serviceList;
        private ServiceDto _selectedService;
        private ObservableCollection<ContactDto> _contactList;
        private int _requestId;

        public ObservableCollection<CityDto> CityList
        {
            get { return _cityList; }
            set { _cityList = value; OnPropertyChanged(nameof(CityList));}
        }

        public int RequestId
        {
            get { return _requestId; }
            set {
                _requestId = value;
                OnPropertyChanged(nameof(RequestId));
                OnPropertyChanged(nameof(CanEdit));
                }
        }

        public bool CanEdit { get { return RequestId == 0; } }
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

        private void ChangeHouse(int? houseId)
        {
            FlatList.Clear();
            if (!houseId.HasValue)
                return;
            foreach (var flat in _requestService.GetFlats(houseId.Value).OrderBy(s => s.TypeId).ThenBy(s => s.Flat?.PadLeft(6,'0')))
            {
                FlatList.Add(flat);
            }
            OnPropertyChanged(nameof(FlatList));
        }

        private void ChangeParentService(int? parentServiceId)
        {
            ServiceList.Clear();
            if (!parentServiceId.HasValue)
                return;
            foreach (var source in _requestService.GetServices(parentServiceId.Value).OrderBy(s=>s.Name))
            {
                ServiceList.Add(source);
            }
            OnPropertyChanged(nameof(ServiceList));
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
                ChangeHouse(value?.Id);
                OnPropertyChanged(nameof(SelectedHouse));
            }
        }

        public ObservableCollection<FlatDto> FlatList
        {
            get { return _flatList; }
            set { _flatList = value; OnPropertyChanged(nameof(FlatList));}
        }

        public FlatDto SelectedFlat
        {
            get { return _selectedFlat; }
            set { _selectedFlat = value; OnPropertyChanged(nameof(SelectedFlat));}
        }

        public ObservableCollection<ServiceDto> ParentServiceList
        {
            get { return _parentServiceList; }
            set { _parentServiceList = value; OnPropertyChanged(nameof(ParentServiceList));}
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

        public ObservableCollection<ServiceDto> ServiceList
        {
            get { return _serviceList; }
            set { _serviceList = value; OnPropertyChanged(nameof(ServiceList));}
        }

        public ServiceDto SelectedService
        {
            get { return _selectedService; }
            set { _selectedService = value; OnPropertyChanged(nameof(SelectedService)); }
        }

        public ObservableCollection<WorkerDto> WorkerList
        {
            get { return _workerList; }
            set { _workerList = value; OnPropertyChanged(nameof(WorkerList));}
        }

        public WorkerDto SelectedWorker
        {
            get { return _selectedWorker; }
            set { _selectedWorker = value; OnPropertyChanged(nameof(SelectedWorker));}
        }

        public ObservableCollection<ContactDto> ContactList
        {
            get { return _contactList; }
            set { _contactList = value; OnPropertyChanged(nameof(ContactList)); }
        }
        private string _requestMessage;
        private ContactDto _selectedContact;

        private ICommand _addContactCommand;
        public ICommand AddContactCommand { get { return _addContactCommand ?? (_addContactCommand = new CommandHandler(AddContact, true)); } }
        private ICommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(Delete, true)); } }

        private ICommand _changeCheckedStateCommand;
        public ICommand ChangeCheckedStateCommand { get { return _changeCheckedStateCommand ?? (_changeCheckedStateCommand = new CommandHandler(ChangeChekedState, true)); } }


        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new CommandHandler(SaveRequest, true)); } }

        private ICommand _closeCommand;
        private ObservableCollection<WorkerDto> _workerList;
        private WorkerDto _selectedWorker;
        private DateTime? _selectedDateTime;
        public ICommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new CommandHandler(Close, true)); } }

        private void ChangeChekedState()
        {
            if (SelectedContact.IsMain)
            {
                foreach (var phone in ContactList.Where(c => !c.Equals(SelectedContact)))
                {
                    phone.IsMain = false;
                }
            }
        }

        private void Close()
        {
            _view.DialogResult = true;
        }

        private void SaveRequest()
        {
            var request = _requestService.SaveNewRequest(SelectedFlat.Id, SelectedService.Id, ContactList.ToArray(), RequestMessage);
            if (!request.HasValue)
            {
                MessageBox.Show("Произошла непредвиденная ошибка!");
                return;
            }
            if (SelectedWorker!= null && SelectedWorker.Id>0)
                _requestService.AddNewWorker(request.Value,SelectedWorker.Id);
            if(SelectedDateTime.HasValue)
                _requestService.AddNewExecuteDate(request.Value, SelectedDateTime.Value,"");

            MessageBox.Show($"Создана заявка №{request}", "Заявка", MessageBoxButton.OK);
        }

        public ContactDto SelectedContact
        {
            get { return _selectedContact; }
            set { _selectedContact = value; OnPropertyChanged(nameof(SelectedContact));}
        }

        public string RequestMessage
        {
            get { return _requestMessage; }
            set { _requestMessage = value; OnPropertyChanged(nameof(RequestMessage));}
        }

        public DateTime? SelectedDateTime
        {
            get { return _selectedDateTime; }
            set { _selectedDateTime = value;OnPropertyChanged(nameof(SelectedDateTime));}
        }

        private void Delete()
        {
            ContactList.Remove(SelectedContact);
        }


        private void AddContact()
        {
            ContactList.Add(new ContactDto());
        }


        public void SetView(Window view)
        {
            _view = view;
        }

        public RequestDialogViewModel()
        {
            _requestService = new RequestService(AppSettings.DbConnection);

            //_requestService.AddNewWorker(2,1);
            //_requestService.AddNewDescription(2,"Проверка добавления");
            //_requestService.AddNewExecuteDate(2,DateTime.Now, "Описание");
            //_requestService.AddNewNote(2, "Note1");

            StreetList = new ObservableCollection<StreetDto>();
            HouseList = new ObservableCollection<HouseDto>();
            FlatList = new ObservableCollection<FlatDto>();
            ServiceList = new ObservableCollection<ServiceDto>();
            WorkerList = new ObservableCollection<WorkerDto>(_requestService.GetWorkers(null));
            ContactList = new ObservableCollection<ContactDto>(new [] {new ContactDto {Id = 1,IsMain = true,PhoneNumber = AppSettings.LastIncomingCall}});
            CityList = new ObservableCollection<CityDto>(_requestService.GetCities());
            if (CityList.Count > 0)
            {
                SelectedCity = CityList.FirstOrDefault();
            }

            ParentServiceList = new ObservableCollection<ServiceDto>(_requestService.GetServices(null));
            if (ParentServiceList.Count > 0)
            {
                SelectedParentService = ParentServiceList.FirstOrDefault();
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
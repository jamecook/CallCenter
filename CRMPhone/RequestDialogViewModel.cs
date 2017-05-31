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
        private string _callUniqueId;
        
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
            var serviceCompanyId = _requestService.GetServiceCompany(houseId.Value);
            if (serviceCompanyId != null)
            {
                foreach (var request in RequestList.Where(r => r.CanSave))
                {
                    request.SelectedCompany = request.CompanyList.FirstOrDefault(c => c.Id == serviceCompanyId.Value);
                }
                
            }
            OnPropertyChanged(nameof(FlatList));
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
        public string Entrance
        {
            get { return _entrance; }
            set { _entrance = value; OnPropertyChanged(nameof(Entrance)); }
        }
        public string Floor
        {
            get { return _floor; }
            set { _floor = value; OnPropertyChanged(nameof(Floor)); }
        }

        public AddressTypeDto SelectedAddressType
        {
            get { return _selectedAddressType; }
            set { _selectedAddressType = value; OnPropertyChanged(nameof(SelectedAddressType));}
        }

        public ObservableCollection<AddressTypeDto> AddressTypeList
        {
            get { return _addressTypeList; }
            set { _addressTypeList = value; OnPropertyChanged(nameof(AddressTypeList));}
        }

        public ObservableCollection<ContactDto> ContactList
        {
            get { return _contactList; }
            set { _contactList = value; OnPropertyChanged(nameof(ContactList)); }
        }
        private ContactDto _selectedContact;
        private RequestItemViewModel _selectedRequest;

        private ICommand _addContactCommand;
        public ICommand AddContactCommand { get { return _addContactCommand ?? (_addContactCommand = new CommandHandler(AddContact, true)); } }
        private ICommand _addRequestCommand;
        public ICommand AddRequestCommand { get { return _addRequestCommand ?? (_addRequestCommand = new CommandHandler(AddRequest, true)); } }

        private void AddRequest()
        {
            RequestList.Add(new RequestItemViewModel());
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(Delete, true)); } }

        private ICommand _changeCheckedStateCommand;
        public ICommand ChangeCheckedStateCommand { get { return _changeCheckedStateCommand ?? (_changeCheckedStateCommand = new CommandHandler(ChangeChekedState, true)); } }


        private ICommand _saveRequestCommand;
        public ICommand SaveRequestCommand { get { return _saveRequestCommand ?? (_saveRequestCommand = new RelayCommand(SaveRequestRequest)); } }

        private ICommand _changeWorkerCommand;
        public ICommand ChangeWorkerCommand { get { return _changeWorkerCommand ?? (_changeWorkerCommand = new RelayCommand(ChangeWorker)); } }

        private ICommand _changeDateCommand;
        public ICommand ChangeDateCommand { get { return _changeDateCommand ?? (_changeDateCommand = new RelayCommand(ChangeDate)); } }


        private void ChangeWorker(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if(!requestModel.RequestId.HasValue)
                return;
            var model = new ChangeWorkerDialogViewModel(_requestService, requestModel.RequestId.Value);
            var view = new ChangeWorkerDialog();
            model.SetView(view);
            view.Owner = _view;
            view.DataContext = model;
            if (view.ShowDialog()==true)
            {
                requestModel.SelectedWorker = requestModel.WorkerList.SingleOrDefault(w => w.Id == model.ExecuterId);
            }
        }

        private void ChangeDate(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if (!requestModel.RequestId.HasValue)
                return;
            var model = new ChangeExecuteDateDialogViewModel(_requestService, requestModel.RequestId.Value);
            var view = new ChangeExecuteDateDialog();
            model.SetView(view);
            view.Owner = _view;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                requestModel.SelectedDateTime = model.SelectedDateTime;
                requestModel.SelectedPeriod = requestModel.PeriodList.SingleOrDefault(w => w.Id == model.SelectedPeriod.Id);
            }

        }

        private void SaveRequestRequest(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if (requestModel.SelectedService == null)
            {
                MessageBox.Show("Необходимо выбрать причину обращения!");
                return;
            }
            if (SelectedFlat == null)
            {
                MessageBox.Show("Необходимо выбрать верный адрес!");
                return;
            }
            var request = _requestService.SaveNewRequest(SelectedFlat.Id, requestModel.SelectedService.Id, ContactList.ToArray(), requestModel.Description, requestModel.IsChargeable, requestModel.IsImmediate,_callUniqueId,Entrance,Floor);
            if (!request.HasValue)
            {
                MessageBox.Show("Произошла непредвиденная ошибка!");
                return;
            }
            requestModel.RequestId = request;
            if (requestModel.SelectedWorker != null && requestModel.SelectedWorker.Id > 0)
                _requestService.AddNewWorker(request.Value, requestModel.SelectedWorker.Id);
            if (requestModel.SelectedDateTime.HasValue)
                _requestService.AddNewExecuteDate(request.Value, requestModel.SelectedDateTime.Value, requestModel.SelectedPeriod, "");
            //Обновление информации о заявке
            var newRequest = _requestService.GetRequest(request.Value);
            requestModel.RequestCreator = newRequest.CreateUser.ShortName;
            requestModel.RequestDate = newRequest.CreateTime;
            requestModel.RequestState = newRequest.State.Description;
            MessageBox.Show($"Номер заявки №{request}", "Заявка", MessageBoxButton.OK);

        }

        private ICommand _closeCommand;
        private AddressTypeDto _selectedAddressType;
        private ObservableCollection<AddressTypeDto> _addressTypeList;
        private ObservableCollection<RequestItemViewModel> _requestList;
        private string _entrance;
        private string _floor;

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

        public ContactDto SelectedContact
        {
            get { return _selectedContact; }
            set { _selectedContact = value; OnPropertyChanged(nameof(SelectedContact));}
        }

        public ObservableCollection<RequestItemViewModel> RequestList
        {
            get { return _requestList; }
            set { _requestList = value; OnPropertyChanged(nameof(RequestList));}
        }

        public RequestItemViewModel SelectedRequest
        {
            get { return _selectedRequest; }
            set { _selectedRequest = value; OnPropertyChanged(nameof(SelectedRequest)); }
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

            _callUniqueId = _requestService.GetActiveCallUniqueId();
            StreetList = new ObservableCollection<StreetDto>();
            HouseList = new ObservableCollection<HouseDto>();
            FlatList = new ObservableCollection<FlatDto>();
            AddressTypeList = new ObservableCollection<AddressTypeDto>(_requestService.GetAddressTypes());
            //if (AddressTypeList.Count > 0)
            //{
            //    SelectedAddressType = AddressTypeList.FirstOrDefault();
            //}
            ContactList = new ObservableCollection<ContactDto>(new [] {new ContactDto {Id = 1,IsMain = true,PhoneNumber = AppSettings.LastIncomingCall}});
            CityList = new ObservableCollection<CityDto>(_requestService.GetCities());
            if (CityList.Count > 0)
            {
                SelectedCity = CityList.FirstOrDefault();
            }
            RequestList = new ObservableCollection<RequestItemViewModel> {new RequestItemViewModel()};
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
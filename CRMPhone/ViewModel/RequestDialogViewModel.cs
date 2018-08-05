using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dialogs;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class RequestDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private readonly RequestServiceImpl.RequestService _requestService;
        private ObservableCollection<CityDto> _cityList;
        private CityDto _selectedCity;
        private ObservableCollection<StreetDto> _streetList;
        private StreetDto _selectedStreet;
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;
        private ObservableCollection<FlatDto> _flatList;
        private FlatDto _selectedFlat;
        private string _callUniqueId;
        private int? _selectedServiceCompanyId;
        private ObservableCollection<ContactDto> _contactList;
        private int _requestId;

        public ObservableCollection<CityDto> CityList
        {
            get { return _cityList; }
            set { _cityList = value; OnPropertyChanged(nameof(CityList));}
        }

        public ObservableCollection<RequestForListDto> AddressRequestList
        {
            get { return _addressRequestList; }
            set { _addressRequestList = value; OnPropertyChanged(nameof(AddressRequestList)); }
        }
        private ICommand _openRequestCommand;
        public ICommand OpenRequestCommand { get { return _openRequestCommand ?? (_openRequestCommand = new RelayCommand(OpenRequest)); } }


        private void OpenRequest(object sender)
        {
            var selectedItem = sender as RequestForListDto;
            if (selectedItem == null)
                return;
            var request = _requestService.GetRequest(selectedItem.Id);
            if (request == null)
            {
                MessageBox.Show("Произошла непредвиденная ошибка!");
                return;
            }

            var viewModel = new RequestDialogViewModel();
            var view = new RequestDialog(viewModel);
            viewModel.SetView(view);
            viewModel.RequestId = request.Id;
            viewModel.SelectedCity = viewModel.CityList.SingleOrDefault(i => i.Id == request.Address.CityId);
            viewModel.SelectedStreet = viewModel.StreetList.SingleOrDefault(i => i.Id == request.Address.StreetId);
            viewModel.SelectedHouse = viewModel.HouseList.SingleOrDefault(i => i.Id == request.Address.HouseId);
            viewModel.SelectedFlat = viewModel.FlatList.SingleOrDefault(i => i.Id == request.Address.Id);
            viewModel.Floor = request.Floor;
            viewModel.Entrance = request.Entrance;
            viewModel.FromTime = request.FromTime;
            viewModel.ToTime = request.ToTime;
            var requestModel = viewModel.RequestList.FirstOrDefault();
            requestModel.SelectedParentService = requestModel.ParentServiceList.SingleOrDefault(i => i.Id == request.Type.ParentId);
            requestModel.SelectedService = requestModel.ServiceList.SingleOrDefault(i => i.Id == request.Type.Id);
            requestModel.Description = request.Description;
            requestModel.IsChargeable = request.IsChargeable;
            requestModel.IsImmediate = request.IsImmediate;
            requestModel.IsBadWork = request.IsBadWork;
            requestModel.IsRetry = request.IsRetry;
            requestModel.Gatanty = request.Garanty;
            requestModel.RequestCreator = request.CreateUser.ShortName;
            requestModel.RequestDate = request.CreateTime;
            requestModel.RequestState = request.State.Description;
            requestModel.SelectedMaster = requestModel.MasterList.SingleOrDefault(w => w.Id == request.MasterId);
            requestModel.SelectedExecuter = requestModel.ExecuterList.SingleOrDefault(w => w.Id == request.ExecuterId);
            requestModel.SelectedEquipment = requestModel.EquipmentList.SingleOrDefault(e => e.Id == request.Equipment.Id);
            requestModel.RequestId = request.Id;
            requestModel.Rating = request.Rating;
            requestModel.AlertTime = request.AlertTime;
            if (request.ServiceCompanyId.HasValue)
            {
                requestModel.SelectedCompany = requestModel.CompanyList.FirstOrDefault(c => c.Id == request.ServiceCompanyId.Value);
            }
            if (request.ExecuteDate.HasValue && request.ExecuteDate.Value.Date > DateTime.MinValue)
            {
                requestModel.SelectedDateTime = request.ExecuteDate.Value.Date;
                requestModel.SelectedPeriod = requestModel.PeriodList.SingleOrDefault(i => i.Id == request.PeriodId);
            }
            requestModel.TermOfExecution = request.TermOfExecution;
            viewModel.ContactList = new ObservableCollection<ContactDto>(request.Contacts);
            view.Show();

        }

        public int RequestId
        {
            get { return _requestId; }
            set {
                _requestId = value;
                OnPropertyChanged(nameof(RequestId));
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(ReadOnly));
            }
        }

        public bool CanEdit { get { return RequestId == 0; } }
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

        private void ChangeHouse(int? houseId)
        {
            FlatList.Clear();
            if (!houseId.HasValue)
                return;
            if(CanEdit)
                AlertExists = _requestService.AlertCountByHouseId(houseId.Value)>0;
            foreach (var flat in _requestService.GetFlats(houseId.Value).OrderBy(s => s.TypeId).ThenBy(s => s.Flat?.PadLeft(6,'0')))
            {
                FlatList.Add(flat);
            }
            var serviceCompanyId = _requestService.GetServiceCompany(houseId.Value);
            _selectedServiceCompanyId = serviceCompanyId;

            foreach (var request in RequestList.Where(r => r.CanSave))
            {
                request.SelectedHouseId = houseId;
                if (serviceCompanyId != null)
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

        public bool AlertExists
        {
            get { return _alertExists; }
            set { _alertExists = value; OnPropertyChanged(nameof(AlertExists)); }
        }

        public ObservableCollection<FlatDto> FlatList
        {
            get { return _flatList; }
            set { _flatList = value; OnPropertyChanged(nameof(FlatList));}
        }

        public FlatDto SelectedFlat
        {
            get { return _selectedFlat; }
            set { _selectedFlat = value;
                if (_selectedFlat != null)
                {
                    LoadRequestsBySelectedAddress(_selectedFlat.Id);
                    if (string.IsNullOrEmpty(_callUniqueId))
                    {
                        _callUniqueId = _requestService.GetActiveCallUniqueId();
                    }
                }
                else
                {
                    AddressRequestList.Clear();
                }
                OnPropertyChanged(nameof(SelectedFlat));}
        }

        private void LoadRequestsBySelectedAddress(int addressId)
        {
            var currentDate = _requestService.GetCurrentDate();
            AddressRequestList = new ObservableCollection<RequestForListDto>(_requestService.GetRequestList(null, true, currentDate.AddDays(-90), currentDate.AddDays(1), DateTime.Today,
                DateTime.Today, null, null, addressId, null, null,null,null,null,null,null,null,null,false,false,null,false));
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

        private ICommand _getScInfoCommand;
        public ICommand GetScInfoCommand { get { return _getScInfoCommand ?? (_getScInfoCommand = new RelayCommand(GetScInfo)); } }

        private void GetScInfo(object obj)
        {
            var view = new GetScInfoDialog();
            var model = new GetScInfoDialogViewModel(_requestService,_selectedServiceCompanyId,view);
            view.DataContext = model;
            model.SetView(view);
            view.Owner = _view;
            view.Show();
        }

        private void AddRequest()
        {
            RequestList.Add(new RequestItemViewModel());
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(Delete, true)); } }

        private ICommand _changeCheckedStateCommand;
        public ICommand ChangeCheckedStateCommand { get { return _changeCheckedStateCommand ?? (_changeCheckedStateCommand = new CommandHandler(ChangeChekedState, true)); } }

        private ICommand _openAlertsCommand;
        private ICommand _editAddressCommand;
        public ICommand OpenAlertsCommand { get { return _openAlertsCommand ?? (_openAlertsCommand = new CommandHandler(OpenAlerts, true)); } }

        public ICommand EditAddressCommand { get { return _editAddressCommand ?? (_editAddressCommand = new CommandHandler(EditAddress, true)); } }

        private void EditAddress()
        {
            if (RequestId == 0)
                return;
            var model = new EditAddressOnRequestDialogViewModel(RequestId);
            var view = new EditAddressOnRequestDialog();
            view.DataContext = model;
            view.Owner = Application.Current.MainWindow;
            model.SetView(view);
            if (view.ShowDialog() != true)
                return;
            SelectedCity = CityList.FirstOrDefault(c => c.Id == model.SelectedCity.Id);
            SelectedStreet = StreetList.FirstOrDefault(s=>s.Id == model.SelectedStreet.Id);
            SelectedHouse = HouseList.FirstOrDefault(h=>h.Id == model.SelectedHouse.Id);
            SelectedFlat = FlatList.FirstOrDefault(f=>f.Id == model.SelectedFlat.Id);
            _requestService.RequestChangeAddress(RequestId,model.SelectedFlat.Id);
        }

        private void OpenAlerts()
        {
            var alerts = _requestService.GetAlerts(DateTime.Now, DateTime.Now, SelectedHouse.Id);
            var model = new AlertForHouseDialogViewModel(alerts);
            var view = new AlertByHouseListDialog();
            view.DataContext = model;
            view.Owner = Application.Current.MainWindow;
            model.SetView(view);
            view.ShowDialog();
        }

        private ICommand _saveRequestCommand;
        public ICommand SaveRequestCommand { get { return _saveRequestCommand ?? (_saveRequestCommand = new RelayCommand(SaveRequest)); } }

        private ICommand _changeMasterCommand;
        public ICommand ChangeMasterCommand { get { return _changeMasterCommand ?? (_changeMasterCommand = new RelayCommand(ChangeMaster)); } }
        private ICommand _changeExecuterCommand;
        public ICommand ChangeExecuterCommand { get { return _changeExecuterCommand ?? (_changeExecuterCommand = new RelayCommand(ChangeExecuter)); } }

        private ICommand _setWorkingTimesCommand;
        public ICommand SetWorkingTimesCommand { get { return _setWorkingTimesCommand ?? (_setWorkingTimesCommand = new RelayCommand(SetWorkingTimes)); } }

        private ICommand _openAttachmentDialogCommand;
        public ICommand OpenAttachmentDialogCommand { get { return _openAttachmentDialogCommand ?? (_openAttachmentDialogCommand = new RelayCommand(OpenAttachmentDialog)); } }

        private ICommand _noteCommand;
        public ICommand OpenNoteDialogCommand { get { return _noteCommand ?? (_noteCommand = new RelayCommand(OpenNotesDialog)); } }
        private ICommand _addCallCommand;
        public ICommand AddCallCommand { get { return _addCallCommand ?? (_addCallCommand = new RelayCommand(AddCall)); } }

        private ICommand _showMasterInfoCommand;
        public ICommand ShowMasterInfoCommand
        {
            get { return _showMasterInfoCommand ?? (_showMasterInfoCommand = new RelayCommand(ShowMasterInfo)); }
            
        }

        private void ShowMasterInfo(object obj)
        {
            var model = obj as RequestItemViewModel;
            if(model?.SelectedMaster == null)
                return;
            var view = new WorkerInfoDialog();
            view.Owner = _view;
            var viewModel = new WorkerInfoViewModel(_requestService,model?.SelectedMaster?.Id??0,_requestId);
            view.DataContext = viewModel;
            viewModel.SetView(view);
            view.ShowDialog();
            //throw new NotImplementedException();
        }

        private ICommand _callsHistoryCommand;
        public ICommand CallsHistoryCommand { get { return _callsHistoryCommand ?? (_callsHistoryCommand = new RelayCommand(CallsHistory)); } }

        private void CallsHistory(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if (!requestModel.RequestId.HasValue)
                return;
            var model = new CallsHistoryDialogViewModel(_requestService, requestModel.RequestId.Value);
            var view = new CallsHistoryDialog();
            model.SetView(view);
            view.Owner = _view;
            view.DataContext = model;
            view.ShowDialog();
        }

        private void AddCall(object obj)
        {
            var callUniqueId = _requestService.GetActiveCallUniqueId();
            _requestService.AddCallToRequest(RequestId,callUniqueId);
            if(!string.IsNullOrEmpty(callUniqueId))
                MessageBox.Show("Текущий разговор прикреплен к заявке!");
        }

        private ICommand _changeDateCommand;
        public ICommand ChangeDateCommand { get { return _changeDateCommand ?? (_changeDateCommand = new RelayCommand(ChangeDate)); } }

        private ICommand _changeStatusCommand;
        public ICommand ChangeStatusCommand { get { return _changeStatusCommand ?? (_changeStatusCommand = new RelayCommand(ChangeStatus)); } }

        private ICommand _changeNoteCommand;
        public ICommand ChangeNoteCommand { get { return _changeNoteCommand ?? (_changeNoteCommand = new RelayCommand(ChangeNote)); } }
        private ICommand _ratingCommand;
        public ICommand RatingCommand { get { return _ratingCommand ?? (_ratingCommand = new RelayCommand(AddRating)); } }
        private ICommand _saveDescCommand;
        public ICommand SaveDescCommand { get { return _saveDescCommand ?? (_saveDescCommand = new RelayCommand(SaveDesc)); } }
        private ICommand _changeAlertTimeCommand;
        public ICommand ChangeAlertTimeCommand { get { return _changeAlertTimeCommand ?? (_changeAlertTimeCommand = new RelayCommand(ChangeAlertTime)); } }

        private void ChangeAlertTime(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            var times = _requestService.GetAlertTimes(requestModel.IsImmediate);
            var model = new ChangeAlertTimeDialogViewModel(times);
            var view = new ChangeAlertTimeDialog();
            model.SetView(view);
            view.Owner = _view;
            view.DataContext = model;
            if(view.ShowDialog() ?? false)
            {
                var currentTime = model.ByTime?_requestService.GetCurrentDate().AddMinutes(model.SelectedTime.AddMinutes)
                        :(model.SelectedDate?? _requestService.GetCurrentDate()).AddMinutes(model.SelectedDateTime.AddMinutes);
                requestModel.AlertTime = currentTime;
            }

        }

        private void SaveDesc(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if (!requestModel.RequestId.HasValue)
                return;
            _requestService.ChangeDescription(requestModel.RequestId.Value, requestModel.Description);
            MessageBox.Show("Примечание сохранено!");
        }
        private void OpenAttachmentDialog(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if (!requestModel.RequestId.HasValue)
                return;
            var model = new AttachmentDialogViewModel(_requestService, requestModel.RequestId.Value);
            var view = new AttachmentDialog();
            model.SetView(view);
            view.Owner = _view;
            view.DataContext = model;
            view.ShowDialog();
        }
        private void OpenNotesDialog(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if (!requestModel.RequestId.HasValue)
                return;
            var model = new NoteDialogViewModel(_requestService, requestModel.RequestId.Value);
            var view = new NotesDialog();
            model.SetView(view);
            view.Owner = _view;
            view.DataContext = model;
            view.ShowDialog();
        }

        private void ChangeMaster(object sender)
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
                if (requestModel.MasterList.Where(m=>m!=null).All(m => m.Id != model.MasterId))
                {
                    requestModel.MasterList.Add(model.SelectedWorker);
                }
                requestModel.SelectedMaster = requestModel.MasterList.SingleOrDefault(w => w.Id == model.MasterId);
            }
        }
        private void ChangeExecuter(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if(!requestModel.RequestId.HasValue)
                return;
            var model = new ChangeExecuterDialogViewModel(_requestService, requestModel.RequestId.Value);
            var view = new ChangeWorkerDialog();
            model.SetView(view);
            view.Owner = _view;
            view.DataContext = model;
            if (view.ShowDialog()==true)
            {
                requestModel.SelectedExecuter = requestModel.ExecuterList.SingleOrDefault(w => w.Id == model.MasterId);
            }
        }
        private void SetWorkingTimes(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if(!requestModel.RequestId.HasValue)
                return;
            var model = new SetWorkingTimesDialogViewModel(FromTime,ToTime);
            var view = new SetWorkingTimesDialog();
            model.SetView(view);
            view.Owner = _view;
            view.DataContext = model;
            if (view.ShowDialog()==true)
            {
                var fromTime = DateTime.ParseExact($"01.01.0001 {model.FromHour}:{model.FromMinute}", "dd.MM.yyyy HH:mm", null);
                var toTime = DateTime.ParseExact($"01.01.0001 {model.ToHour}:{model.ToMinute}", "dd.MM.yyyy HH:mm", null);
                if (toTime < fromTime)
                    toTime = toTime.AddDays(1);
                _requestService.SetRequestWorkingTimes(requestModel.RequestId.Value,fromTime,toTime,AppSettings.CurrentUser.Id);
            }
        }
        private void ChangeStatus(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if (!requestModel.RequestId.HasValue)
                return;
            var model = new ChangeStatusDialogViewModel(_requestService, requestModel.RequestId.Value);
            var view = new ChangeStatusDialog();
            model.SetView(view);
            view.Owner = _view;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                requestModel.RequestState = model.SelectedStatus.Description;
            }
        }

        private void AddRating(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            if (!requestModel.RequestId.HasValue)
                return;
            var model = new AddRatingDialogViewModel(_requestService, requestModel.RequestId.Value);
            var view = new AddRatingDialog();
            model.SetView(view);
            view.Owner = _view;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                model.SelectedRating.Description = model.Description;
                requestModel.Rating = model.SelectedRating;
            }
        }
        private void ChangeNote(object sender)
        {
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

        private void SaveRequest(object sender)
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
            if (requestModel.RequestId.HasValue)
            {
                _requestService.EditRequest(requestModel.RequestId.Value, requestModel.SelectedService.Id,
                    requestModel.Description, requestModel.IsImmediate, requestModel.IsChargeable,requestModel.IsBadWork,requestModel.Gatanty, requestModel.IsRetry, requestModel.AlertTime, requestModel.TermOfExecution);
                MessageBox.Show($"Данные успешно сохранены!", "Заявка", MessageBoxButton.OK);
                return;
            }

            if (string.IsNullOrEmpty(_callUniqueId))
            {
                _callUniqueId = _requestService.GetActiveCallUniqueId();
            }
            var request = _requestService.SaveNewRequest(SelectedFlat.Id, requestModel.SelectedService.Id, ContactList.ToArray(), requestModel.Description, requestModel.IsChargeable, requestModel.IsImmediate, _callUniqueId, Entrance, Floor, requestModel.SelectedCompany.Id, requestModel.AlertTime,requestModel.IsRetry,requestModel.IsBadWork, requestModel.SelectedEquipment?.Id);
            if (!request.HasValue)
            {
                MessageBox.Show("Произошла непредвиденная ошибка!");
                return;
            }
            var smsSettings = _requestService.GetSmsSettingsForServiceCompany(requestModel.SelectedCompany.Id);
            if (smsSettings.SendToClient && ContactList.Any(c => c.IsMain) && requestModel.SelectedParentService.CanSendSms && requestModel.SelectedService.CanSendSms)
            {
                var mainClient = ContactList.FirstOrDefault(c => c.IsMain);
                _requestService.SendSms(request.Value, smsSettings.Sender,
                    mainClient.PhoneNumber, $"Заявка № {request.Value}. {requestModel.SelectedParentService.Name} - {requestModel.SelectedService.Name}", true);
            }
            requestModel.RequestId = request;
            if (requestModel.SelectedMaster != null && requestModel.SelectedMaster.Id > 0)
                _requestService.AddNewMaster(request.Value, requestModel.SelectedMaster.Id);
            if (requestModel.SelectedExecuter!= null && requestModel.SelectedExecuter.Id > 0)
                _requestService.AddNewExecuter(request.Value, requestModel.SelectedExecuter.Id);
            if (requestModel.SelectedDateTime.HasValue)
                _requestService.AddNewExecuteDate(request.Value, requestModel.SelectedDateTime.Value, requestModel.SelectedPeriod, "");
            if (requestModel.TermOfExecution.HasValue)
                _requestService.AddNewTermOfExecution(request.Value, requestModel.SelectedDateTime.Value, "");
            //Обновление информации о заявке
            var newRequest = _requestService.GetRequest(request.Value);
            requestModel.RequestCreator = newRequest.CreateUser.ShortName;
            requestModel.RequestDate = newRequest.CreateTime;
            requestModel.RequestState = newRequest.State.Description;
            requestModel.Rating = newRequest.Rating;
            MessageBox.Show($"Номер заявки №{request}", "Заявка", MessageBoxButton.OK);

        }

        private ICommand _closeCommand;
        private AddressTypeDto _selectedAddressType;
        private ObservableCollection<AddressTypeDto> _addressTypeList;
        private ObservableCollection<RequestItemViewModel> _requestList;
        private string _entrance;
        private string _floor;
        private ObservableCollection<RequestForListDto> _addressRequestList;
        private bool _alertExists;
        private bool _canEditAddress;

        public bool CanEditAddress
        {
            get { return _canEditAddress; }
            set
            {
                _canEditAddress = value;
                OnPropertyChanged(nameof(CanEditAddress));
            }
        }

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
            _view.Close();
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
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }


        public RequestDialogViewModel()
        {
            AlertExists = false;
            _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);
            var contactInfo = new ContactDto {Id = 1, IsMain = true, PhoneNumber = AppSettings.LastIncomingCall};
            _callUniqueId = _requestService.GetActiveCallUniqueId();
            StreetList = new ObservableCollection<StreetDto>();
            HouseList = new ObservableCollection<HouseDto>();
            FlatList = new ObservableCollection<FlatDto>();
            AddressTypeList = new ObservableCollection<AddressTypeDto>(_requestService.GetAddressTypes());
            //if (AddressTypeList.Count > 0)
            //{
            //    SelectedAddressType = AddressTypeList.FirstOrDefault();
            //}
            CityList = new ObservableCollection<CityDto>(_requestService.GetCities());
            if (CityList.Count > 0)
            {
                SelectedCity = CityList.FirstOrDefault();
            }
            RequestList = new ObservableCollection<RequestItemViewModel> { new RequestItemViewModel() };
            //AppSettings.LastIncomingCall = "932";
            CanEditAddress = AppSettings.CurrentUser.Roles.Select(r => r.Name).Contains("admin");
            if (!string.IsNullOrEmpty(AppSettings.LastIncomingCall))
            {
                var clientInfoDto = _requestService.GetLastAddressByClientPhone(AppSettings.LastIncomingCall);
                if (clientInfoDto != null)
                {
                    SelectedStreet = StreetList.FirstOrDefault(s => s.Id == clientInfoDto.StreetId);
                    SelectedHouse = HouseList.FirstOrDefault(h => h.Building == clientInfoDto.Building &&
                                                      h.Corpus == clientInfoDto.Corpus);
                    SelectedFlat = FlatList.FirstOrDefault(f => f.Flat == clientInfoDto.Flat);
                    contactInfo = new ContactDto { Id = 1, IsMain = true, PhoneNumber = AppSettings.LastIncomingCall,Name = clientInfoDto.Name};
                }
            }
            ContactList = new ObservableCollection<ContactDto>(new[] {contactInfo});

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
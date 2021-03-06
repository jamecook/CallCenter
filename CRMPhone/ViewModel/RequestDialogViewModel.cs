﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dialogs;
using Newtonsoft.Json;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using RudiGrobler.Calendar.Common;

namespace CRMPhone.ViewModel
{
    public class RequestDialogViewModel : INotifyPropertyChanged
    {
        private RequestDialog _view;
        private double _defaultWidth = 1020;

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
        private List<ContactDto> _requestContacts;

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

            var viewModel = new RequestDialogViewModel(request);
            var view = new RequestDialog(viewModel);
            viewModel.SetView(view);
            viewModel.RequestId = request.Id;
            viewModel.SelectedCity = viewModel.CityList.SingleOrDefault(i => i.Id == request.Address.CityId);
            viewModel.SelectedStreet = viewModel.StreetList.SingleOrDefault(i => i.Id == request.Address.StreetId);
            viewModel.StreetName = request.Address.StreetName;
            viewModel.SelectedHouse = viewModel.HouseList.SingleOrDefault(i => i.Id == request.Address.HouseId);
            if (viewModel.FlatList.All(i => i.Id != request.Address.Id))
            {
                viewModel.FlatList.Add(new FlatDto()
                {
                    Id = request.Address.Id,
                    Flat = request.Address.Flat,
                    TypeId = request.Address.TypeId,
                    TypeName = request.Address.Type
                });
            }
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
            var sched = _requestService.GetScheduleTaskByRequestId(request.Id);
            requestModel.SelectedAppointment = sched != null ? new Appointment()
            {
                Id = sched.Id,
                RequestId = sched.RequestId,
                StartTime = sched.FromDate,
                EndTime = sched.ToDate,
            } : null;
            requestModel.OpenAppointment = requestModel.SelectedAppointment;
            //requestModel.Gatanty = request.Warranty;
            requestModel.SelectedGaranty = requestModel.GarantyList.FirstOrDefault(g => g.Id == request.GarantyId);

            requestModel.RequestCreator = request.CreateUser.ShortName;
            requestModel.RequestDate = request.CreateTime;
            requestModel.RequestState = request.State.Description;
            requestModel.SelectedMaster = request.MasterId.HasValue ? _requestService.GetWorkerById(request.MasterId.Value) : null;
            requestModel.SelectedExecuter = request.ExecuterId.HasValue ? _requestService.GetWorkerById(request.ExecuterId.Value) : null;

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
                OnPropertyChanged(nameof(CanEditPhone));
                OnPropertyChanged(nameof(ReadOnlyPhone));
                OnPropertyChanged(nameof(ReadOnly));
            }
        }

        public bool ExistForAllInfo { get; set; }
        public bool ExistForServiceInfo { get; set; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value;
                if (value)
                {
                    AdditionInfoVisibility = Visibility.Visible;
                    WindowWidth = 1400;
                }
                else
                {
                    WindowWidth = _defaultWidth;
                    AdditionInfoVisibility = Visibility.Collapsed;

                }
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public Visibility AdditionInfoVisibility
        {
            get => _additionInfoVisibility;
            set { _additionInfoVisibility = value; OnPropertyChanged(nameof(AdditionInfoVisibility));}
        }


        public double WindowWidth
        {
            get => _windowWidth;
            set { _windowWidth = value; OnPropertyChanged(nameof(WindowWidth));}
        }

        public bool CanEdit => RequestId == 0;
        public bool CanEditPhone { get { return RequestId == 0 || (AppSettings.CurrentUser != null && AppSettings.CurrentUser.Roles.Exists(r => r.Name == "admin" || r.Name == "supervizor")); } }
        public bool ReadOnly => !CanEdit;
        public bool ReadOnlyPhone => !CanEditPhone;

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
            if (houseId.HasValue)
            {
                var house = _requestService.GetHouseById(houseId.Value);
                CommissioningDate = house.CommissioningDate;
                ElevatorCount = house.ElevatorCount;
                ServiceCompany = house.ServiceCompanyName;
                CityRegion = house.RegionName;
            }
            else
            {
                CommissioningDate = null;
                ElevatorCount = null;
            }

            OnPropertyChanged(nameof(FlatList));
        }

        public ObservableCollection<StreetDto> StreetList
        {
            get { return _streetList;}
            set { _streetList = value; OnPropertyChanged(nameof(StreetList));}
        }

        public string StreetName
        {
            get { return _streetName; }
            set { _streetName = value; OnPropertyChanged(nameof(StreetName)); }
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

        public DateTime? CommissioningDate
        {
            get { return _commissioningDate; }
            set { _commissioningDate = value; OnPropertyChanged(nameof(CommissioningDate)); }
        }

        public string ServiceCompany
        {
            get { return _serviceCompany; }
            set { _serviceCompany = value; OnPropertyChanged(nameof(ServiceCompany)); }
        }

        public string CityRegion
        {
            get { return _cityRegion; }
            set { _cityRegion = value; OnPropertyChanged(nameof(CityRegion));}
        }

        public int? ElevatorCount
        {
            get { return _elevatorCount; }
            set { _elevatorCount = value; OnPropertyChanged(nameof(ElevatorCount));}
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
                        _callUniqueId = _requestService.GetActiveCallUniqueIdByCallId(AppSettings.LastCallId);
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
            AddressRequestList = new ObservableCollection<RequestForListDto>(_requestService.GetRequestList(null, true, currentDate.AddDays(-365), currentDate.AddDays(1), DateTime.Today,
                DateTime.Today, null, null, addressId, null, null,null,null,null,null,null,null,null,false,false,null,false,false, false, false));
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
            set
            {
                _contactList = value; OnPropertyChanged(nameof(ContactList));
                _requestContacts = new List<ContactDto>();
                foreach (var contactDto in ContactList)
                {
                    _requestContacts.Add(contactDto.Copy());
                }
            }
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
            var newRequest = new RequestItemViewModel(this);
            newRequest.SelectedHouseId = SelectedHouse.Id;
            RequestList.Add(newRequest);
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
        private ICommand _showExecuterInfoCommand;
        public ICommand ShowExecuterInfoCommand
        {
            get { return _showExecuterInfoCommand ?? (_showExecuterInfoCommand = new RelayCommand(ShowExecuterInfo)); }
            
        }


        private ICommand _dialCommand;
        public ICommand DialCommand
        {
            get { return _dialCommand ?? (_dialCommand = new RelayCommand(Dial)); }

        }

        private void Dial(object obj)
        {
            var viewModel = obj as RequestItemViewModel;
            if ((viewModel is null) || string.IsNullOrEmpty(viewModel.PhoneNumber))
                return;

            ContextSaver.CrmContext.SipPhone = viewModel.PhoneNumber;
            ContextSaver.CrmContext.Call();
            if (viewModel.RequestId.HasValue)
            {
                Thread.Sleep(500);
                if (string.IsNullOrEmpty(AppSettings.LastCallId))
                {
                    var sipState = JsonConvert.SerializeObject(ContextSaver.CrmContext.SipLines);
                    _requestService.AddCallHistory(viewModel.RequestId.Value, "-------",
                        AppSettings.CurrentUser.Id, sipState, "RequestDialogDial");
                    return;
                }
                var callUniqueId = _requestService.GetActiveCallUniqueIdByCallId(AppSettings.LastCallId);

                if (!string.IsNullOrEmpty(callUniqueId))
                {
                    _requestService.AddCallToRequest(viewModel.RequestId.Value, callUniqueId);
                    _requestService.AddCallHistory(viewModel.RequestId.Value, callUniqueId,
                        AppSettings.CurrentUser.Id, AppSettings.LastCallId, "RequestDialogDial");
                }
            }
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
        private void ShowExecuterInfo(object obj)
        {
            var model = obj as RequestItemViewModel;
            if(model?.SelectedExecuter == null)
                return;
            var view = new WorkerInfoDialog();
            view.Owner = _view;
            var viewModel = new WorkerInfoViewModel(_requestService,model?.SelectedExecuter?.Id??0,_requestId);
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
            //todo сделать логирование нажатий
            //var appSetting = JsonConvert.SerializeObject(AppSettings.CurrentUser) + JsonConvert.SerializeObject(AppSettings.SipInfo) + JsonConvert.SerializeObject(AppSettings.LastIncomingCall) + JsonConvert.SerializeObject(AppSettings.LastCallId);

            var lastCallId = AppSettings.LastCallId;
            if (string.IsNullOrEmpty(lastCallId))
            {
                MessageBox.Show("ОШИБКА прикрепление звонка! Пустой номер последнего звонка!");
                return;
            }
            if (!(obj is RequestItemViewModel))
                return;
            var requestModel = obj as RequestItemViewModel;
            if (!requestModel.RequestId.HasValue)
                return;

            var callUniqueId = _requestService.GetActiveCallUniqueIdByCallId(lastCallId);
            _requestService.AddCallToRequest(requestModel.RequestId.Value, callUniqueId);
            _requestService.AddCallHistory(requestModel.RequestId.Value, callUniqueId, AppSettings.CurrentUser.Id, AppSettings.LastCallId,"RequestDialogAddCall");
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
        private ICommand _openCalendarCommand;
        public ICommand OpenCalendarCommand { get { return _openCalendarCommand ?? (_openCalendarCommand = new RelayCommand(OpenCalendar)); } }

        

        private void OpenCalendar(object sender)
        {
            if (!(sender is RequestItemViewModel))
                return;
            var requestModel = sender as RequestItemViewModel;
            var model = new CalendarDialogViewModel(_requestService,requestModel.RequestId);
            if (requestModel.SelectedExecuter == null)
            {
                MessageBox.Show(_view, "Необходимо выбрать исполнителя!");
                return;
            }
            var sched = _requestService.GetScheduleTasks(requestModel.SelectedExecuter.Id, DateTime.Now.Date.AddDays(-7),
                DateTime.Now.Date.AddDays(14));
            var app = sched.Select(s => new Appointment()
            {
                Id = s.Id,
                RequestId = s.RequestId,
                Subject = string.Format($"{0}", s.RequestId),
                StartTime = s.FromDate,
                EndTime = s.ToDate,
                WorkerInfo = s.Worker.FullName
            });
            model.ScheduleTaskList = new ObservableCollection<Appointment>(app);
            var view = new CalendarDialog(model);
            view.DataContext = model;
            model.SetView(view);
            view.ShowDialog();
            var t = model;
            requestModel.SelectedAppointment = model.ScheduleTaskList.LastOrDefault(s => s.Id == 0);

        }

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
                var currentTime = model.ClearAlert ? (DateTime?)null : 
                            model.ByTime?_requestService.GetCurrentDate().AddMinutes(model.SelectedTime.AddMinutes) :
                            (model.SelectedDate ?? _requestService.GetCurrentDate()).Date.AddMinutes(model.SelectedDateTime.AddMinutes);
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
            model.WorkerTitle = "Мастер:";
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
                requestModel.SelectedMaster = model.SelectedWorker;
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
            model.WorkerTitle = "Исполнитель:";
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
                if (_requestContacts != null && AppSettings.CurrentUser.Roles.Exists(r => r.Name == "admin" || r.Name == "supervizor"))
                {
                    var forDeleteItems = _requestContacts?.Where(c => ContactList.All(l => l.Id != c.Id)).ToArray();
                    _requestService.DeleteContacts(requestModel.RequestId.Value, forDeleteItems);

                    var newItems = ContactList.Where(c => _requestContacts.All(l => l.Id != c.Id)).Where(c=>!string.IsNullOrEmpty(c.PhoneNumber)).ToArray();
                    _requestService.SaveContacts(requestModel.RequestId.Value,newItems);

                    var changedItems = (from c in ContactList
                        from r in _requestContacts
                        where c.PhoneNumber == r.PhoneNumber && (c.Name != r.Name || c.IsMain != r.IsMain || c.AdditionInfo != r.AdditionInfo || c.Email != r.Email )
                        select c).ToArray();
                    _requestService.EditContacts(requestModel.RequestId.Value, changedItems);
                    var updatedRequest = _requestService.GetRequest(RequestId);
                    if(updatedRequest != null)
                    {
                        ContactList = new ObservableCollection<ContactDto>(updatedRequest.Contacts);
                    }
                }

                _requestService.EditRequest(requestModel.RequestId.Value, requestModel.SelectedService.Id,
                    requestModel.Description, requestModel.IsImmediate, requestModel.IsChargeable,requestModel.IsBadWork,requestModel.SelectedGaranty?.Id??0, requestModel.IsRetry, requestModel.AlertTime, requestModel.TermOfExecution);
                //Делаем назначение в расписании
                if (requestModel.SelectedExecuter != null && requestModel.SelectedAppointment != null && requestModel.SelectedAppointment.RequestId == null)
                {
                    if (requestModel.OpenAppointment != null && requestModel.OpenAppointment.RequestId != null)
                    {
                        _requestService.DeleteScheduleTask(requestModel.OpenAppointment.Id);
                    }
                    _requestService.AddScheduleTask(requestModel.SelectedExecuter.Id, requestModel.RequestId.Value,
                        requestModel.SelectedAppointment.StartTime, requestModel.SelectedAppointment.EndTime, null);
                }
                MessageBox.Show($"Данные успешно сохранены!", "Заявка", MessageBoxButton.OK);
                return;
            }
            //if (string.IsNullOrEmpty(_callUniqueId))
            {
                _callUniqueId = _requestService.GetOnlyActiveCallUniqueIdByCallId(AppSettings.LastCallId);
            }
            //var request = _requestService.SaveNewRequest(SelectedFlat.Id, requestModel.SelectedService.Id, ContactList.ToArray(), requestModel.Description, requestModel.IsChargeable, requestModel.IsImmediate, null, Entrance, Floor, requestModel.AlertTime,requestModel.IsRetry,requestModel.IsBadWork, requestModel.SelectedEquipment?.Id);
            var request = _requestService.SaveNewRequest(SelectedFlat.Id, requestModel.SelectedService.Id, ContactList.ToArray(), requestModel.Description, requestModel.IsChargeable, requestModel.IsImmediate, _callUniqueId, Entrance, Floor, requestModel.AlertTime,requestModel.IsRetry,requestModel.IsBadWork, requestModel.SelectedEquipment?.Id, requestModel.SelectedGaranty?.Id ?? 0);
            if (!request.HasValue)
            {
                MessageBox.Show("Произошла непредвиденная ошибка!");
                return;
            }
            //Надо УК брать из сохраненной заявки
            /*
             var requestDto = _requestService.GetRequest(request.Value);

            var smsSettings = _requestService.GetSmsSettingsForServiceCompany(requestDto.ServiceCompanyId);
            if (smsSettings.SendToClient && ContactList.Any(c => c.IsMain) && requestModel.SelectedParentService.CanSendSms && requestModel.SelectedService.CanSendSms)
            {
                var mainClient = ContactList.FirstOrDefault(c => c.IsMain);
                _requestService.SendSms(request.Value, smsSettings.Sender,
                    mainClient.PhoneNumber, $"Заявка № {request.Value}. {requestModel.SelectedParentService.Name} - {requestModel.SelectedService.Name}", true);
            }
            */
            _requestService.SendSmsToClient(request.Value);

            requestModel.RequestId = request;
            if (requestModel.SelectedMaster != null && requestModel.SelectedMaster.Id > 0)
                _requestService.AddNewMaster(request.Value, requestModel.SelectedMaster.Id);
            if (requestModel.SelectedExecuter!= null && requestModel.SelectedExecuter.Id > 0)
                _requestService.AddNewExecutor(request.Value, requestModel.SelectedExecuter.Id);
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

            //Делаем назначение в расписании
            if (requestModel.SelectedExecuter != null && requestModel.SelectedAppointment != null)
            {
                _requestService.AddScheduleTask(requestModel.SelectedExecuter.Id, request.Value,
                    requestModel.SelectedAppointment.StartTime, requestModel.SelectedAppointment.EndTime, null);
            }

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
        private DateTime? _commissioningDate;
        private int? _elevatorCount;
        private string _serviceCompany;
        private string _cityRegion;
        private string _streetName;
        private bool _isExpanded;
        private double _windowWidth;
        private Visibility _additionInfoVisibility;

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
            set {
                _selectedRequest = value;
                GetInfoByHouseAndType(value.SelectedHouseId, value.SelectedService?.Id);
                OnPropertyChanged(nameof(SelectedRequest));
            }
        }

        private void Delete()
        {
            ContactList.Remove(SelectedContact);
        }


        private void AddContact()
        {
            ContactList.Add(new ContactDto());
        }


        public void SetView(RequestDialog view)
        {
            _view = view;
            LoadInfoForAll();
        }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }

        public RequestDialogViewModel(RequestInfoDto request)
        {
            WindowWidth = _defaultWidth;
            AlertExists = false;
            AdditionInfoVisibility = Visibility.Collapsed;
            _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);
            var contactInfo = new ContactDto {Id = 0, IsMain = true, PhoneNumber = AppSettings.LastIncomingCall};
            _callUniqueId = _requestService.GetActiveCallUniqueIdByCallId(AppSettings.LastCallId);
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

            var model = new RequestItemViewModel(this);
            model.SetRequest(request);
            RequestList = new ObservableCollection<RequestItemViewModel> { model };
            //AppSettings.LastIncomingCall = "932";
            CanEditAddress = true;
            //CanEditAddress = AppSettings.CurrentUser.Roles.Select(r => r.Name).Contains("admin");
            if (!string.IsNullOrEmpty(AppSettings.LastIncomingCall))
            {
                var clientInfoDto = _requestService.GetLastAddressByClientPhone(AppSettings.LastIncomingCall);
                if (clientInfoDto != null)
                {
                    SelectedStreet = StreetList.FirstOrDefault(s => s.Id == clientInfoDto.StreetId);
                    SelectedHouse = HouseList.FirstOrDefault(h => h.Building == clientInfoDto.Building &&
                                                      h.Corpus == clientInfoDto.Corpus);
                    SelectedFlat = FlatList.FirstOrDefault(f => f.Flat == clientInfoDto.Flat);
                    contactInfo = new ContactDto { Id = 0, IsMain = true, PhoneNumber = AppSettings.LastIncomingCall,Name = clientInfoDto.Name};
                }
            }
            ContactList = new ObservableCollection<ContactDto>(new[] {contactInfo});
            if (request?.Id > 0)
            {
                _selectedCity = CityList.SingleOrDefault(i => i.Id == request.Address.CityId);
                SelectedStreet = StreetList.SingleOrDefault(i => i.Id == request.Address.StreetId);
                _streetName = request.Address.StreetName;
                _selectedHouse = HouseList.SingleOrDefault(i => i.Id == request.Address.HouseId);
                if (_flatList.All(i => i.Id != request.Address.Id))
                {
                    _flatList.Add(new FlatDto()
                    {
                        Id = request.Address.Id,
                        Flat = request.Address.Flat,
                        TypeId = request.Address.TypeId,
                        TypeName = request.Address.Type
                    });
                }
                _selectedFlat = FlatList.SingleOrDefault(i => i.Id == request.Address.Id);
                _floor = request.Floor;
                _entrance = request.Entrance;

                var house = _requestService.GetHouseById(request.Address.HouseId);
                CommissioningDate = house.CommissioningDate;
                ElevatorCount = house.ElevatorCount;
                ServiceCompany = house.ServiceCompanyName;
                CityRegion = house.RegionName;

                FromTime = request.FromTime;
                ToTime = request.ToTime;
                //var requestModel = RequestList.FirstOrDefault();
                //requestModel.SetRequest(request);
                
                ContactList = new ObservableCollection<ContactDto>(request.Contacts);
                if (_selectedFlat != null)
                {
                    LoadRequestsBySelectedAddress(_selectedFlat.Id);
                }
                //Для обновления CanEdit и прочего
                RequestId = request.Id;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void GetInfoByHouseAndType(int? selectedHouseId, int? typeId)
        {
            LoadServiceCompanyInfo(selectedHouseId, typeId);
        }
        private void LoadServiceCompanyInfo(int? houseId, int? typeId)
        {
            if (_view == null)
                return;

            var flowDoc = _view.FlowInfo.Document;

            var flowDocument = houseId.HasValue && typeId.HasValue ? _requestService.GetHouseTypeInfo(houseId.Value, typeId.Value): "";
            if (string.IsNullOrEmpty(flowDocument))
            {
                flowDocument = _selectedServiceCompanyId.HasValue && typeId.HasValue ? _requestService.GetServiceCompanyTypeInfo(_selectedServiceCompanyId.Value, typeId.Value) : "";
            }
            var content = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
            if (content.CanLoad(System.Windows.DataFormats.Xaml))
            {
                using (var stream = new MemoryStream())
                {
                    var buffer = Encoding.Default.GetBytes(flowDocument);
                    stream.Write(buffer, 0, buffer.Length);
                    if (stream.Length > 0)
                    {
                        content.Load(stream, System.Windows.DataFormats.Xaml);
                        IsExpanded = true;
                        ExistForServiceInfo = true;
                    }
                    else
                    {
                        content.Text = "";
                        if (!ExistForAllInfo)
                        {
                            IsExpanded = false;
                        }
                        ExistForServiceInfo = false;

                    }
                }
            }
        }
        private void LoadInfoForAll()
        {
            if (_view == null)
                return;
            
            var flowDoc = _view.InfoForAll.Document;

            var flowDocument = _requestService.GetInfoForAll();
            
            var content = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
            if (content.CanLoad(System.Windows.DataFormats.Xaml))
            {
                using (var stream = new MemoryStream())
                {
                    var buffer = Encoding.Default.GetBytes(flowDocument);
                    stream.Write(buffer, 0, buffer.Length);
                    if (stream.Length > 0)
                    {
                        content.Load(stream, System.Windows.DataFormats.Xaml);
                        IsExpanded = true;
                        ExistForAllInfo = true;
                    }
                    else
                    {
                        content.Text = "";
                        if (!ExistForServiceInfo)
                        {
                            IsExpanded = false;
                        }
                        ExistForAllInfo = true;
                    }
                }
            }
        }
    }
}
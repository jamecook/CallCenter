using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using conaito;
using CRMPhone.Annotations;
using MySql.Data.MySqlClient;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using System.Diagnostics;
using System.Security.RightsManagement;
using CRMPhone.Dialogs;

namespace CRMPhone.ViewModel
{
    public class CRMContext : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _refreshTimer;
        private MySqlConnection _dbRefreshConnection;
        private UserAgent _sipAgent;
        //public MainWindow mainWindow;

        private DateTime _lastAliveTime;
        private string _sipUser;
        private string _sipSecret;
        private string _serverIP;
        private string _incomingCallFrom = "";

        public string serverIp => _serverIP;

        private string _sipState;
        private string _sipPhone;
        private int _lineNum;
        private ObservableCollection<ActiveChannelsDto> _activeChannels;
        private ObservableCollection<NotAnsweredDto> _notAnsweredCalls;
        private ObservableCollection<CallsListDto> _callsList;
        private SoundPlayer _ringPlayer;
        public MainWindow mainWindow { get; set; }

        public string AppTitle
        {
            get { return _appTitle; }
            set {
                _appTitle = value;
                OnPropertyChanged(nameof(AppTitle));
            }
        }

        public Visibility PhoneVisibility { get { return EnablePhone?Visibility.Visible:Visibility.Collapsed;} }

        public bool EnablePhone
        {
            get { return _enablePhone; }
            set
            {
                _enablePhone = value;
                OnPropertyChanged(nameof(EnablePhone));
                OnPropertyChanged(nameof(PhoneVisibility));
            }
        }
        public Visibility IsAdminRoleExist
        {
            get { if(AppSettings.CurrentUser != null && AppSettings.CurrentUser.Roles.Exists(r => r.Name == "admin"))
                    {return Visibility.Visible;}
                    return Visibility.Collapsed;
            }
        }

        public CRMContext()
        {
            IsMuted = false;
            _serverIP = ConfigurationManager.AppSettings["CallCenterIP"];
            LineNum = 0;
            _canRegistration = true;
            _canExecute = true;
            _refreshTimer = new DispatcherTimer();
            ActiveChannels = new ObservableCollection<ActiveChannelsDto>();
            NotAnsweredCalls = new ObservableCollection<NotAnsweredDto>();
            MetersHistoryList = new ObservableCollection<MeterListDto>();
            CallsList = new ObservableCollection<CallsListDto>();
            var uri = new Uri(@"pack://application:,,,/Resources/ringin.wav");
            _ringPlayer = new SoundPlayer(Application.GetResourceStream(uri).Stream);
            _lastAliveTime = DateTime.Today;
            EnablePhone = false;
            RequestDataContext = new RequestControlContext();
            ServiceCompanyDataContext = new ServiceCompanyControlContext();
            WorkerAdminDataContext = new WorkerAdminControlContext();
            SpecialityAdminContext = new SpecialityControlContext();
            ServiceAdminContext = new ServiceAdminControlContext();
            HouseAdminContext = new HouseAdminControlContext();
            RedirectAdminContext = new RedirectAdminControlContext();
            BlackListContext = new BlackListControlContext();
            AlertRequestDataContext = new AlertRequestControlContext();
            AlertAndWorkContext = new AlertAndWorkControlContext();
            AlertRequestControlModel = new AlertRequestControlModel();
        }

        public AlertRequestControlModel AlertRequestControlModel
        {
            get { return _alertRequestControlModel; }
            set { _alertRequestControlModel = value; OnPropertyChanged(nameof(AlertRequestControlModel));}
        }

        public void InitMysqlAndSip()
        {
            if (!string.IsNullOrEmpty(SipUser))
            {
                EnablePhone = true;
                SipRegister();
            }
            InitMySql();
            AppTitle = $"Call Center. {AppSettings.CurrentUser.SurName} {AppSettings.CurrentUser.FirstName} {AppSettings.CurrentUser.PatrName} ({AppSettings.SipInfo?.SipUser})";
            AlertRequestDataContext.InitCollections();
            RequestDataContext.InitCollections();
            ServiceCompanyDataContext.RefreshList();
            WorkerAdminDataContext.RefreshList();
            SpecialityAdminContext.RefreshList();
            ServiceAdminContext.RefreshParentServiceList();
            HouseAdminContext.RefreshCities();
            RedirectAdminContext.Refresh();
            BlackListContext.RefreshList();
            AlertAndWorkContext.InitCollections();
            OnPropertyChanged(nameof(IsAdminRoleExist));
        }
        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; OnPropertyChanged(nameof(FromDate)); }
        }

        public DateTime ToDate
        {
            get { return _toDate; }
            set { _toDate = value; OnPropertyChanged(nameof(ToDate)); }
        }

        public string SipState {
            get { return _sipState; }
            set { _sipState = value; OnPropertyChanged(nameof(SipState));}
        }

        public string IncomingCallFrom
        {
            get { return _incomingCallFrom; }
            set { _incomingCallFrom = value;
                AppSettings.LastIncomingCall = value;
                OnPropertyChanged(nameof(IncomingCallFrom)); }
        }

        public string LastAnsweredPhoneNumber
        {
            get { return _lastAnsweredPhoneNumber; }
            set
            {
                _lastAnsweredPhoneNumber = value; 
                OnPropertyChanged(nameof(LastAnsweredPhoneNumber));
            }
        }

        public ServiceCompanyDto CallFromServiceCompany
        {
            get { return _callFromServiceCompany; }
            set
            {
                _callFromServiceCompany = value;
                OnPropertyChanged(nameof(CallFromServiceCompany));
            }
        }

        public string SipPhone {
            get { return _sipPhone; }
            set { _sipPhone = value; OnPropertyChanged(nameof(SipPhone));}
        }

        public string SipSecret
        {
            get { return _sipSecret; }
            set { _sipSecret = value; OnPropertyChanged(nameof(SipSecret)); }
        }

        public string SipUser
        {
            get { return _sipUser; }
            set { _sipUser = value; OnPropertyChanged(nameof(SipUser)); }
        }

        public int LineNum
        {
            get { return _lineNum; }
            set { _lineNum = value; OnPropertyChanged(nameof(LineNum));}
        }

        public int CallsCount
        {
            get { return _callsCount; }
            set { _callsCount = value; OnPropertyChanged(nameof(CallsCount)); }
        }

        public ObservableCollection<ActiveChannelsDto> ActiveChannels
        {
            get { return _activeChannels; }
            set { _activeChannels = value; OnPropertyChanged(nameof(ActiveChannels));}
        }

        public ObservableCollection<NotAnsweredDto> NotAnsweredCalls
        {
            get { return _notAnsweredCalls; }
            set
            {
                _notAnsweredCalls = value;
                OnPropertyChanged(nameof(NotAnsweredCalls));
            }
        }

        public NotAnsweredDto SelectedCall { get; set; }

        public ObservableCollection<CallsListDto> CallsList
        {
            get { return _callsList; }
            set
            {
                _callsList = value;
                OnPropertyChanged(nameof(CallsList));
            }
        }

        public CallsListDto SelectedRecordCall
        {
            get { return _selectedRecordCall; }
            set { _selectedRecordCall = value; OnPropertyChanged(nameof(SelectedRecordCall));}
        }

        private ICommand _playCommand;
        public ICommand PlayCommand { get { return _playCommand ?? (_playCommand = new RelayCommand(PlayRecord)); } }

        private void PlayRecord(object obj)
        {
            var record = obj as CallsListDto;
            var serverIpAddress = ConfigurationManager.AppSettings["CallCenterIP"];
            var localFileName = record.MonitorFileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\").Replace("/","\\");
            Process.Start(localFileName);
        }


        public Brush MuteButtonBackground
        {
            get { return _muteButtonBackground; }
            set
            {
                _muteButtonBackground = value;
                OnPropertyChanged(nameof(MuteButtonBackground));
            }
        }

        public bool DisableIncomingCalls { get; set; }

        private bool _canRegistration;
        private bool _canExecute;
        public bool EnableRegistration => _canRegistration;
        private ICommand _registrationCommand;
        public ICommand RegistrationCommand { get {return _registrationCommand ?? (_registrationCommand = new CommandHandler(SipRegister, _canRegistration));}}

        private ICommand _callCommand;
        public ICommand CallCommand { get {return _callCommand ?? (_callCommand = new CommandHandler(Call, _canExecute));}}

        private ICommand _serviceCompanyInfoCommand;
        public ICommand ServiceCompanyInfoCommand { get { return _serviceCompanyInfoCommand ?? (_serviceCompanyInfoCommand = new CommandHandler(ServiceCompanyInfo, _canExecute)); } }

        private ICommand _openMetersCommand;
        public ICommand OpenMetersCommand { get { return _openMetersCommand ?? (_openMetersCommand = new RelayCommand(OpenMeters)); } }


        private void ServiceCompanyInfo()
        {
            if(CallFromServiceCompany!=null)
            {
                var info = _requestService.GetServiceCompanyInfo(CallFromServiceCompany.Id);
                var view = new ServiceCompanyInfoDialog();
                var model = new ServiceCompanyInfoDialogViewModel(CallFromServiceCompany.Name,info);
                view.DataContext = model;
                view.Owner = mainWindow;
                view.ShowDialog();

            }
        }

        private ICommand _deleteNotAnsweredCommand;
        public ICommand DeleteNotAnsweredCommand { get { return _deleteNotAnsweredCommand ?? (_deleteNotAnsweredCommand = new CommandHandler(DeleteNotAnswered, _canExecute)); } }

        private void DeleteNotAnswered()
        {
            _requestService.DeleteNotAnswered();
        }

        public DateTime MetersToDate
        {
            get { return _metersToDate; }
            set { _metersToDate = value; OnPropertyChanged(nameof(MetersToDate)); }
        }

        public DateTime MetersFromDate
        {
            get { return _metersFromDate; }
            set { _metersFromDate = value; OnPropertyChanged(nameof(MetersFromDate));}
        }

        public ObservableCollection<MeterListDto> MetersHistoryList
        {
            get { return _metersHistoryList; }
            set { _metersHistoryList = value; OnPropertyChanged(nameof(MetersHistoryList));}
        }

        public ObservableCollection<ServiceCompanyDto> MetersSCList
        {
            get { return _metersScList; }
            set { _metersScList = value; OnPropertyChanged(nameof(MetersSCList));}
        }

        public MeterListDto SelectedMeter
        {
            get { return _selectedMeter; }
            set { _selectedMeter = value; OnPropertyChanged(nameof(SelectedMeter)); }
        }

        public ServiceCompanyDto SelectedMetersSC
        {
            get { return _selectedMetersSc; }
            set { _selectedMetersSc = value; OnPropertyChanged(nameof(SelectedMetersSC));}
        }

        private ICommand _holdCommand;
        public ICommand HoldCommand { get {return _holdCommand ?? (_holdCommand = new CommandHandler(Hold, _canExecute));}}

        private ICommand _unholdCommand;
        public ICommand UnholdCommand { get {return _unholdCommand ?? (_unholdCommand = new CommandHandler(UnHold, _canExecute));}}

        private ICommand _hangUpCommand;
        public ICommand HangUpCommand { get {return _hangUpCommand ?? (_hangUpCommand = new CommandHandler(HangUpAll, _canExecute));}}

        private ICommand _transferCommand;
        public ICommand TransferCommand { get { return _transferCommand ?? (_transferCommand = new CommandHandler(Transfer, _canExecute)); } }
        
        private ICommand _addMeterCommand;
        public ICommand AddMeterCommand { get { return _addMeterCommand ?? (_addMeterCommand = new CommandHandler(AddMeters, _canExecute)); } }

        private ICommand _refreshMeterCommand;
        public ICommand RefreshMeterCommand { get { return _refreshMeterCommand ?? (_refreshMeterCommand = new CommandHandler(RefreshMeters, _canExecute)); } }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(Delete, _canExecute)); } }

        private void Delete()
        {
            if (SelectedMeter != null)
            {
                _requestService.DeleteMeter(SelectedMeter.Id);
                RefreshMeters();
            }
        }

        private void RefreshMeters()
        {
            MetersHistoryList.Clear();
            var meters = _requestService.GetMetersByDate(SelectedMetersSC?.Id, MetersFromDate, MetersToDate);
            foreach (var meter in meters)
            {
                MetersHistoryList.Add(meter);
            }
        }

        private void OpenMeters(object sender)
        {
            var selectedItem = sender as MeterListDto;
            if (selectedItem == null)
                return;
            if (_requestService == null)
                _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);

            var model = new MeterDeviceViewModel(selectedItem);
            var view = new MeterDeviceDialog();
            model.SetView(view);
            model.PhoneNumber = LastAnsweredPhoneNumber;
            view.DataContext = model;
            view.Owner = mainWindow;
            view.ShowDialog();
        }
        private void AddMeters()
        {
            var model = new MeterDeviceViewModel();
            var view = new MeterDeviceDialog();
            model.SetView(view);
            model.PhoneNumber = LastAnsweredPhoneNumber;
            view.DataContext = model;
            view.Owner = mainWindow;
            view.ShowDialog();

        }

        private ICommand _muteCommand;
        public ICommand MuteCommand { get { return _muteCommand ?? (_muteCommand = new CommandHandler(Mute, _canExecute)); } }

        private ICommand _deleteNumberFromListCommand;
        public ICommand DeleteNumberFromListCommand { get { return _deleteNumberFromListCommand ?? (_deleteNumberFromListCommand = new CommandHandler(DeleteNumberFromList, _canExecute)); } }

        private DateTime _fromDate;
        private DateTime _toDate;
        private int _callsCount;
        private Brush _muteButtonBackground;
        private bool _isMuted;
        private string _appTitle;
        private bool _enablePhone;
        private RequestControlContext _requestDataContext;
        private RequestService _requestService;
        private string _requestNum;
        private RequestUserDto _selectedUser;
        private ObservableCollection<RequestUserDto> _userList;
        private ServiceCompanyDto _callFromServiceCompany;
        private string _lastAnsweredPhoneNumber;
        private ServiceCompanyControlContext _serviceCompanyDataContext;
        private WorkerAdminControlContext _workerAdminDataContext;
        private SpecialityControlContext _specialityAdminContext;
        private ServiceAdminControlContext _serviceAdminContext;
        private HouseAdminControlContext _houseAdminContext;
        private RedirectAdminControlContext _redirectAdminContext;
        private BlackListControlContext _blackListContext;
        private DateTime _metersToDate;
        private DateTime _metersFromDate;
        private ObservableCollection<MeterListDto> _metersHistoryList;
        private ObservableCollection<ServiceCompanyDto> _metersScList;
        private ServiceCompanyDto _selectedMetersSc;
        private AlertRequestControlContext _alertRequestDataContext;
        private AlertAndWorkControlContext _alertAndWorkContext;
        private MeterListDto _selectedMeter;
        private ICommand _refreshCommand;

        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new CommandHandler(RefreshList, _canExecute)); } }

        private ICommand _addRequestToCallCommand;
        private CallsListDto _selectedRecordCall;
        private ServiceCompanyDto _selectedCompany;
        private ObservableCollection<ServiceCompanyDto> _companyList;
        private Color _alertRequestColor;
        private AlertRequestControlModel _alertRequestControlModel;

        public ICommand AddRequestToCallCommand { get { return _addRequestToCallCommand ?? (_addRequestToCallCommand = new CommandHandler(AddRequestToCall, _canExecute)); } }

        private void AddRequestToCall()
        {
            if(SelectedRecordCall == null)
                return;
            var model = new AddRequestToCallDialogViewModel();
            var view = new AddRequestToCallDialog();
            view.DataContext = model;
            model.SetView(view);
            view.Owner = mainWindow;
            if (view.ShowDialog()??false)
            {
                _requestService.AddCallToRequest(model.RequestId,SelectedRecordCall.UniqueId);
            }
        }

        public bool IsMuted
        {
            get { return _isMuted; }
            private set
            {
                _isMuted = value;
                if (value)
                {
                    MuteButtonBackground = new SolidColorBrush(Color.FromRgb(236, 128, 128));
                }
                else
                {
                    MuteButtonBackground = null;
                }
            }
        }

        public Color AlertRequestColor
        {
            get { return _alertRequestColor; }
            set { _alertRequestColor = value; OnPropertyChanged(nameof(AlertRequestColor));}
        }

        public RequestControlContext RequestDataContext
        {
            get { return _requestDataContext; }
            set { _requestDataContext = value; OnPropertyChanged(nameof(RequestDataContext)); }
        }

        public AlertAndWorkControlContext AlertAndWorkContext
        {
            get { return _alertAndWorkContext; }
            set { _alertAndWorkContext = value; OnPropertyChanged(nameof(AlertAndWorkContext));}
        }

        public AlertRequestControlContext AlertRequestDataContext
        {
            get { return _alertRequestDataContext; }
            set { _alertRequestDataContext = value; OnPropertyChanged(nameof(AlertRequestDataContext));}
        }

        public ServiceCompanyControlContext ServiceCompanyDataContext
        {
            get { return _serviceCompanyDataContext; }
            set { _serviceCompanyDataContext = value; OnPropertyChanged(nameof(ServiceCompanyDataContext));}
        }

        public WorkerAdminControlContext WorkerAdminDataContext
        {
            get { return _workerAdminDataContext; }
            set { _workerAdminDataContext = value; OnPropertyChanged(nameof(WorkerAdminDataContext));}
        }

        public ServiceAdminControlContext ServiceAdminContext
        {
            get { return _serviceAdminContext; }
            set { _serviceAdminContext = value; OnPropertyChanged(nameof(ServiceAdminContext));}
        }

        public HouseAdminControlContext HouseAdminContext
        {
            get { return _houseAdminContext; }
            set { _houseAdminContext = value; OnPropertyChanged(nameof(HouseAdminContext)); }
        }

        public RedirectAdminControlContext RedirectAdminContext
        {
            get { return _redirectAdminContext; }
            set { _redirectAdminContext = value; OnPropertyChanged(nameof(RedirectAdminContext)); }
        }

        public BlackListControlContext BlackListContext
        {
            get { return _blackListContext; }
            set { _blackListContext = value; OnPropertyChanged(nameof(BlackListContext));}
        }

        public SpecialityControlContext SpecialityAdminContext
        {
            get { return _specialityAdminContext; }
            set { _specialityAdminContext = value; OnPropertyChanged(nameof(SpecialityAdminContext));}
        }

        public string RequestNum
        {
            get { return _requestNum; }
            set { _requestNum = value; OnPropertyChanged(nameof(RequestNum)); }
        }

        public ObservableCollection<RequestUserDto> UserList
        {
            get { return _userList; }
            set { _userList = value; OnPropertyChanged(nameof(UserList));}
        }

        public RequestUserDto SelectedUser
        {
            get { return _selectedUser; }
            set { _selectedUser = value; OnPropertyChanged(nameof(SelectedUser)); }
        }

        public ServiceCompanyDto SelectedCompany
        {
            get { return _selectedCompany; }
            set { _selectedCompany = value; OnPropertyChanged(nameof(SelectedCompany)); }
        }

        public ObservableCollection<ServiceCompanyDto> CompanyList
        {
            get { return _companyList; }
            set { _companyList = value; OnPropertyChanged(nameof(CompanyList));}
        }

        private void InitMySql()
        {
            var connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", _serverIP, "asterisk", "mysqlasterisk", "asterisk");
            _dbRefreshConnection = new MySqlConnection(connectionString);
            try
            {
                _dbRefreshConnection.Open();
                _requestService = new RequestService(_dbRefreshConnection);
                UserList = new ObservableCollection<RequestUserDto>(_requestService.GetOperators());
                CompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
                MetersSCList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
                var curDate = _requestService.GetCurrentDate().Date;
                FromDate = curDate;
                ToDate = FromDate.AddDays(1);
                MetersToDate = curDate;
                MetersFromDate = curDate.AddDays(-30);

                if (EnablePhone)
                {
                _refreshTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                _refreshTimer.Tick += RefreshTimerOnTick;
                _refreshTimer.Start();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка в приложении! Приложение будет закрыто!\r\n"+ex.Message,"Ошибка");
                Environment.Exit(0);
            }
        }

        public void InitSip()
        {
            #region Создаем и настраиваем SIP-агента

            if (_sipAgent == null)
            {
                _sipAgent = new UserAgent();
                _sipAgent.OnConnected += OnConnected;
                _sipAgent.OnRegistered += RegisterSIP;
                _sipAgent.OnIncomingCall += IncomingCall;
                _sipAgent.OnTerminated += TerminateCall;
                _sipAgent.OnRegistrationFailed += SIPRegError;
                _sipAgent.OnUnregistered += SipAgentOnUnregistered;

                _sipAgent.AddTransport(1, 5060);
                var mediaPort = _sipAgent.FindPort(10000, 20000, 2, 1);
                _sipAgent.Startup(mediaPort, 1, "", "");

            }
            try
            {
                _sipAgent.Registrator.Register(_serverIP, _sipUser, _sipSecret, _sipUser);
                /*
                object names = null;
                object ids = null;
                _sipAgent.VoiceSettings.GetPlayers(out names, out ids);
                var playersId = ids as int[];

                _sipAgent.VoiceSettings.PlayerDevice = playersId[0];
                _sipAgent.VoiceSettings.GetRecorders(out names, out ids);
                var recordsId = ids as int[];

                _sipAgent.VoiceSettings.RecorderDevice = recordsId[0];
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при подключении к АТС!\r\n" +
                                "Для использования звонков необходимо перезагрузить приложение!\r\n"
                                + ex.Message, "Ошибка");
            }

            #endregion
        }

        public void RefreshList()
        {
            CallsList = new ObservableCollection<CallsListDto>(_requestService.GetCallList(FromDate, ToDate, RequestNum, SelectedUser?.Id, SelectedCompany?.Id));
            CallsCount = CallsList.Count;
        }
        private void RefreshTimerOnTick(object sender, EventArgs eventArgs)
        {
            RefreshActiveChannels();
            RefreshNotAnsweredCalls();
            TimeSpan t = DateTime.Now - _lastAliveTime;
            if (t.TotalSeconds > 30)
            {
                SendAlive();
                RefreshAlertRequest();
                _lastAliveTime = DateTime.Now;
            }
        }

        private void RefreshAlertRequest()
        {
            var alertedRequests = _requestService.GetAlertedRequests();
            if (alertedRequests.Count > 0 && _sipAgent.CallMaker.callStatus[0] < 0)
            {
                AlertRequestControlModel.RequestList.Clear();
                foreach (var request in alertedRequests)
                {
                    AlertRequestControlModel.RequestList.Add(request);
                }
                AlertRequestControlModel.RequestCount = alertedRequests.Count;
                mainWindow.ShowNotify("Напоминалка!", "Обнаружены заявки требующие контроля");
            }
        }

        private void SendAlive()
        {
            
        }
        private void RefreshActiveChannels()
        {
            var readedChannels = new List<ActiveChannelsDto>();
            using (var cmd = new MySqlCommand("SELECT * FROM asterisk.ActiveChannels where Application = 'queue' and BridgeId is null", _dbRefreshConnection))
            using (var dataReader = cmd.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    readedChannels.Add(new ActiveChannelsDto
                    {
                        UniqueId = dataReader.GetNullableString("UniqueID"),
                        Channel = dataReader.GetNullableString("Channel"),
                        CallerIdNum = dataReader.GetNullableString("CallerIDNum"),
                        ChannelState = dataReader.GetNullableString("ChannelState"),
                        AnswerTime = dataReader.GetNullableDateTime("AnswerTime"),
                        CreateTime = dataReader.GetNullableDateTime("CreateTime")
                    });
                }
                dataReader.Close();
            }
            var remotedChannels = ActiveChannels.Where(n => readedChannels.All(c => c.UniqueId != n.UniqueId)).ToList();
            var newChannels = readedChannels.Where(n => ActiveChannels.All(c => c.UniqueId != n.UniqueId)).ToList();
            newChannels.ForEach(c => ActiveChannels.Add(c));
            remotedChannels.ForEach(c => ActiveChannels.Remove(c));
        }

        private void RefreshNotAnsweredCalls()
        {
            var callList = new List<NotAnsweredDto>();
            using (var cmd = new MySqlCommand("SELECT UniqueID, CallerIDNum, CreateTime FROM asterisk.NotAnswered", _dbRefreshConnection))
            using (var dataReader = cmd.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    callList.Add(new NotAnsweredDto
                    {
                        UniqueId = dataReader.GetNullableString("UniqueID"),
                        CallerId = dataReader.GetNullableString("CallerIDNum"),
                        CreateTime = dataReader.GetNullableDateTime("CreateTime")
                    });
                }
                dataReader.Close();
            }
            var remotedCalls = NotAnsweredCalls.Where(n => !callList.Any(c=>c.CallerId == n.CallerId && c.CreateTime == n.CreateTime)).ToList();
            var newCalls = callList.Where(n => !NotAnsweredCalls.Any(c => c.CallerId == n.CallerId && c.CreateTime == n.CreateTime)).ToList();
            newCalls.ForEach(c=>NotAnsweredCalls.Add(c));
            remotedCalls.ForEach(c=>NotAnsweredCalls.Remove(c));
        }

        #region SIP Events
        public void SipRegister()
        {
            InitSip();
        }

        public void DeleteNumberFromList()
        {
            if(SelectedCall == null)
                return;
            DateTime currentDate;
            using (var cmd = new MySqlCommand("SELECT sysdate()", AppSettings.DbConnection))
            using (var dataReader = cmd.ExecuteReader())
            {
                currentDate = dataReader.GetDateTime(1);
            }
            if (SelectedCall.CreateTime != null && currentDate < SelectedCall.CreateTime.Value.AddDays(1))
            {
                MessageBox.Show("Вы не можете удалить запись с даты создания которой прошло меньше суток!",
                    "Предупреждение");
                return;
            }
            using (var cmd = new MySqlCommand($"delete from asterisk.NotAnsweredQueue where UniqueID = {SelectedCall.UniqueId}",
                        AppSettings.DbConnection))
            {
                cmd.ExecuteNonQuery();
            }

        }

        public void Call()
        {
            if (_sipAgent.CallMaker.callStatus[0] == 180)
            {
                //if (DisableIncomingCalls)
                //{
                //    _sipAgent.CallMaker.HangupAll();
                //    string callId = string.Format("sip:{0}@{1}", _sipPhone, _serverIP);
                //    SipState = $"Исходящий вызов на номер {_sipPhone}";
                //    _sipAgent.CallMaker.Invite(callId);
                //    return;
                //}
                LastAnsweredPhoneNumber = IncomingCallFrom;

                _sipAgent.CallMaker.Accept(0);
                return;
            }
            if (string.IsNullOrEmpty(_sipPhone))
                return;
            if (_sipAgent.CallMaker.callStatus[0] < 0)
            {
                string callId = string.Format("sip:{0}@{1}", _sipPhone, _serverIP);
                SipState = $"Исходящий вызов на номер {_sipPhone}";
                _sipAgent.CallMaker.Invite(callId);
            }
        }

        public void CallFromList()
        {
            if (SelectedCall == null)
                return;
            SipPhone = SelectedCall.CallerId;
            string callId = string.Format("sip:{0}@{1}", SelectedCall.CallerId, _serverIP);
            _sipAgent.CallMaker.Invite(callId);
        }

        public void Mute()
        {
            if (_sipAgent.CallMaker.callStatus[0] < 0)
                return;
            IsMuted = !IsMuted;
            _sipAgent.VoiceSettings.MuteLineMic(0,IsMuted);
        }

        public void Transfer()
        {
            var phoneList = new List<string> {"101", "102", "103", "104", "105"};
            phoneList.Remove(_sipUser);
            var transferContext = new TrasferDialogViewModel(phoneList);
            var transfer = new TransferDialog(transferContext);
            transfer.Owner = Application.Current.MainWindow;
            if (transfer.ShowDialog() == true)
            {
                string callId = string.Format("sip:{0}@{1}", transferContext.ClientPhone, _serverIP);
                _sipAgent.CallMaker.Transfer(0,callId);
            }

        }

        public void Hold()
        {
            _sipAgent.CallMaker.Hold(LineNum);
        }
        public void UnHold()
        {
            _sipAgent.CallMaker.Reinvite(LineNum);
        }
        public void HangUpAll()
        {
            _sipAgent.CallMaker.HangupAll();
            IncomingCallFrom = "";
            CallFromServiceCompany = null;
        }

        public void Unregister()
        {
            using (var cmd = new MySqlCommand($"call CallCenter.LogoutUser({AppSettings.CurrentUser.Id})", AppSettings.DbConnection))
            {
                cmd.ExecuteNonQuery();
            }
            _sipAgent?.Registrator.Unregister();
            _sipAgent?.Shutdown();
        }


        private void SipAgentOnUnregistered()
        {
            SipState = "UnRegistered!";
            _canRegistration = true;
            _ringPlayer.Stop();
            OnPropertyChanged(nameof(EnableRegistration));
        }

        private void SIPRegError(int code, string reason)
        {
            SipState = $"Ошибка регистрации! {reason}";
            _canRegistration = true;
            OnPropertyChanged(nameof(EnableRegistration));
        }

        private void TerminateCall(int callId, string remoteUri, string contact, int code, string statusText)
        {
            var phoneNumber = GetPhoneNumberFromUri(remoteUri);
            SipState = $"Звонок завершен {phoneNumber} ({statusText})";
            _ringPlayer.Stop();
            IsMuted = false;
        }

        private static string GetPhoneNumberFromUri(string remoteUri)
        {
            var phoneNumber = string.Empty;
            var lines = remoteUri.Split(':', '@');
            if (lines.Length > 1)
            {
                phoneNumber = lines[1].Trim();
            }
            return phoneNumber;
        }

        private void IncomingCall(int callId, string remoteUri, string toUri, string contact)
        {
            //if (DisableIncomingCalls)
            //{
            //    _sipAgent.CallMaker.Hangup(callId);
            //    return;
            //}

            var phoneNumber = GetPhoneNumberFromUri(remoteUri);
            CallFromServiceCompany = _requestService.ServiceCompanyByIncommingPhoneNumber(phoneNumber);
            SipState = $"Входящий вызов от {phoneNumber}";
            IncomingCallFrom = phoneNumber;

            _ringPlayer.PlayLooping();
            //Bring To Front
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (!mainWindow.IsVisible)
                {
                    mainWindow.Show();
                }

                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
                mainWindow.Activate();
                mainWindow.Topmost = true; // important
                mainWindow.Topmost = false; // important
                mainWindow.Focus(); // important
            }));
        }


        private void RegisterSIP()
        {
            SipState = "Успешная регистрация!";
            _canRegistration = false;
            OnPropertyChanged(nameof(EnableRegistration));
        }

        private void OnConnected(int callId, string remoteUri, string contact)
        {
            _ringPlayer.Stop();
            SipState = $"Связь установлена: {GetPhoneNumberFromUri(remoteUri)}";
        }
#endregion
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

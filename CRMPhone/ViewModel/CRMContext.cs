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
using CRMPhone.Annotations;
using MySql.Data.MySqlClient;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.RightsManagement;
using System.Threading;
using System.Xml.Linq;
using CRMPhone.Dialogs;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using NLog;
using SIPEVOActiveXLib;
using Color = System.Windows.Media.Color;

namespace CRMPhone.ViewModel
{
    public class CRMContext : INotifyPropertyChanged
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly DispatcherTimer _refreshTimer;
        private MySqlConnection _dbRefreshConnection;
        private MySqlConnection _dbMainConnection;
        private SIPClientCtl _sipClient;
        //public MainWindow mainWindow;

       private SipLine[] _sipLinesArray = {new SipLine { Id = 0, Name = "Линия 1:"}, new SipLine { Id = 1, Name = "Линия 2:" } };
        private DateTime _lastAliveTime;
        private string _sipUser;
        private string _sipSecret;
        private string _serverIP;
        private string _incomingCallFrom = "";
        private bool _sipCallActive = true;
        private int _maxLineNumber = 2;

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
            ContextSaver.CrmContext = this;
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
            SipLines = new ObservableCollection<SipLine>(_sipLinesArray);
            SelectedLine = SipLines.FirstOrDefault();
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
            RingUpAdminContext = new RingUpAdminControlContext();
            BlackListContext = new BlackListControlContext();
            AlertRequestDataContext = new AlertRequestControlContext();
            AlertAndWorkContext = new AlertAndWorkControlContext();
            AlertRequestControlModel = new AlertRequestControlModel();
            DispexRequestControlModel = new DispexRequestControlModel();
        }

        public AlertRequestControlModel AlertRequestControlModel
        {
            get { return _alertRequestControlModel; }
            set { _alertRequestControlModel = value; OnPropertyChanged(nameof(AlertRequestControlModel));}
        }

        public DispexRequestControlModel DispexRequestControlModel
        {
            get { return _dispexRequestControlModel; }
            set { _dispexRequestControlModel = value;
                OnPropertyChanged(nameof(DispexRequestControlModel));
            }
        }

        public void InitMysqlAndSip()
        {
            if (!string.IsNullOrEmpty(SipUser))
            {
                EnablePhone = true;
                SipRegister();
            }
            InitMySql();
            AppTitle = $"Call Center. {AppSettings.CurrentUser.SurName} {AppSettings.CurrentUser.FirstName} {AppSettings.CurrentUser.PatrName} ({AppSettings.SipInfo?.SipUser}) ver. {Assembly.GetEntryAssembly().GetName().Version}";
            AlertRequestDataContext.InitCollections();
            RequestDataContext.InitCollections();
            ServiceCompanyDataContext.RefreshList();
            WorkerAdminDataContext.RefreshList();
            SpecialityAdminContext.RefreshList();
            ServiceAdminContext.RefreshParentServiceList();
            HouseAdminContext.RefreshCities();
            RedirectAdminContext.Refresh();
            RingUpAdminContext.Refresh();
            BlackListContext.RefreshList();
            AlertAndWorkContext.InitCollections();
            OnPropertyChanged(nameof(IsAdminRoleExist));
            if (!string.IsNullOrEmpty(AppSettings.SipInfo?.SipUser))
            {
                using (var bridgeService = new AmiService(_serverIP, 5038))
                {
                    bridgeService.LoginAndQueuePause("zerg", "asteriskzerg", AppSettings.SipInfo.SipUser, false);
                }

            }
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

        public SipLine SelectedLine
        {
            get { return _selectedLine; }
            set { _selectedLine = value; OnPropertyChanged(nameof(SelectedLine));}
        }

        public ObservableCollection<SipLine> SipLines
        {
            get { return _sipLines; }
            set { _sipLines = value; OnPropertyChanged(nameof(SipLines));}
        }

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
            _requestService.PlayRecord(serverIpAddress,record.MonitorFileName);
/*
            var localFileName = record.MonitorFileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\").Replace("/","\\");
            var localFileNameMp3 = localFileName.Replace(".wav", ".mp3");
            if (File.Exists(localFileNameMp3))
                Process.Start(localFileNameMp3);
            else if (File.Exists(localFileNameMp3))
                Process.Start(localFileName);
            else
                MessageBox.Show($"Файл с записью недоступен!\r\n{localFileNameMp3}", "Ошибка");
*/
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

        private ICommand _getCallFromQueryCommand;
        public ICommand GetCallFromQueryCommand { get { return _getCallFromQueryCommand ?? (_getCallFromQueryCommand = new RelayCommand(GetCallFromQuery)); } }

        private void GetCallFromQuery(object currentChannel)
        {
            var item = currentChannel as ActiveChannelsDto;
            if(item == null || string.IsNullOrEmpty(item.Channel))
                return;
            if (_sipClient.CallState[0] != CallState.CallState_Free)
            {
                MessageBox.Show("Невозможно взять из очереди если занята первай линия!");
                return;
            }
            string callId = string.Format("sip:{0}@{1}", "123123321", _serverIP);
            _sipClient.PhoneLine = 0;
            _sipClient.Connect(callId);
            var bridgeThread = new Thread(BridgeFunc); //Создаем новый объект потока (Thread)
            
            bridgeThread.Start(item); //запускаем поток

        }

        private void BridgeFunc(object number)
        {
            var item = number as ActiveChannelsDto;
            //if(item == null || string.IsNullOrEmpty(item.Channel))
              //  return;
            Thread.Sleep(600);
            var channel1 = _requestService.GetCurrentChannel(_sipUser);
            if(string.IsNullOrEmpty(channel1))
                return;
            using (var bridgeService = new AmiService(_serverIP, 5038))
            {
                if (bridgeService.LoginAndBridge("zerg", "asteriskzerg", channel1, item.Channel))
                {
                    SipLines[0].Phone = item.CallerIdNum;
                    LastAnsweredPhoneNumber = item.CallerIdNum;
                    IncomingCallFrom = item.CallerIdNum;
                }
            }
        }

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
        private ICommand _conferenceCommand;
        public ICommand ConferenceCommand { get { return _conferenceCommand ?? (_conferenceCommand = new CommandHandler(Conference, _canExecute)); } }
        private ICommand _bridgeCommand;
        public ICommand BridgeCommand { get { return _bridgeCommand ?? (_bridgeCommand = new CommandHandler(Bridge, _canExecute)); } }

        private ICommand _hangUpCommand;
        public ICommand HangUpCommand { get {return _hangUpCommand ?? (_hangUpCommand = new CommandHandler(HangUp, _canExecute));}}

        private ICommand _transferCommand;
        public ICommand TransferCommand { get { return _transferCommand ?? (_transferCommand = new CommandHandler(Transfer, _canExecute)); } }

        private ICommand _queuePauseCommand;
        public ICommand QueuePauseCommand { get { return _queuePauseCommand ?? (_queuePauseCommand = new CommandHandler(QueuePause, _canExecute)); } }
        private ICommand _queueUnPauseCommand;
        public ICommand QueueUnPauseCommand { get { return _queueUnPauseCommand ?? (_queueUnPauseCommand = new CommandHandler(QueueUnPause, _canExecute)); } }

        private void QueueUnPause()
        {
            using (var bridgeService = new AmiService(_serverIP, 5038))
            {
                if (bridgeService.LoginAndQueuePause("zerg", "asteriskzerg", AppSettings.SipInfo.SipUser, false))
                {
                    MessageBox.Show("Теперь вы будете принимать звонки!");
                }
            }
        }

        private void QueuePause()
        {
            using (var bridgeService = new AmiService(_serverIP, 5038))
            {
                if (bridgeService.LoginAndQueuePause("zerg", "asteriskzerg", AppSettings.SipInfo.SipUser, true))
                {
                    MessageBox.Show("Вы не будете принимать звонки из очереди!");
                }
            }
        }

        private ICommand _addMeterCommand;
        public ICommand AddMeterCommand { get { return _addMeterCommand ?? (_addMeterCommand = new CommandHandler(AddMeters, _canExecute)); } }

        private ICommand _refreshMeterCommand;
        public ICommand RefreshMeterCommand { get { return _refreshMeterCommand ?? (_refreshMeterCommand = new CommandHandler(RefreshMeters, _canExecute)); } }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(Delete, _canExecute)); } }
        private ICommand _exportCommand;
        public ICommand ExportCommand { get { return _exportCommand ?? (_exportCommand = new CommandHandler(Export, _canExecute)); } }

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

        private ICommand _exportMetersCommand;

        public ICommand ExportMetersCommand { get { return _exportMetersCommand ?? (_exportMetersCommand = new CommandHandler(ExportMeters, _canExecute)); } }

        private ICommand _exportToTrizCommand;

        public ICommand ExportToTrizCommand { get { return _exportToTrizCommand ?? (_exportToTrizCommand = new CommandHandler(ExportToTriz, _canExecute)); } }

        private void ExportToTriz()
        {
            if (MetersHistoryList.Count == 0)
            {
                MessageBox.Show("Нельзя экспортировать пустой список!", "Ошибка");
                return;
            }
            try
            {
                var streetsNames = new Dictionary<string, string>();

                // Add some elements to the dictionary. There are no 
                // duplicate keys, but some of the values are duplicates.
                streetsNames.Add("Ледниковый", "Ледниковый проезд");
                streetsNames.Add("Александра Протозанова", "Протозанова");
                streetsNames.Add("Бориса Опрокиднева", "Опрокиднева");
                streetsNames.Add("Василия Подшибякина", "Подшибякина");
                streetsNames.Add("Дмитрия Менделеева", "Менделеева");
                streetsNames.Add("Мелиораторов", "Мелиораторов");
                streetsNames.Add("Николая Федорова", "Федорова");
                streetsNames.Add("Сидора Путилова", "Путилова");
                streetsNames.Add("Николая Зелинского", "Зелинского");
                streetsNames.Add("Первомайская", "Первомайская");
                streetsNames.Add("Ванцетти", "Ванцетти");

                var openFileDialog = new OpenFileDialog();
                openFileDialog.AddExtension = true;
                openFileDialog.DefaultExt = ".xlsx";
                openFileDialog.Filter = "Excel файл|*.xlsx";
                if (openFileDialog.ShowDialog() == true)
                {
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(openFileDialog.FileName, true))
                    {
                        WorkbookPart workbookPart = document.WorkbookPart;
                        WorksheetPart worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
                        SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                        //WorkbookStylesPart styles = workbookPart.GetPartsOfType<WorkbookStylesPart>().FirstOrDefault();
                        //// FillId = 1
                        //Fill fill2 = new Fill();
                        //var patternFill2 = new PatternFill() { PatternType = PatternValues.Gray125 };
                        //fill2.Append(patternFill2);
                        //styles.Stylesheet.Fills.Append(fill2);
                        //styles.Stylesheet.Save();
                        SharedStringTable sst = sstpart.SharedStringTable;
                        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                        var cells = sheetData.Descendants<Cell>();
                        var rows = sheetData.Descendants<Row>();

                        //Console.WriteLine("Row count = {0}", rows.LongCount());
                        //Console.WriteLine("Cell count = {0}", cells.LongCount());
                        var readedTriz = new List<TrizMeterListDto>();
                        uint styleId = 0;
                        var exported = new List<ExportedMeterDto>();
                        foreach (Row row in rows)
                        {
                            var newRow = new TrizMeterListDto(){RowId = row.RowIndex};
                            var cellIndex = 0;
                            foreach (Cell c in row.Elements<Cell>())
                            {
                                var value = "";
                                if ((c.DataType != null) && (c.DataType == CellValues.SharedString))
                                {
                                    var ssid = int.Parse(c.CellValue.Text);
                                    value = sst.ChildElements[ssid].InnerText;
                                }
                                else if (c.CellValue != null)
                                {
                                    value = c.CellValue.Text;
                                }
                                if (cellIndex == 4)
                                    newRow.Street = value;
                                else if(cellIndex == 5)
                                    newRow.Building = value;
                                else if(cellIndex == 6)
                                    newRow.Corpus = value.Replace(", корп","").Trim();
                                else if(cellIndex == 8)
                                    newRow.Flat = value;
                                else if(cellIndex == 16)
                                    newRow.ServiceName = value;
                                else if(cellIndex == 25)
                                    newRow.Position = value;
                                else if(cellIndex == 26)
                                    newRow.LastDate = value;
                                else if(cellIndex == 27)
                                    newRow.LastValue = value;
                                else if (cellIndex == 28)
                                {
                                    newRow.CurrentValue = value;
                                    if (row.RowIndex == 1)
                                    {
                                        styleId = c.StyleIndex;
                                    }

                                    else
                                    {
                                        //Проверка надо указывать значение или нет
                                        var findVal = MetersHistoryList.FirstOrDefault(m => m.StreetName == streetsNames[newRow.Street] &&
                                                        m.Building == newRow.Building && (m.Corpus??"") == newRow.Corpus &&
                                                        m.Flat == newRow.Flat);
                                        if (findVal != null)
                                        {
                                            if(exported.All(e => e.Id != findVal.Id))
                                                exported.Add(new ExportedMeterDto{Id = findVal.Id});
                                            double insertVal = -1;
                                            if (newRow.ServiceName == "Электроэнергия (день)" || newRow.ServiceName == "Электроэнергия(день)")
                                                insertVal = findVal.Electro1;
                                            if (newRow.ServiceName == "Электроэнергия(ночь)" || newRow.ServiceName == "Электроэнергия (ночь)")
                                                insertVal = findVal.Electro2;
                                            if (newRow.ServiceName == "Электроэнергия")
                                                insertVal = findVal.Electro1 + findVal.Electro2;

                                            if (newRow.ServiceName == "Горячее водоснабжение")
                                            {
                                                if (newRow.Position == "Кухня")
                                                {
                                                    insertVal = findVal.HotWater1;
                                                    exported.First(e => e.Id == findVal.Id).SavedHot1 = true;
                                                }
                                                else if(!string.IsNullOrEmpty(newRow.Position))
                                                {
                                                    insertVal = findVal.HotWater2;
                                                    exported.First(e => e.Id == findVal.Id).SavedHot2 = true;
                                                }
                                                else
                                                {
                                                    var meter = exported.First(e => e.Id == findVal.Id);
                                                    if (meter.SavedHot1)
                                                    {
                                                        insertVal = findVal.HotWater2;
                                                        meter.SavedHot2 = true;
                                                    }
                                                    else
                                                    {
                                                        insertVal = findVal.HotWater1;
                                                        meter.SavedHot1 = true;
                                                    }
                                                }
                                            }

                                            if (newRow.ServiceName == "Холодное водоснабжение")
                                            {
                                                if (newRow.Position == "Кухня")
                                                {
                                                    insertVal = findVal.ColdWater1;
                                                    exported.First(e => e.Id == findVal.Id).SavedCold1 = true;
                                                }
                                                else if (!string.IsNullOrEmpty(newRow.Position))
                                                {
                                                    insertVal = findVal.ColdWater2;
                                                    exported.First(e => e.Id == findVal.Id).SavedCold2 = true;
                                                }
                                            else
                                                {
                                                    var meter = exported.First(e => e.Id == findVal.Id);
                                                    if (meter.SavedCold1)
                                                    {
                                                        insertVal = findVal.ColdWater2;
                                                        meter.SavedCold2 = true;
                                                    }
                                                    else
                                                    {
                                                        insertVal = findVal.ColdWater1;
                                                        meter.SavedCold1 = true;
                                                    }
                                                }
                                            }

                                            c.CellValue = new CellValue(insertVal.ToString().Replace(",","."));

                                            if (styleId > 0)
                                                c.StyleIndex = styleId;
                                        }
                                    }
                                }
                                cellIndex++;
                            }
                            //readedTriz.Add(newRow);
                        }

                        
                            worksheetPart.Worksheet.Save();

                    }
                    MessageBox.Show("Экспортирование завершено!");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Произошла ошибка:\r\n" + exc.Message);
            }
        }

        private void ExportMeters()
        {
            if (MetersHistoryList.Count == 0)
            {
                MessageBox.Show("Нельзя экспортировать пустой список!", "Ошибка");
                return;
            }
            try
            {

                var saveDialog = new SaveFileDialog();
                saveDialog.AddExtension = true;
                saveDialog.DefaultExt = ".xlsx";
                saveDialog.Filter = "Excel файл|*.xlsx|XML Файл|*.xml";
                if (saveDialog.ShowDialog() == true)
                {
                    var fileName = saveDialog.FileName;
                    if (fileName.EndsWith(".xml"))
                    {
                        XElement root = new XElement("Records");
                        foreach (var record in MetersHistoryList)
                        {
                            root.AddFirst(
                                new XElement("Record",
                                    new[]
                                    {
                                        new XElement("Время", record.Date.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("УК", record.ServiceCompany),
                                        new XElement("Улица", record.StreetName),
                                        new XElement("Дом", record.Building),
                                        new XElement("Корпус", record.Corpus),
                                        new XElement("Квартира", record.Flat),
                                        new XElement("ЭлектроТ1", record.Electro1),
                                        new XElement("ЭлектроТ2", record.Electro2),
                                        new XElement("ГВСстояк1", record.HotWater1),
                                        new XElement("ХВСстояк1", record.ColdWater1),
                                        new XElement("ГВСстояк2", record.HotWater2),
                                        new XElement("ХВСстояк2", record.ColdWater2),
                                        new XElement("Отопление", record.Heating)
                                    }));
                        }
                        var saver = new FileStream(fileName, FileMode.Create);
                        root.Save(saver);
                        saver.Close();
                    }
                    if (fileName.EndsWith(".xlsx"))
                    {
                        File.Copy("templates\\meters.xlsx", fileName, true);
                        ExportMetersToExcelByTemplate(fileName);
                    }
                    MessageBox.Show("Данные сохранены в файл\r\n" + fileName);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Произошла ошибка:\r\n" + exc.Message);
            }
        }

        public void ExportMetersToExcelByTemplate(string fileName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, true))
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Inserting each rows
                foreach (var record in MetersHistoryList)
                {
                    {
                        var row = new Row();
                        row.Append(
                            ConstructCell(record.Date.ToString("dd.MM.yyyy HH:mm"), CellValues.String),
                            ConstructCell(record.ServiceCompany, CellValues.String),
                            ConstructCell(record.StreetName, CellValues.String),
                            ConstructCell(record.Building, CellValues.String),
                            ConstructCell(record.Corpus, CellValues.String),
                            ConstructCell(record.Flat, CellValues.String),
                            ConstructCell(record.Electro1.ToString(), CellValues.String),
                            ConstructCell(record.Electro2.ToString(), CellValues.String),
                            ConstructCell(record.HotWater1.ToString(), CellValues.String),
                            ConstructCell(record.ColdWater1.ToString(), CellValues.String),
                            ConstructCell(record.HotWater2.ToString(), CellValues.String),
                            ConstructCell(record.ColdWater2.ToString(), CellValues.String),
                            ConstructCell(record.Heating.ToString(), CellValues.String));

                        sheetData.AppendChild(row);
                    }
                    worksheetPart.Worksheet.Save();
                }
            }
        }

        private void Export()
        {
            if (CallsList.Count == 0)
            {
                MessageBox.Show("Нельзя экспортировать пустой список!", "Ошибка");
                return;
            }
            try
            {

                var saveDialog = new SaveFileDialog();
                saveDialog.AddExtension = true;
                saveDialog.DefaultExt = ".xlsx";
                saveDialog.Filter = "Excel файл|*.xlsx|XML Файл|*.xml";
                if (saveDialog.ShowDialog() == true)
                {
                    var fileName = saveDialog.FileName;
                    if (fileName.EndsWith(".xml"))
                    {
                        XElement root = new XElement("Records");
                        foreach (var record in CallsList)
                        {
                            root.AddFirst(
                                new XElement("Record",
                                    new[]
                                    {
                                        new XElement("ВремяЗвонка", record.CreateTime?.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("УК", record.ServiceCompany),
                                        new XElement("Направление", record.Direction  == "in" ? "вх." : "исх."),
                                        new XElement("Номер", record.CallerId),
                                        new XElement("ВремяОжидания", record.WaitingTime),
                                        new XElement("ВремяРазговора", record.TalkTime),
                                        new XElement("Заявки", record.Requests),
                                        new XElement("Оператор", record.User?.ShortName),
                                    }));
                        }
                        var saver = new FileStream(fileName, FileMode.Create);
                        root.Save(saver);
                        saver.Close();
                    }
                    if (fileName.EndsWith(".xlsx"))
                    {
                        File.Copy("templates\\calls.xlsx", fileName, true);
                        CreateExcelDocByTemplate(fileName);
                    }
                    MessageBox.Show("Данные сохранены в файл\r\n" + fileName);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Произошла ошибка:\r\n" + exc.Message);
            }
        }

        public void CreateExcelDocByTemplate(string fileName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, true)
            )
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Inserting each rows
                foreach (var record in CallsList)
                {
                    {
                        var row = new Row();
                        row.Append(
                            ConstructCell(record.CreateTime?.ToString("dd.MM.yyyy HH:mm"), CellValues.String),
                            ConstructCell(record.ServiceCompany, CellValues.String),
                            ConstructCell(record.Direction == "in" ? "вх." : "исх.", CellValues.String),
                            ConstructCell(record.CallerId, CellValues.String),
                            ConstructCell(record.WaitingTime?.ToString(), CellValues.String),
                            ConstructCell(record.TalkTime?.ToString(), CellValues.String),
                            ConstructCell(record.Requests, CellValues.String),
                            ConstructCell(record.User?.ShortName, CellValues.String));

                        sheetData.AppendChild(row);
                    }
                    worksheetPart.Worksheet.Save();
                }
            }
        }

        private Cell ConstructCell(string value, CellValues dataType)
        {
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType),
            };
        }


        private ICommand _addRequestToCallCommand;
        private CallsListDto _selectedRecordCall;
        private ServiceCompanyDto _selectedCompany;
        private ObservableCollection<ServiceCompanyDto> _companyList;
        private Color _alertRequestColor;
        private AlertRequestControlModel _alertRequestControlModel;
        private RingUpAdminControlContext _ringUpAdminContext;
        private ObservableCollection<SipLine> _sipLines;
        private SipLine _selectedLine;
        private DispexRequestControlModel _dispexRequestControlModel;

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

        public RingUpAdminControlContext RingUpAdminContext
        {
            get { return _ringUpAdminContext; }
            set { _ringUpAdminContext = value; OnPropertyChanged(nameof(RingUpAdminContext));}
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

        public string PhoneNumber { get; set; }
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
            _dbMainConnection = new MySqlConnection(connectionString);
            try
            {
                _dbRefreshConnection.Open();
                _dbMainConnection.Open();
                _requestService = new RequestService(_dbMainConnection);
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

            try
            {
                if (_sipClient == null)
                {
                    _sipClient = new SIPClientCtl();

                    _sipClient.OnConnected += SipClientOnConnected;
                    _sipClient.OnRegistrationSuccess += SipClientOnRegistrationSuccess;
                    _sipClient.OnUnregistration += SipClientOnUnregistration;
                    _sipClient.OnConnectingLine += SipClientOnConnectingLine;
                    _sipClient.OnTerminatedLine += SipClientOnTerminatedLine;
                    _sipClient.OnRegistrationFailure += SipClientOnRegistrationFailure;
                    _sipClient.OnHold += SipClientOnHold;

                    _sipClient.LogEnabled = false;
                    _sipClient.UserID = _sipUser;
                    _sipClient.LoginID = _sipUser;
                    _sipClient.Password = _sipSecret;
                    _sipClient.RegistrationProxy = _serverIP;
                    _sipClient.DisplayName = _sipUser;
                    _sipClient.Initialize(null);
                    _sipClient.TCPPort = -1;
                    _sipClient.Register();
                    _sipClient.PlayRingtone = false;

                    _sipClient.MaxPhoneLines = 2;
                    _sipClient.NoiseReduction = false;
                    _sipClient.AEC = false;
                    //_sipClient.EchoTail = 0;

                    //_sipClient.ConferenceJoin();
                    //_sipClient.ConferenceRemove();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при подключении к АТС!\r\n" +
                                "Для использования звонков необходимо перезагрузить приложение!\r\n"
                                + ex.Message, "Ошибка");
            }

            #endregion
        }

        private void SipClientOnHold(string sFromUri, string sLocalUri, int nLine)
        {
            var phoneNumber = GetPhoneNumberFromUri(_sipClient.URLGetAOR(sFromUri));
            if (nLine < _maxLineNumber)
            {
                SipLines[nLine].State = "Hold";
                SipLines[nLine].Uri = sFromUri;
                SipLines[nLine].Phone = phoneNumber;
            }
        }

        private void SipClientOnConnectingLine(string sFromUri, string sLocalUri, int nLine)
        {
            var phoneNumber = GetPhoneNumberFromUri(_sipClient.URLGetAOR(sFromUri));
            if (nLine < _maxLineNumber)
            {
                SipLines[nLine].State = "Incoming";
                SipLines[nLine].Uri = sFromUri;
                SipLines[nLine].Phone = phoneNumber;
                SelectedLine = SipLines.FirstOrDefault(s => s.Id == nLine);
            }
            CallFromServiceCompany = _requestService?.ServiceCompanyByIncommingPhoneNumber(phoneNumber);
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

        private void SipClientOnRegistrationFailure(string sLocalUri, int nCause)
        {
            SipState = $"Ошибка регистрации! {nCause}";
            _canRegistration = true;
            OnPropertyChanged(nameof(EnableRegistration));
        }

        private void SipClientOnTerminatedLine(string sFromURI, string sLocalURI, int nStatusCode, string sStatusText, int nLine)
        {
            var phoneNumber = GetPhoneNumberFromUri(_sipClient.URLGetAOR(sFromURI));
            if (nLine < _maxLineNumber)
            {
                SipLines[nLine].State = "Free";
                SipLines[nLine].Uri = sFromURI;
                SipLines[nLine].Phone = phoneNumber;
            }

            SipState = $"Звонок завершен {phoneNumber}";
            _ringPlayer.Stop();
            IsMuted = false;
            _sipCallActive = false;
        }

        private void SipClientOnUnregistration(string sLocalURI)
        {
            SipState = "UnRegistered!";
            _canRegistration = true;
            _ringPlayer.Stop();
            OnPropertyChanged(nameof(EnableRegistration));
        }

        private void SipClientOnRegistrationSuccess(string slocaluri)
        {
            foreach (SipLine line in _sipLines)
            {
                line.State = "Free";
                line.Phone = "";
                line.Uri = "";
            }


            SipState = "Успешная регистрация!";
            _canRegistration = false;
            _sipCallActive = false;
            OnPropertyChanged(nameof(EnableRegistration));
        }

        private void SipClientOnConnected(string sFromURI, string sLocalURI, int nLine)
        {
            var phoneNumber = GetPhoneNumberFromUri(_sipClient.URLGetAOR(sFromURI));
            if (nLine < _maxLineNumber)
            {
                SipLines[nLine].State = "Connect";
                SipLines[nLine].Uri = sFromURI;
                SipLines[nLine].Phone = phoneNumber;
            }
            _ringPlayer.Stop();
            _sipCallActive = true;
            SipState = $"Связь установлена: {phoneNumber}";
        }

        public void RefreshList()
        {
            CallsList = new ObservableCollection<CallsListDto>(_requestService.GetCallList(FromDate, ToDate, RequestNum, SelectedUser?.Id, SelectedCompany?.Id,PhoneNumber));
            CallsCount = CallsList.Count;
        }

        private void RefreshTimerOnTick(object sender, EventArgs eventArgs)
        {
            try
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
            catch
            {
                // ignored
            }
        }

        private void RefreshAlertRequest()
        {
            var alertedRequests = _requestService.GetAlertedRequests();
            if (alertedRequests.Count > 0 && !_sipCallActive)
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
            _requestService.SendAlive();
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
            if (_sipClient.CallState[SelectedLine.Id] == CallState.CallState_Inbound)
            {
                LastAnsweredPhoneNumber = IncomingCallFrom;
                _sipClient.PhoneLine = SelectedLine.Id;
                _sipClient.AcceptCall();
                return;
            }
            if (string.IsNullOrEmpty(_sipPhone))
                return;
            if (_sipClient.CallState[SelectedLine.Id] == CallState.CallState_Free)
            {
                string callId = string.Format("sip:{0}@{1}", _sipPhone, _serverIP);
                SipState = $"Исходящий вызов на номер {_sipPhone}";
                _sipClient.PhoneLine = SelectedLine.Id;
                _sipClient.Connect(callId);
            }
            else
            {
                MessageBox.Show("Линия занята, выберите другую линию!",
                    "Предупреждение");
            }
        }

        public void CallFromList()
        {
            if (SelectedCall == null)
                return;
            SipPhone = SelectedCall.CallerId;
            string callId = string.Format("sip:{0}@{1}", SelectedCall.CallerId, _serverIP);
            _sipClient.Connect(callId);
        }

        public void Mute()
        {
            IsMuted = !IsMuted;
            _sipClient.MicrophoneMuted = IsMuted;
        }

        private void Conference()
        {
            foreach (var sipLine in SipLines)
            {
                _sipClient.PhoneLine = sipLine.Id;
                _sipClient.ConferenceJoin();
            }
        }

        public void Transfer()
        {
            var phoneList = _requestService.GetTransferList();
            phoneList.Remove(phoneList.FirstOrDefault(p=>p.SipNumber==_sipUser));
            var transferContext = new TrasferDialogViewModel(phoneList);
            var transfer = new TransferDialog(transferContext);
            transfer.Owner = Application.Current.MainWindow;
            if (transfer.ShowDialog() == true)
            {
                var phone = string.IsNullOrEmpty(transferContext.TransferPhone)
                    ? transferContext.ClientPhone.SipNumber
                    : transferContext.TransferPhone;
                string callId = string.Format("sip:{0}@{1}", phone, _serverIP);
                _sipClient.TransferCall(callId);
            }

        }
        public void Hold()
        {
            var callState = _sipClient.get_CallState(SelectedLine.Id);
            _sipClient.PhoneLine = SelectedLine.Id;
            if ((callState == CallState.CallState_LocalHeld) || (callState == CallState.CallState_RemoteHeld))
            {
                _sipClient.Unhold();
            }
            else
            {
                _sipClient.Hold();
            }
        }
        public void HangUp()
        {
            _sipClient.PhoneLine = SelectedLine.Id;
            _sipClient.Disconnect();
            IncomingCallFrom = "";
            CallFromServiceCompany = null;
        }
        private void Bridge()
        {
        }


        public void Unregister()
        {
            using (var cmd = new MySqlCommand($"call CallCenter.LogoutUser({AppSettings.CurrentUser.Id})", AppSettings.DbConnection))
            {
                cmd.ExecuteNonQuery();
            }
            _sipClient?.UnRegister();
        }



        private static string GetPhoneNumberFromUri(string remoteUri)
        {
            var phoneNumber = string.Empty;
            var lines = remoteUri.Split(':', '@');
            if (lines.Length > 1)
            {
                phoneNumber = lines[0].Trim();
            }
            return phoneNumber;
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

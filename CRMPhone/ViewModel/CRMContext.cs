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
using CRMPhone.Dto;
using MySql.Data.MySqlClient;

namespace CRMPhone.ViewModel
{
    public class CRMContext : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _refreshTimer;
        private MySqlConnection _dbRefreshConnection;
        private UserAgent _sipAgent;

        private string _incommingCallPhone;
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
        public Window mainWindow { get; set; }

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
            CallsList = new ObservableCollection<CallsListDto>();
            var uri = new Uri(@"pack://application:,,,/Resources/ringin.wav");
            _ringPlayer = new SoundPlayer(Application.GetResourceStream(uri).Stream);
            FromDate = DateTime.Now.Date;
            ToDate = FromDate.AddDays(1);
            EnablePhone = false;
            _requestDataContext = new RequestControlContext();
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

        private ICommand _holdCommand;
        public ICommand HoldCommand { get {return _holdCommand ?? (_holdCommand = new CommandHandler(Hold, _canExecute));}}

        private ICommand _unholdCommand;
        public ICommand UnholdCommand { get {return _unholdCommand ?? (_unholdCommand = new CommandHandler(UnHold, _canExecute));}}

        private ICommand _hangUpCommand;
        public ICommand HangUpCommand { get {return _hangUpCommand ?? (_hangUpCommand = new CommandHandler(HangUpAll, _canExecute));}}

        private ICommand _transferCommand;
        public ICommand TransferCommand { get { return _transferCommand ?? (_transferCommand = new CommandHandler(Transfer, _canExecute)); } }

        private ICommand _muteCommand;
        public ICommand MuteCommand { get { return _muteCommand ?? (_muteCommand = new CommandHandler(Mute, _canExecute)); } }

        private ICommand _deleteNumberFromListCommand;
        public ICommand DeleteNumberFromListCommand { get { return _deleteNumberFromListCommand ?? (_deleteNumberFromListCommand = new CommandHandler(DeleteNumberFromList, _canExecute)); } }

        private ICommand _refreshCommand;
        private DateTime _fromDate;
        private DateTime _toDate;
        private int _callsCount;
        private Brush _muteButtonBackground;
        private bool _isMuted;
        private string _appTitle;
        private bool _enablePhone;
        private RequestControlContext _requestDataContext;
        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new CommandHandler(RefreshList, _canExecute)); } }

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

        public RequestControlContext RequestDataContext
        {
            get { return _requestDataContext; }
            set { _requestDataContext = value; OnPropertyChanged(nameof(RequestDataContext)); }
        }

        private void InitMySql()
        {
            var connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", _serverIP, "asterisk", "mysqlasterisk", "asterisk");
            _dbRefreshConnection = new MySqlConnection(connectionString);
            try
            {
                _dbRefreshConnection.Open();
                if(EnablePhone)
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
            using (
                var cmd =
                    new MySqlCommand(@"SELECT UniqueId,CallDirection,CallerIDNum,CreateTime,AnswerTime,EndTime,BridgedTime,MonitorFile,TalkTime,WaitingTime FROM asterisk.CallsHistory C
 where CreateTime between @fromdate and @todate order by UniqueId", AppSettings.DbConnection))
            {
                cmd.Parameters.AddWithValue("@fromdate", FromDate);
                cmd.Parameters.AddWithValue("@todate", ToDate);
                using (var dataReader = cmd.ExecuteReader())
                {
                    CallsList.Clear();
                    while (dataReader.Read())
                    {
                        CallsList.Add(new CallsListDto
                        {
                            UniqueId = dataReader.GetNullableString("UniqueID"),
                            CallerId = dataReader.GetNullableString("CallerIDNum"),
                            Direction = dataReader.GetNullableString("CallDirection"),
                            AnswerTime = dataReader.GetNullableDateTime("AnswerTime"),
                            CreateTime = dataReader.GetNullableDateTime("CreateTime"),
                            BridgedTime = dataReader.GetNullableDateTime("BridgedTime"),
                            EndTime = dataReader.GetNullableDateTime("EndTime"),
                            TalkTime = dataReader.GetNullableInt("TalkTime"),
                            WaitingTime = dataReader.GetNullableInt("WaitingTime"),
                            MonitorFileName = dataReader.GetNullableString("MonitorFile"),
                        });
                    }
                    dataReader.Close();
                    CallsCount = CallsList.Count;
                }
            }
        }


        private void RefreshTimerOnTick(object sender, EventArgs eventArgs)
        {
            RefreshActiveChannels();
            RefreshNotAnsweredCalls();
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
            var t = _sipAgent.CallMaker.callStatus[0];
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
                IncomingCallFrom = _incommingCallPhone;

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

            SipState = $"Входящий вызов от {phoneNumber}";
            var lines = remoteUri.Split(':', '@');
            if (lines.Length > 1)
            {
                 _incommingCallPhone = lines[1].Trim();
            }
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

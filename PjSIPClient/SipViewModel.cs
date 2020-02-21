using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using Newtonsoft.Json;
using PjSIPClient.Annotations;
using PJSip.Interop;

namespace PjSIPClient
{
    public class SipViewModel : INotifyPropertyChanged,IDisposable
    {
        private Endpoint _endpoint;
        private MyCall _call;
        private MyAccount _acc;
        //private string _sipIp = "sipnet.ru";
        private string _sipIp = "192.168.0.130";

        public string Messages
        {
            get { return _messages; }
            set { _messages = value; OnPropertyChanged(nameof(Messages));}
        }

        public void Init()
        {
            //var sipUser = "zerg282";
            //var sipSecret = "sipnetzerg";
            var sipUser = "199";
            var sipSecret = "call199";
            Messages += $"---------------------{DateTime.Now.ToString("T")}---------------------\r";
            Messages += "Init Sip!\r";
            _endpoint = new Endpoint();
            _endpoint.libCreate();
            // Initialize endpoint
            var epConfig = new EpConfig();
            //epConfig.logConfig.consoleLevel = 4;
            _endpoint.libInit(epConfig);
            // Create SIP transport. Error handling sample is shown
            var sipTpConfig = new TransportConfig();
            sipTpConfig.port = 5060;
            //sipTpConfig.portRange = 10;
            _endpoint.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_UDP, sipTpConfig);
            // Start the library
            _endpoint.libStart();

            var acfg = new AccountConfig();
            acfg.idUri = $"sip:{sipUser}@{_sipIp}";
            acfg.regConfig.registrarUri = $"sip:{_sipIp}";
            var cred = new AuthCredInfo("DispexPhone", "*", sipUser, 0, sipSecret);
            acfg.sipConfig.authCreds.Add(cred);
            // Create the account
            _acc = new MyAccount();
            _acc.OnAccountRegState += OnAccountRegState;
            //_acc.onRegStarted(new OnRegStartedParam());
            _acc.create(acfg);
            

            var t = _endpoint.audDevManager().getDevCount();
            _endpoint.audDevManager().setPlaybackDev(0);
            _endpoint.audDevManager().setCaptureDev(1);
            _call = new MyCall(_acc);
            _call.OnCallState += OnCallState;

        }

        private void OnCallState(object sender, CallInfo info, OnCallStateParam prm, MyAccount account)
        {
            Messages += $"---------------------{DateTime.Now.ToString("T")}---------------------\r";
            Messages += "CallStateParam:\r" + JsonConvert.SerializeObject(prm) + "\r";
            Messages += "CallInfo:\r" + JsonConvert.SerializeObject(info) + "\r";
            var call = (MyCall) sender;
            CallInfo ci = call.getInfo();
            if (ci.state == pjsip_inv_state.PJSIP_INV_STATE_DISCONNECTED)
            {
                account.Calls.Remove(call);
                /* Delete the call */
                call.Dispose();
            }
        }

        private void OnAccountRegState(object sender, AccountInfo ai, OnRegStateParam prm)
        {
            Messages += $"---------------------{DateTime.Now.ToString("T")}---------------------\r";
            Messages += "prm:\r" + JsonConvert.SerializeObject(prm)+"\r";
            Messages += "ai:\r" + JsonConvert.SerializeObject(ai) + "\r";
        }

        public void Call(string phone)
        {
            var prm = new CallOpParam(true);
            //prm.opt.audioCount = 1;
            //prm.opt.videoCount = 0;
            _call.makeCall($"sip:{phone}@{_sipIp}", prm);
        }

        public void Transfer(string phone)
        {
            var prm = new CallOpParam(true);
            _call.xfer($"sip:{phone}@{_sipIp}", prm);
        }
        public void HangUp()
        {
            _endpoint.hangupAllCalls();
        }
        private string _messages;

        private ICommand _initCommand;
        public ICommand InitCommand { get { return _initCommand ?? (_initCommand = new CommandHandler(Init, true)); } }
        private ICommand _hangUpCommand;
        public ICommand HangUpCommand { get { return _hangUpCommand ?? (_hangUpCommand = new CommandHandler(HangUp, true)); } }
        private ICommand _callCommand;
        public ICommand CallCommand { get { return _callCommand ?? (_callCommand = new CommandHandler(CallToZerg, true)); } }
        private ICommand _transferCommand;
        public ICommand TransferCommand { get { return _transferCommand ?? (_transferCommand = new CommandHandler(TransferToZerg, true)); } }
        private ICommand _getCallInfoCommand;
        public ICommand CallInfoCommand { get { return _getCallInfoCommand ?? (_getCallInfoCommand = new CommandHandler(GetInfoLine0, true)); } }
        private ICommand _answerCommand;
        public ICommand AnswerCommand { get { return _answerCommand ?? (_answerCommand = new CommandHandler(AnswerLine0, true)); } }

        private void AnswerLine0()
        {
            var info = _acc.Calls[0].getInfo();
            if (info.state == pjsip_inv_state.PJSIP_INV_STATE_INCOMING)
            {
                var prm = new CallOpParam {statusCode = (pjsip_status_code) 200};
                _acc.Calls[0].answer(prm);
            }
        }

        public void GetInfoLine0()
        {
            CallInfo ci = PJSip.Interop.Call.lookup(0)?.getInfo();
            Messages += $"---------------------{DateTime.Now.ToString("T")}---------------------\r";
            Messages += "CallInfo:\r" + JsonConvert.SerializeObject(ci) + "\r";
        }


        private void CallToZerg()
        {
            Call("12345");
        }
        private void TransferToZerg()
        {
            Transfer("9323232177");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _call?.Dispose();
            _acc?.Dispose();

            if (_endpoint != null)
            {
                _endpoint.libDestroy();
                _endpoint.Dispose();
            }
        }
    }
}
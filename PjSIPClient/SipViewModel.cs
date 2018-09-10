using System;
using System.Windows.Input;
using PJSip.Interop;

namespace PjSIPClient
{
    public class SipViewModel
    {
        private Endpoint _endpoint;
        private MyCall _call;
        private string _sipIp = "192.168.1.130";

        public void Init()
        {
            var sipUser = "199";
            var sipSecret = "call199";
            
            _endpoint = new Endpoint();
            _endpoint.libCreate();
            // Initialize endpoint
            var epConfig = new EpConfig();
            _endpoint.libInit(epConfig);
            // Create SIP transport. Error handling sample is shown
            var sipTpConfig = new TransportConfig();
            sipTpConfig.port = 5060;
            _endpoint.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_UDP, sipTpConfig);
            // Start the library
            _endpoint.libStart();

            var acfg = new AccountConfig();
            acfg.idUri = $"sip:{sipUser}@{_sipIp}";
            acfg.regConfig.registrarUri = $"sip:{_sipIp}";
            var cred = new AuthCredInfo("DispexPhone", "*", sipUser, 0, sipSecret);
            acfg.sipConfig.authCreds.Add(cred);
            // Create the account
            var acc = new MyAccount();
            //acc.onRegStarted(new OnRegStartedParam());
            acc.create(acfg);

            var t = _endpoint.audDevManager().getDevCount();
            _endpoint.audDevManager().setPlaybackDev(0);
            _endpoint.audDevManager().setCaptureDev(1);
            _call = new MyCall(acc);

        }

        public void Call(string phone)
        {
            var prm = new CallOpParam(true);
            //prm.opt.audioCount = 1;
            //prm.opt.videoCount = 0;
            _call.makeCall($"sip:{phone}@{_sipIp}", prm);
        }

        public void HangUp()
        {
            _endpoint.hangupAllCalls();
        }

        private ICommand _initCommand;
        public ICommand InitCommand { get { return _initCommand ?? (_initCommand = new CommandHandler(Init, true)); } }
        private ICommand _hangUpCommand;
        public ICommand HangUpCommand { get { return _hangUpCommand ?? (_hangUpCommand = new CommandHandler(HangUp, true)); } }
        private ICommand _callCommand;
        public ICommand CallCommand { get { return _callCommand ?? (_callCommand = new CommandHandler(CallToZerg, true)); } }

        private void CallToZerg()
        {
            Call("89323232177");
        }
    }
}
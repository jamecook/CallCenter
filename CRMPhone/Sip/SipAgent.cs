using System;
using System.Linq;
using Newtonsoft.Json;

namespace CRMPhone.Sip
{
    public class SipAgent : IDisposable
    {
        private Endpoint _endpoint;
        private SipAccount _acc;
        private string _sipIp;
        public delegate void AccountRegState(object sender, AccountInfo ai, OnRegStateParam prm);
        public AccountRegState OnRegisterState;

        public delegate void CallState(object sender, CallInfo info, OnCallStateParam prm, SipAccount account);
        public CallState OnCallState;
        public delegate void IncomingCall(object sender, CallInfo info, OnIncomingCallParam iprm);
        public IncomingCall OnIncomingCall;


        public void Init(string sipUser,string sipSecret, string sipAddress)
        {
            _sipIp = sipAddress;
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
            _acc = new SipAccount(_sipIp,2);
            _acc.OnAccountRegState += OnAccountRegState;
            _acc.OnCallState += OnLineCallState;
            _acc.OnIncomingCall += OnLineIncomingCall;
            //_acc.onRegStarted(new OnRegStartedParam());
            _acc.create(acfg);
            

            var t = _endpoint.audDevManager().getDevCount();
            _endpoint.audDevManager().setPlaybackDev(-1);
            _endpoint.audDevManager().setCaptureDev(-1);
        }

        private void OnLineIncomingCall(object sender, CallInfo info, OnIncomingCallParam iprm)
        {
            OnIncomingCall?.Invoke(sender, info, iprm);
        }


        private void OnLineCallState(object sender, CallInfo info, OnCallStateParam prm, SipAccount account)
        {
            OnCallState?.Invoke(sender,info,prm,account);
            var t = $"---------------------{DateTime.Now.ToString("T")}---------------------\r" +
                        $"OnCallState CallInfo: {info.callIdString} - {info.stateText}. Code: {info.state}\r";
            //Messages += "CallStateParam:\r" + JsonConvert.SerializeObject(prm) + "\r";
            //Messages += "CallInfo:\r" + JsonConvert.SerializeObject(info) + "\r";

        }

        private void OnAccountRegState(object sender, AccountInfo ai, OnRegStateParam prm)
        {
            OnRegisterState?.Invoke(sender,ai,prm);
            //Messages += $"---------------------{DateTime.Now.ToString("T")}---------------------\r";
            //Messages += "prm:\r" + JsonConvert.SerializeObject(prm)+"\r";
            //Messages += "ai:\r" + JsonConvert.SerializeObject(ai) + "\r";
        }

        public void Call(string phone)
        {
            _acc.Call(phone);
        }

        public void Transfer(string callId, string phone)
        {
            foreach (var call in _acc.Calls)
            {
                var callInfo = call.getInfo();
                if (callInfo.callIdString == callId)
                {
                    var prm = new CallOpParam(true);
                    call.xfer($"sip:{phone}@{_sipIp}", prm);
                    break;
                }
            }

        }
        public void HangUpAll()
        {
            _endpoint.hangupAllCalls();
        }
        public void HangUp(string callId)
        {
            foreach (var call in _acc.Calls)
            {
                var callInfo = call.getInfo();
                if (callInfo.callIdString == callId)
                {
                    var prm = new CallOpParam { statusCode = pjsip_status_code.PJSIP_SC_BUSY_HERE };//(pjsip_status_code)200 };
                    //var prm = new CallOpParam { statusCode = pjsip_status_code.PJSIP_SC_OK };//(pjsip_status_code)200 };
                    call.hangup(prm);
                    break;
                }
            }
        }
        public void Hold(string callId)
        {
            foreach (var call in _acc.Calls)
            {
                var callInfo = call.getInfo();
                if (callInfo.callIdString == callId)
                {
                    var prm = new CallOpParam { options = (uint) pjsua_call_flag.PJSUA_CALL_UPDATE_CONTACT};
                    call.setHold(prm);
                    break;
                }
            }
        }
        public void UnHold(string callId)
        {
            foreach (var call in _acc.Calls)
            {
                var callInfo = call.getInfo();
                if (callInfo.callIdString == callId)
                {
                    var prm = new CallOpParam();// { options = (uint)pjsua_call_flag.PJSUA_CALL_UNHOLD };
                    prm.opt = new CallSetting(){flag = (uint)pjsua_call_flag.PJSUA_CALL_UNHOLD };
                    call.reinvite(prm);
                    break;
                }
            }
        }
        public bool AnswerLine0()
        {
            if (_acc.Calls.Count == 0)
                return false;
            var info = _acc.Calls[0].getInfo();
            if (info.state == pjsip_inv_state.PJSIP_INV_STATE_INCOMING)
            {
                var prm = new CallOpParam {statusCode = (pjsip_status_code) 200};
                _acc.Calls[0].answer(prm);
                return true;
            }

            return false;
        }

        public void GetInfoLine0()
        {
            //CallInfo ci = PJSip.Interop.Call.lookup(0)?.getInfo();
            //Messages += $"---------------------{DateTime.Now.ToString("T")}---------------------\r";
            //Messages += "CallInfo:\r" + JsonConvert.SerializeObject(ci) + "\r";
        }

        public void Mute(bool isMuted)
        {
            var aud_mgr = Endpoint.instance().audDevManager();
            var captureDev = aud_mgr.getCaptureDevMedia();
            captureDev.adjustTxLevel(isMuted?0:1);
        }

        public void Dispose()
        {
            _acc?.Dispose();

            if (_endpoint != null)
            {
                _endpoint.libDestroy();
                _endpoint.Dispose();
            }
        }
    }
}
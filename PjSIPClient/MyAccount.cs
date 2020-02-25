using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PJSip.Interop;

namespace PjSIPClient
{
    public class MyAccount : Account
    {
        private List<MyCall> _calls;
        private string _sipIp;
        public delegate void AccountRegState(object sender, AccountInfo ai, OnRegStateParam prm);
        public AccountRegState OnAccountRegState;

        public delegate void CallState(object sender, CallInfo info, OnCallStateParam prm, MyAccount account);
        public CallState OnCallState;
        public delegate void IncomingCall(object sender, CallInfo info, OnIncomingCallParam iprm);
        public IncomingCall OnIncomingCall;
        public List<MyCall> Calls => _calls;
        public int LineNumber { get; }
        public MyAccount(string sipIp, int lineNumber) : base()
        {
            _calls = new List<MyCall>();
            _sipIp = sipIp;
            LineNumber = lineNumber;
        }
        public override void onRegState(OnRegStateParam prm)
        {

            var ai = getInfo();
            //Console.WriteLine("prm:");
            //Console.WriteLine(JsonConvert.SerializeObject(prm));
            //Console.WriteLine("getInfo:");
            //Console.WriteLine(JsonConvert.SerializeObject(ai));
            OnAccountRegState?.Invoke(this, ai, prm);
        }

        public void Call(string phone)
        {
            if (Calls.Count < LineNumber)
            {
                var call = new MyCall(this);
                _calls.Add(call);
                call.OnCallState += OnLineCallState;
                var prm = new CallOpParam(true);
                //prm.opt.audioCount = 1;
                //prm.opt.videoCount = 0;
                call.makeCall($"sip:{phone}@{_sipIp}", prm);
            }
        }

        public override void onIncomingCall(OnIncomingCallParam iprm)
        {
            var call = new MyCall(this, iprm.callId);
            call.OnCallState += OnLineCallState;

            _calls.Add(call);
            CallInfo ci = call.getInfo();

            OnIncomingCall?.Invoke(call, ci, iprm);
            //var prm = new CallOpParam {statusCode = (pjsip_status_code) 200};
            //call.answer(prm);

            //std::cout << "*** Incoming Call: " << ci.remoteUri << " ["
            //          << ci.stateText << "]" << std::endl;

            //calls.push_back(call);
        }

        private void OnLineCallState(object sender, CallInfo info, OnCallStateParam prm, MyAccount account)
        {
            var call = (MyCall)sender;
            CallInfo ci = call.getInfo();
            OnCallState?.Invoke(sender, info, prm, account);
            if (ci.state == pjsip_inv_state.PJSIP_INV_STATE_DISCONNECTED)
            {
                account.Calls.Remove(call);
                /* Delete the call */
                call.Dispose();
            }

        }
    }
}
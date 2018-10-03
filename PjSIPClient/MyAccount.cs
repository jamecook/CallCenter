using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PJSip.Interop;

namespace PjSIPClient
{
    public class MyAccount : Account
    {
        private List<MyCall> _calls;
        public delegate void AccountRegState(object sender, AccountInfo ai, OnRegStateParam prm);
        public AccountRegState OnAccountRegState;
        public delegate void IncomingCall(object sender, AccountInfo ai, OnRegStateParam prm);
        public IncomingCall OnIncomingCall;
        public List<MyCall> Calls => _calls;
        public MyAccount() : base()
        {
            _calls = new List<MyCall>();
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

        public override void onIncomingCall(OnIncomingCallParam iprm)
        {
            var call = new MyCall(this, iprm.callId);
            call.OnCallState += OnCallState;

            _calls.Add(call);
            CallInfo ci = call.getInfo();
            //var prm = new CallOpParam {statusCode = (pjsip_status_code) 200};
            //call.answer(prm);

            //std::cout << "*** Incoming Call: " << ci.remoteUri << " ["
            //          << ci.stateText << "]" << std::endl;

            //calls.push_back(call);
        }

        private void OnCallState(object sender, CallInfo info, OnCallStateParam prm, MyAccount account)
        {
            var call = (MyCall)sender;
            CallInfo ci = call.getInfo();
            if (ci.state == pjsip_inv_state.PJSIP_INV_STATE_DISCONNECTED)
            {
                account.Calls.Remove(call);
                /* Delete the call */
                call.Dispose();
            }

        }
    }
}
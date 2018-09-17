using System;
using Newtonsoft.Json;
using PJSip.Interop;

namespace PjSIPClient
{
    public class MyAccount : Account
    {
        public delegate void AccountRegState(object sender, AccountInfo ai, OnRegStateParam prm);
        public AccountRegState OnAccountRegState;
        public delegate void IncomingCall(object sender, AccountInfo ai, OnRegStateParam prm);
        public IncomingCall OnIncomingCall;
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
            CallInfo ci = call.getInfo();
            var prm = new CallOpParam {statusCode = (pjsip_status_code) 200};

            //std::cout << "*** Incoming Call: " << ci.remoteUri << " ["
            //          << ci.stateText << "]" << std::endl;

            //calls.push_back(call);
            call.answer(prm);
        }
    }
}
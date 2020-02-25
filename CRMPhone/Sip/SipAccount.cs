using System.Collections.Generic;

namespace CRMPhone.Sip
{
    public class SipAccount : Account
    {
        private List<SipCall> _calls;
        private string _sipIp;
        public delegate void AccountRegState(object sender, AccountInfo ai, OnRegStateParam prm);
        public AccountRegState OnAccountRegState;
        public delegate void CallState(object sender, CallInfo info, OnCallStateParam prm, SipAccount account);
        public CallState OnCallState;
        public delegate void IncomingCall(object sender, CallInfo info, OnIncomingCallParam iprm);
        public IncomingCall OnIncomingCall;
        public List<SipCall> Calls => _calls;
        public int LineNumber { get; }

        public SipAccount(string sipIp, int lineNumber) : base()
        {
            _calls = new List<SipCall>();
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
                var call = new SipCall(this);
                _calls.Add(call);
                call.OnCallState += OnLineCallState;
                var prm = new CallOpParam(true);
                call.makeCall($"sip:{phone}@{_sipIp}", prm);
            }
        }
        public override void onIncomingCall(OnIncomingCallParam iprm)
        {
            var call = new SipCall(this, iprm.callId);
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

        private void OnLineCallState(object sender, CallInfo info, OnCallStateParam prm, SipAccount account)
        {
            var call = (SipCall)sender;
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
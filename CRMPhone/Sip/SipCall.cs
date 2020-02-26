
namespace CRMPhone.Sip
{
    public class SipCall : Call
    {
        public delegate void CallState(object sender, CallInfo info, OnCallStateParam prm, SipAccount account);

        public CallState OnCallState;
        private SipAccount _account;

        public SipCall(Account acc, int callId) : base(acc, callId)
        {
            var t = callId;
            _account = (SipAccount) acc;
        }

        public SipCall(Account acc) : base(acc)
        {
            _account = (SipAccount) acc;
        }

        public override void onCallState(OnCallStateParam prm)
        {
            var info = getInfo();
            OnCallState?.Invoke(this, info, prm, _account);
        }

        public override void onCallMediaState(OnCallMediaStateParam prm)
        {
            var info = getInfo();

            for (uint i = 0; i < info.media.Count; i++)
            {
                var med_info = info.media[(int) i];
                if ((med_info.type == pjmedia_type.PJMEDIA_TYPE_AUDIO) &&
                    ((med_info.status == pjsua_call_media_status.PJSUA_CALL_MEDIA_ACTIVE) ||
                     (med_info.status == pjsua_call_media_status.PJSUA_CALL_MEDIA_REMOTE_HOLD))
                )
                {
                    var m = getMedia(i);
                    var am = AudioMedia.typecastFromMedia(m);
                    var aud_mgr = Endpoint.instance().audDevManager();

                    aud_mgr.getCaptureDevMedia().startTransmit(am);
                    am.startTransmit(aud_mgr.getPlaybackDevMedia());
                }
            }
        }
    }
}

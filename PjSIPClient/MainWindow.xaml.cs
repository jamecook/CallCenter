using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using PJSip.Interop;

namespace PjSIPClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Endpoint ep;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new SipViewModel();
            //Sip();

        }

        public void Sip()
        {
            var sipUser = "199";
            var sipSecret = "call199";
            var sipIp = "192.168.1.130";
            try
            {
                // Create endpoint
                ep = new Endpoint();
                ep.libCreate();
                // Initialize endpoint
                var epConfig = new EpConfig();
                ep.libInit(epConfig);
                // Create SIP transport. Error handling sample is shown
                var sipTpConfig = new TransportConfig();
                sipTpConfig.port = 5060;
                ep.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_UDP, sipTpConfig);
                // Start the library
                ep.libStart();

                var acfg = new AccountConfig();
                acfg.idUri = $"sip:{sipUser}@{sipIp}";
                acfg.regConfig.registrarUri = $"sip:{sipIp}";
                var cred = new AuthCredInfo("DispexPhone", "*", sipUser, 0, sipSecret);
                acfg.sipConfig.authCreds.Add(cred);
                // Create the account
                var acc = new MyAccount();
                //acc.onRegStarted(new OnRegStartedParam());
                acc.create(acfg);

                var t = ep.audDevManager().getDevCount();
                ep.audDevManager().setPlaybackDev(0);
                var tt = ep.audDevManager().getPlaybackDevMedia();
                for (int i = 0; i < t; i++)
                {
                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine(JsonConvert.SerializeObject(tt));
                    Console.WriteLine("-------------------------------------");
                }
                //ep.audDevManager().setNullDev();
                /*
                // And install sound device using Extra Audio Device 
                ExtraAudioDevice auddev2(-1, -1);
                    auddev2.open();
             

                // Create player and recorder
                {
                    var amp = new AudioMediaPlayer();
                    amp.createPlayer(filename);

                    var amr = new AudioMediaRecorder();
                    amr.createRecorder("recorder_test_output.wav");

                    amp.startTransmit(amr);
                    if (auddev2.isOpened())
                        amp.startTransmit(auddev2);
*/
                    Thread.Sleep(5000);
                var call = new MyCall(acc);
                
                var prm = new CallOpParam(true);
                //prm.opt.audioCount = 1;
                //prm.opt.videoCount = 0;
                call.makeCall($"sip:89323232177@{sipIp}", prm);

                // Hangup all calls
                Thread.Sleep(15000);
                ep.hangupAllCalls();
                Thread.Sleep(5000);
                // Here we don't have anything else to do..

                /* Explicitly delete the account.
                 * This is to avoid GC to delete the endpoint first before deleting
                 * the account.
                 */
                //acc.Dispose();

                // Explicitly destroy and delete endpoint
                //ep.libDestroy();
                //ep.Dispose();

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return;
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            (DataContext as SipViewModel)?.Dispose();
        }
    }
}

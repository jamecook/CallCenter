using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using pj;

namespace Sip2Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Sip();
        }
        public void Sip()
        {
            var sipUser = "199";
            var sipSecret = "call199";
            var sipIp = "192.168.1.130";
            try
            {
                // Create endpoint
                var ep = new Endpoint();
                ep.libCreate();
                // Initialize endpoint
                var epConfig = new EpConfig();
                ep.libInit(epConfig);
                // Create SIP transport. Error handling sample is shown
                TransportConfig sipTpConfig = new TransportConfig();
                sipTpConfig.port = 5060;
                ep.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_UDP, sipTpConfig);
                // Start the library
                ep.libStart();

                AccountConfig acfg = new AccountConfig();
                acfg.idUri = $"sip:{sipUser}@{sipIp}";
                acfg.regConfig.registrarUri = $"sip:{sipIp}";
                AuthCredInfo cred = new AuthCredInfo("DispexPhone", "*", sipUser, 0, sipSecret);
                acfg.sipConfig.authCreds.Add(cred);
                // Create the account
                var acc = new MyAccount();
                //acc.onRegStarted(new OnRegStartedParam());
                acc.create(acfg);

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
    }

    public class MyAccount : Account
    {
        public virtual void onRegState(OnRegStateParam prm)
        {
            AccountInfo ai = getInfo();
            var tt = prm;
            Console.WriteLine("prm:");
            Console.WriteLine(JsonConvert.SerializeObject(prm));
            Console.WriteLine("getInfo:");
            Console.WriteLine(JsonConvert.SerializeObject(ai));
        }
    }
}

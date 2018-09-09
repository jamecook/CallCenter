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
using PJSip.Interop;

namespace PjSIPClient
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
            try
            {
                // Create endpoint
                Endpoint ep = new Endpoint();
                ep.libCreate();
                // Initialize endpoint
                EpConfig epConfig = new EpConfig();
                ep.libInit(epConfig);
                // Create SIP transport. Error handling sample is shown
                TransportConfig sipTpConfig = new TransportConfig();
                sipTpConfig.port = 5060;
                ep.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_UDP, sipTpConfig);
                // Start the library
                ep.libStart();

                AccountConfig acfg = new AccountConfig();
                acfg.idUri = "sip:test@pjsip.org";
                acfg.regConfig.registrarUri = "sip:pjsip.org";
                AuthCredInfo cred = new AuthCredInfo("digest", "*", "test", 0, "secret");
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
        }
    }
}

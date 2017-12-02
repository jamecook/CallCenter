using System;
using System.Resources;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CRMPhone.ViewModel;
using RequestServiceImpl;

namespace CRMPhone
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notify;
        public MainWindow()
        {
            InitializeComponent();
            _notify = new NotifyIcon();
            _notify.Icon = new System.Drawing.Icon("PhoneIco.ico");
            _notify.Visible = true;

            notAnsweredListBox.Items.SortDescriptions.Add(
                new System.ComponentModel.SortDescription("CreateTime",
                System.ComponentModel.ListSortDirection.Ascending));
            var mainContext = new CRMContext();
            DataContext = mainContext;
            ((CRMContext) DataContext).mainWindow = this;
            var login = new LoginView();
            var loginModel = new LoginContext(mainContext.serverIp);
            login.DataContext = loginModel;
            loginModel.View = login;
            var t = login.ShowDialog();
            if (t != true)
            {
                Environment.Exit(0);
            }
            mainContext.SipUser = AppSettings.SipInfo?.SipUser;
            mainContext.SipSecret = AppSettings.SipInfo?.SipSecret;
            mainContext.InitMysqlAndSip();
        }

        public void ShowNotify(string message, string header)
        {
            _notify.ShowBalloonTip(1000, message, header, ToolTipIcon.None);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (DataContext as CRMContext)?.Unregister();
        }

        private void NotAnsweredListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (DataContext as CRMContext)?.CallFromList();
        }
    }
}

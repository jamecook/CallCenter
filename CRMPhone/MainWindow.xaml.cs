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
using CRMPhone.ViewModel;

namespace CRMPhone
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

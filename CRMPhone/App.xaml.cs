using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CRMPhone
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private void Application_Startup(object sender, StartupEventArgs e)
        //{
        //    var login = new LoginView();
        //    Current.MainWindow = login;
        //    var t = login.ShowDialog();
        //    var main = new MainWindow();
        //    Current.MainWindow = main;
        //    var tt = main.ShowDialog();
        //}

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}

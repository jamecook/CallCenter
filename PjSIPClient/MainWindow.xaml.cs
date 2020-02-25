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
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            (DataContext as SipViewModel)?.Dispose();
        }
    }
}

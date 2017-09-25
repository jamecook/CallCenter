using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CRMPhone.Dialogs
{
    /// <summary>
    /// Interaction logic for ServiceCompanyInfoDialog.xaml
    /// </summary>
    public partial class ServiceCompanyInfoDialog : Window
    {
        public ServiceCompanyInfoDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

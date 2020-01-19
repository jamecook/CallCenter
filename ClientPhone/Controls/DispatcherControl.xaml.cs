using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RequestServiceImpl.Dto;

namespace CRMPhone.Controls
{
    /// <summary>
    /// Логика взаимодействия для RequestControl.xaml
    /// </summary>
    public partial class DispatcherControl : UserControl
    {
        public DispatcherControl()
        {
            InitializeComponent();
        }

        private void DispatcherGrid_OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
        }
    }
}

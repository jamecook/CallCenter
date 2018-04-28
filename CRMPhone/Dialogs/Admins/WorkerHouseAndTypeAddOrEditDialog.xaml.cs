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
using RequestServiceImpl.Dto;

namespace CRMPhone.Dialogs.Admins
{
    /// <summary>
    /// Interaction logic for WorkerAddOrEditDialog.xaml
    /// </summary>
    public partial class WorkerHouseAndTypeAddOrEditDialog : Window
    {
        public WorkerHouseAndTypeAddOrEditDialog()
        {
            InitializeComponent();
        }

        private void CbOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            comboBox.SelectedItem = null;
        }

        private void CbOnDropDownClosed(object sender, EventArgs e)
        {
            var count = ((ComboBox)sender).ItemsSource.Cast<FieldForFilterDto>().Count(w => w.Selected);
            if (count > 1)
            {
                ((ComboBox)sender).Text = "Несколько";
            }
            else if (count == 1)
            {
                var item = ((ComboBox)sender).ItemsSource.Cast<FieldForFilterDto>().FirstOrDefault(w => w.Selected);
                ((ComboBox)sender).Text = item?.Name;
            }
            else
                ((ComboBox)sender).Text = "";

        }
    }
}

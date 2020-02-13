using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RequestServiceImpl.Dto;

namespace CRMPhone.Dialogs.Admins
{
    /// <summary>
    /// Interaction logic for AttachmentDialog.xaml
    /// </summary>
    public partial class BindWorkerToWorkerDialog : Window
    {
        public BindWorkerToWorkerDialog()
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

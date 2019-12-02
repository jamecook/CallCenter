using System.Windows.Controls;
using System.Windows.Input;
using CRMPhone.ViewModel;

namespace CRMPhone.Controls
{
    /// <summary>
    /// Логика взаимодействия для PhoneControl.xaml
    /// </summary>
    public partial class PhoneControl : UserControl
    {
        public PhoneControl()
        {
            InitializeComponent();
            notAnsweredListBox.Items.SortDescriptions.Add(
                new System.ComponentModel.SortDescription("CreateTime",
                    System.ComponentModel.ListSortDirection.Ascending));

        }
        private void NotAnsweredListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (DataContext as CRMContext)?.CallFromList();
        }
    }
}

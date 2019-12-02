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
    /// Interaction logic for AlertAndWorkDialog.xaml
    /// </summary>
    public partial class EditAddressOnRequestDialog : Window
    {
        public EditAddressOnRequestDialog()
        {
            InitializeComponent();
        }
        private void tabItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //e.Key = Key.Tab;
                var request = new TraversalRequest(FocusNavigationDirection.Right) { Wrapped = true };
                if (sender is ComboBox)
                {
                    var parentDepObj = VisualTreeHelper.GetParent(sender as DependencyObject);
                    var comboBoxes = (parentDepObj as WrapPanel).Children.OfType<ComboBox>().ToList();
                    var currentIndex = comboBoxes.IndexOf(sender as ComboBox);
                    if (currentIndex < comboBoxes.Count - 1)
                        comboBoxes[currentIndex + 1].Focus();
                    else
                    {
                        var t = (sender as FrameworkElement).PredictFocus(FocusNavigationDirection.Next);
                        (sender as FrameworkElement).MoveFocus(request);
                    }
                }
                else
                {
                    var t = (sender as FrameworkElement).PredictFocus(FocusNavigationDirection.Next);
                    (sender as FrameworkElement).MoveFocus(request);
                }
            }
        }

    }
}

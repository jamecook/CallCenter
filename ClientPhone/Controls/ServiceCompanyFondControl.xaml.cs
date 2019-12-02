using System;
using System.Activities.Expressions;
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
    public partial class ServiceCompanyFondControl : UserControl
    {
        public ServiceCompanyFondControl()
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

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
    public partial class NotifyRequestControl : UserControl
    {
        public NotifyRequestControl()
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

        private void RequestsGrid_OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
                try
                {
                    switch (((RequestForListDto)e.Row.DataContext).Status)
                    {
                    case "Закрыта":
                    case "Выполнена(обзвон)":
                        e.Row.Background = new SolidColorBrush(Colors.PaleGreen);
                        break;
                    case "Аннулирована":
                            e.Row.Background = new SolidColorBrush(Colors.DarkKhaki);
                            break;
                    case "В работе":
                            e.Row.Background = new SolidColorBrush(Colors.LightSkyBlue);
                            break;
                    default:
                        e.Row.Background = new SolidColorBrush(Colors.White);
                        break;
                }
            }
                catch
                {
                }
        }
    }
}

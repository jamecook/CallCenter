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
    public partial class AlertRequestControl : UserControl
    {
        public AlertRequestControl()
        {
            InitializeComponent();
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

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace CRMPhone.Controls
{
    /// <summary>
    /// Interaction logic for TimeControl.xaml
    /// </summary>
    public partial class TimeControl : UserControl
    {
        public DateTime SelectedTime
        {
            get { return (DateTime)GetValue(SelectedTimeProperty); }
            set { SetValue(SelectedTimeProperty, value); }
        }
        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register("SelectedTime", typeof(DateTime),
                typeof(TimeControl), new UIPropertyMetadata(DateTime.Today, DateTimeChanged));

        private static void DateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var t = 1;
        }

        public class HoursItem
        {
            public int Hour { get; set; }
            public string Name { get; set; }
        }
        public class MinutesItem
        {
            public int Minute { get; set; }
            public string Name { get; set; }
        }
        public TimeControl()
        {
            InitializeComponent();
            this.DataContext = this;
            var hours = new List<HoursItem>();
            for(int i=1;i<=23;i++)
                hours.Add(new HoursItem(){Hour = i,Name = i.ToString().PadLeft(2,'0')});
            ComboHours.ItemsSource = hours;
            var minutes = new List<MinutesItem>();
            for(int i=0;i<=55;i+=5)
                minutes.Add(new MinutesItem(){Minute = i,Name = i.ToString().PadLeft(2,'0')});
            ComboMinutes.ItemsSource = minutes;
            ComboHours.SelectedItem = hours.FirstOrDefault();
            ComboMinutes.SelectedItem = minutes.FirstOrDefault();

        }

        public DateTime SelectedTimeValue
        {
            get
            {
                var hourTime = ComboHours.SelectedItem != null ? ((HoursItem) ComboHours.SelectedItem).Name : "01";
                var minuteTime = ComboMinutes.SelectedItem != null
                    ? ((MinutesItem) ComboMinutes.SelectedItem).Name
                    : "00";
                var result = DateTime.ParseExact($"{hourTime}:{minuteTime}", "HH:mm", null);
                return result;
            }
            set
            {
              /*  DateTime? time = value;
                if (time.HasValue)
                {
                    string timeString = time.Value.ToShortTimeString();
                    //9:54 AM
                    string[] values = timeString.Split(':', ' ');
                    if (values.Length == 3)
                    {
                        this.txtHours.Text = values[0];
                        this.txtMinutes.Text = values[1];
                        this.txtAmPm.Text = values[2];
                    }
                }
*/            }
        }

        private void Combo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTime = SelectedTimeValue;
        }
    }
}

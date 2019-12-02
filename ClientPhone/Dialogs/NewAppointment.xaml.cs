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

namespace RudiGrobler.Samples.Calendar
{
    /// <summary>
    /// Interaction logic for NewAppointment.xaml
    /// </summary>
    public partial class NewAppointment : Window
    {
        public NewAppointment()
        {
            InitializeComponent();            
        }

        public void BuildTimeComboBox(DateTime start, DateTime end)
        {
            //startTimeDuration.Text = start.ToShortTimeString();



            int offset = start.Hour * 2;
            if (start.Minute == 30)
                offset++;
            startTimeDuration.SelectedIndex = offset;


        }

        private void startTimeDuration_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        public DateTime ComposeStartTime()
        {
            DateTime date = startTimePicker.SelectedDate.Value;
            int hour = startTimeDuration.SelectedIndex / 2;
            int minutes = 0;
            if ((startTimeDuration.SelectedIndex-(hour*2))==1)
                minutes = 30;
            return new DateTime(date.Year, date.Month, date.Day, hour, minutes, 0);
        }

        public DateTime ComposeEndTime()
        {
            DateTime start = ComposeStartTime();
            int minutes = (endTimeDuration.SelectedIndex+1)*30;
            return start + TimeSpan.FromMinutes(minutes);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

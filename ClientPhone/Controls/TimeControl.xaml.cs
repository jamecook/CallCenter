using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RequestServiceImpl.Dto;

namespace ClientPhone.Controls
{
    /// <summary>
    /// Interaction logic for TimeControl.xaml
    /// </summary>
    public partial class TimeControl : UserControl
    {
        public TimeControl()
        {
            HoursItems = new List<string>();
            for (int i = 0; i < 24; i++)
            {
                HoursItems.Add(string.Format("{0}", i));
            }
            MinutesItems = new List<string>();
            for (int i = 0; i < 60; i++)
            {
                MinutesItems.Add(string.Format("{0:D2}", i));
            }

            InitializeComponent();
        }

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TimeControl control = obj as TimeControl;
            var newTime = (TimeSpan)e.NewValue;

            control.SelectedHour = HoursItems.Where(s=>s == string.Format("{0}", newTime.Hours)).FirstOrDefault();
            control.SelectedMinute = MinutesItems.Where(s=>s == string.Format("{0:D2}", newTime.Minutes)).FirstOrDefault();
        }

        public static List<string> HoursItems { get; set; }
        public static List<string> MinutesItems { get; set; }



        private static void OnTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TimeControl control = obj as TimeControl;

            if (!int.TryParse(control.SelectedHour, out int hour))
            {
                hour = 0;
            }
            if (!int.TryParse(control.SelectedMinute, out int minute))
            {
                minute = 0;
            }

            control.Value = new TimeSpan(hour, minute, 0);
        }

        public TimeSpan Value
        {
            get { return (TimeSpan)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(TimeSpan), typeof(TimeControl),
        new FrameworkPropertyMetadata(DateTime.Now.TimeOfDay, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnValueChanged)));



        public string SelectedHour
        {
            get { return (string)GetValue(SelectedHourProperty); }
            set { SetValue(SelectedHourProperty, value); }
        }
        public static readonly DependencyProperty SelectedHourProperty =
        DependencyProperty.Register("SelectedHour", typeof(string), typeof(TimeControl),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTimeChanged)));

        public string SelectedMinute
        {
            get { return (string)GetValue(SelectedMinuteProperty); }
            set { SetValue(SelectedMinuteProperty, value); }
        }
        public static readonly DependencyProperty SelectedMinuteProperty =
        DependencyProperty.Register("SelectedMinute", typeof(string), typeof(TimeControl),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTimeChanged)));

    }
}

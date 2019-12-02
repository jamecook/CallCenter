using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RudiGrobler.Controls
{
    public class CalendarAppointmentItem : ButtonBase
    {
        public const string StateNormal = "Normal";
        public const string StateMouseOver = "MouseOver";
        public const string StateDisabled = "Disabled";

        public const string GroupCommon = "CommonStates";

        static CalendarAppointmentItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarAppointmentItem), new FrameworkPropertyMetadata(typeof(CalendarAppointmentItem)));
        }
        protected override void OnClick()
        {
            var tt = this;
            base.OnClick();

            RaiseEditAppointmentEvent();
        }

        #region EditAppointment

        private void RaiseEditAppointmentEvent()
        {
            RoutedEventArgs e = new RoutedEventArgs();
            e.RoutedEvent = EditAppointmentEvent;
            e.Source = this;

            OnEditAppointment(e);
        }

        public static readonly RoutedEvent EditAppointmentEvent =
            EventManager.RegisterRoutedEvent("EditAppointment", RoutingStrategy.Bubble,
            typeof(RoutedEventArgs), typeof(CalendarAppointmentItem));

        public event RoutedEventHandler EditAppointment
        {
            add
            {
                AddHandler(EditAppointmentEvent, value);
            }
            remove
            {
                RemoveHandler(EditAppointmentEvent, value);
            }
        }

        protected virtual void OnEditAppointment(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        #endregion
        #region StartTime/EndTime

        public static readonly DependencyProperty StartTimeProperty =
            TimeslotPanel.StartTimeProperty.AddOwner(typeof(CalendarAppointmentItem));

        public bool StartTime
        {
            get { return (bool)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DependencyProperty EndTimeProperty =
            TimeslotPanel.EndTimeProperty.AddOwner(typeof(CalendarAppointmentItem));

        public bool EndTime
        {
            get { return (bool)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        #endregion


    }
}

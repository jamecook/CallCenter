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

namespace RudiGrobler.Controls
{
    [TemplatePart(Name = CalendarDay.ElementTimeslotItems, Type = typeof(StackPanel))]
    public sealed class CalendarDay : ItemsControl
    {
        private const string ElementTimeslotItems = "PART_TimeslotItems";

        StackPanel _dayItems;

        static CalendarDay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarDay), new FrameworkPropertyMetadata(typeof(CalendarDay)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dayItems = GetTemplateChild(ElementTimeslotItems) as StackPanel;

            PopulateDay();
        }

        public void PopulateDay()
        {
            if (_dayItems != null)
            {
                _dayItems.Children.Clear();

                DateTime startTime = new DateTime(Owner.CurrentDate.Year, Owner.CurrentDate.Month, Owner.CurrentDate.Day, 0, 0, 0);
                for (int i = 0; i < 48; i++)
                {                   
                    CalendarTimeslotItem timeslot = new CalendarTimeslotItem();
                    timeslot.StartTime = startTime;
                    timeslot.EndTime = startTime + TimeSpan.FromMinutes(30);

                    if (startTime.Hour >= 8 && startTime.Hour <= 17)
                        timeslot.SetBinding(Calendar.BackgroundProperty, GetOwnerBinding("PeakTimeslotBackground"));
                    else
                        timeslot.SetBinding(Calendar.BackgroundProperty, GetOwnerBinding("OffPeakTimeslotBackground"));

                    timeslot.SetBinding(CalendarTimeslotItem.StyleProperty, GetOwnerBinding("CalendarTimeslotItemStyle"));
                    _dayItems.Children.Add(timeslot);

                    startTime = startTime + TimeSpan.FromMinutes(30);
                }
            }
            if (Owner != null)
            {
                Owner.ScrollToHome();
            }
        }

        #region ItemsControl Container Override

        protected override DependencyObject GetContainerForItemOverride()
        {            
            return new CalendarAppointmentItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is CalendarAppointmentItem);
        }

        #endregion

        public Calendar Owner { get; set; }

        private BindingBase GetOwnerBinding(string propertyName)
        {
            Binding result = new Binding(propertyName);
            result.Source = this.Owner;
            return result;
        }
    }
}

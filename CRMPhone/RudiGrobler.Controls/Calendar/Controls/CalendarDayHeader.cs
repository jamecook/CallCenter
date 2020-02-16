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
    [TemplatePart(Name = CalendarDayHeader.ElementDayHeaderLabel, Type = typeof(TextBlock))]
    public sealed class CalendarDayHeader : Control
    {
        private const string ElementDayHeaderLabel = "PART_DayHeaderLabel";

        static CalendarDayHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarDayHeader), new FrameworkPropertyMetadata(typeof(CalendarDayHeader)));
        }

        public Calendar Owner { get; set; }

        private BindingBase GetOwnerBinding(string propertyName)
        {
            Binding result = new Binding(propertyName);
            result.Source = this.Owner;
            return result;
        }

        TextBlock _dayHeaderLabel;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dayHeaderLabel = GetTemplateChild(ElementDayHeaderLabel) as TextBlock;

            PopulateHeader();
        }

        void PopulateHeader()
        {
            BindingBase binding = GetOwnerBinding("CurrentDateText");
            //binding.StringFormat = "{0:yyyy-MM-dd}";
            _dayHeaderLabel.SetBinding(TextBlock.TextProperty, binding);
        }
    }
}

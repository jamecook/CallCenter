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
using System.ComponentModel;

using RudiGrobler.Calendar.Common;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace RudiGrobler.Controls
{
    [TemplatePart(Name = Calendar.ElementDay, Type = typeof(CalendarDay))]
    [TemplatePart(Name = Calendar.ElementDayHeader, Type = typeof(CalendarDayHeader))]
    [TemplatePart(Name = Calendar.ElementLedger, Type = typeof(CalendarLedger))]
    [TemplatePart(Name = Calendar.ElementScrollViewer, Type = typeof(ScrollViewer))]
    public class Calendar : Control
    {
        private const string ElementDay = "PART_Day";
        private const string ElementDayHeader = "PART_DayHeader";
        private const string ElementLedger = "PART_Ledger";
        private const string ElementScrollViewer = "PART_ScrollViewer";

        static Calendar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Calendar), new FrameworkPropertyMetadata(typeof(Calendar)));

            CommandManager.RegisterClassCommandBinding(typeof(Calendar), new CommandBinding(NextDay, new ExecutedRoutedEventHandler(OnExecutedNextDay), new CanExecuteRoutedEventHandler(OnCanExecuteNextDay)));
            CommandManager.RegisterClassCommandBinding(typeof(Calendar), new CommandBinding(PreviousDay, new ExecutedRoutedEventHandler(OnExecutedPreviousDay), new CanExecuteRoutedEventHandler(OnCanExecutePreviousDay)));
        }

        #region CalendarTimeslotItemStyle

        public static readonly DependencyProperty CalendarTimeslotItemStyleProperty =
            DependencyProperty.Register("CalendarTimeslotItemStyle", typeof(Style), typeof(Calendar));

        [Category("Calendar")]
        //[Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public Style CalendarTimeslotItemStyle
        {
            get { return (Style)GetValue(CalendarTimeslotItemStyleProperty); }
            set { SetValue(CalendarTimeslotItemStyleProperty, value); }
        }

        #endregion

        #region CalendarLedgerItemStyle

        public static readonly DependencyProperty CalendarLedgerItemStyleProperty =
            DependencyProperty.Register("CalendarLedgerItemStyle", typeof(Style), typeof(Calendar));

        [Category("Calendar")]
        //[Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public Style CalendarLedgerItemStyle
        {
            get { return (Style)GetValue(CalendarLedgerItemStyleProperty); }
            set { SetValue(CalendarLedgerItemStyleProperty, value); }
        }

        #endregion

        #region CalendarAppointmentItemStyle

        public static readonly DependencyProperty CalendarAppointmentItemStyleProperty =
            DependencyProperty.Register("CalendarAppointmentItemStyle", typeof(Style), typeof(Calendar));

        [Category("Calendar")]
        //[Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public Style CalendarAppointmentItemStyle
        {
            get { return (Style)GetValue(CalendarAppointmentItemStyleProperty); }
            set { SetValue(CalendarAppointmentItemStyleProperty, value); }
        }

        #endregion

        #region CurrentDateChanged

        #endregion

        #region AddAppointment

        public static readonly RoutedEvent AddAppointmentEvent = 
            CalendarTimeslotItem.AddAppointmentEvent.AddOwner(typeof(CalendarDay));

        public event RoutedEventHandler AddAppointment
        {
            add
            {
                AddHandler(AddAppointmentEvent, value);
            }
            remove
            {
                RemoveHandler(AddAppointmentEvent, value);
            }
        }

        #endregion

        #region Appointments

        public static readonly DependencyProperty AppointmentsProperty =
            DependencyProperty.Register("Appointments", typeof(IList<Appointment>), typeof(Calendar),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(Calendar.OnAppointmentsChanged)));

        public IList<Appointment> Appointments
        {
            get { return (ObservableCollection<Appointment>)GetValue(AppointmentsProperty); }
            set { SetValue(AppointmentsProperty, value); }
        }

        private static void OnAppointmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Calendar)d).OnAppointmentsChanged(e);
        }

        protected virtual void OnAppointmentsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_day != null)
            {
                _day.PopulateDay();
            }

            INotifyCollectionChanged appointments = Appointments as INotifyCollectionChanged;
            if (appointments != null)
            {
                appointments.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Appointments_CollectionChanged);
            }
            FilterAppointments(CurrentDate);
        }

        void Appointments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FilterAppointments(CurrentDate);
        }

        #endregion        
       
        #region CurrentDate

        public static readonly DependencyProperty CurrentDateProperty =
            DependencyProperty.Register("CurrentDate", typeof(DateTime), typeof(Calendar),
                new FrameworkPropertyMetadata((DateTime)DateTime.Now, FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnCurrentDateChanged)));

        [Category("Calendar")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public DateTime CurrentDate
        {
            get { return (DateTime)GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }

        private static void OnCurrentDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Calendar)d).OnCurrentDateChanged(e);
        }

        protected virtual void OnCurrentDateChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_day != null)
            {
                _day.PopulateDay();
            }

            FilterAppointments(CurrentDate);
        }

        #endregion             

        #region PeakTimeslotBackground

        public static readonly DependencyProperty PeakTimeslotBackgroundProperty =
            DependencyProperty.Register("PeakTimeslotBackground", typeof(Brush), typeof(Calendar),
                new FrameworkPropertyMetadata((Brush)Brushes.White,
                    new PropertyChangedCallback(OnPeakTimeslotBackgroundChanged)));

        [Category("Calendar")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Brush PeakTimeslotBackground
        {
            get { return (Brush)GetValue(PeakTimeslotBackgroundProperty); }
            set { SetValue(PeakTimeslotBackgroundProperty, value); }
        }

        private static void OnPeakTimeslotBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Calendar)d).OnPeakTimeslotBackgroundChanged(e);
        }

        protected virtual void OnPeakTimeslotBackgroundChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_day != null)
            {
                _day.PopulateDay();
            }
        }

        #endregion

        #region OffPeakTimeslotBackground

        public static readonly DependencyProperty OffPeakTimeslotBackgroundProperty =
            DependencyProperty.Register("OffPeakTimeslotBackground", typeof(Brush), typeof(Calendar),
                new FrameworkPropertyMetadata((Brush)Brushes.LightCyan,
                    new PropertyChangedCallback(OnOffPeakTimeslotBackgroundChanged)));

        [Category("Calendar")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Brush OffPeakTimeslotBackground
        {
            get { return (Brush)GetValue(OffPeakTimeslotBackgroundProperty); }
            set { SetValue(OffPeakTimeslotBackgroundProperty, value); }
        }

        private static void OnOffPeakTimeslotBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Calendar)d).OnOffPeakTimeslotBackgroundChanged(e);
        }

        protected virtual void OnOffPeakTimeslotBackgroundChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_day != null)
            {
                _day.PopulateDay();
            }
        }

        #endregion

        private void FilterAppointments(DateTime date)
        {            
            if (_day != null)
            {
                _day.ItemsSource = Appointments.ByDate(date);
            }
        }

        
        CalendarLedger _ledger;
        CalendarDayHeader _dayHeader;
        ScrollViewer _scrollViewer;

CalendarDay _day;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _ledger = GetTemplateChild(ElementLedger) as CalendarLedger;
            if (_ledger != null)
            {
                _ledger.Owner = this;
            }

            _day = GetTemplateChild(ElementDay) as CalendarDay;
            if (_day != null)
            {
                _day.Owner = this;
            }

            _dayHeader = GetTemplateChild(ElementDayHeader) as CalendarDayHeader;
            if (_dayHeader != null)
            {
                _dayHeader.Owner = this;
            }

            _scrollViewer = GetTemplateChild(ElementScrollViewer) as ScrollViewer;
        }

        public void ScrollToHome()
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollToHome();
            }
        }

        public void ScrollToOffset(double offset)
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollToHorizontalOffset(offset);
            }
        }

        #region NextDay/PreviousDay

        public static readonly RoutedCommand NextDay = new RoutedCommand("NextDay", typeof(Calendar));
        public static readonly RoutedCommand PreviousDay = new RoutedCommand("PreviousDay", typeof(Calendar));

        private static void OnCanExecuteNextDay(object sender, CanExecuteRoutedEventArgs e)
        {
            ((Calendar)sender).OnCanExecuteNextDay(e);
        }

        private static void OnExecutedNextDay(object sender, ExecutedRoutedEventArgs e)
        {
            ((Calendar)sender).OnExecutedNextDay(e);
        }

        protected virtual void OnCanExecuteNextDay(CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = false;
        }

        protected virtual void OnExecutedNextDay(ExecutedRoutedEventArgs e)
        {
            CurrentDate += TimeSpan.FromDays(1);
            e.Handled = true;            
        }

        private static void OnCanExecutePreviousDay(object sender, CanExecuteRoutedEventArgs e)
        {
            ((Calendar)sender).OnCanExecutePreviousDay(e);
        }

        private static void OnExecutedPreviousDay(object sender, ExecutedRoutedEventArgs e)
        {
            ((Calendar)sender).OnExecutedPreviousDay(e);
        }

        protected virtual void OnCanExecutePreviousDay(CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = false;
        }

        protected virtual void OnExecutedPreviousDay(ExecutedRoutedEventArgs e)
        {
            CurrentDate -= TimeSpan.FromDays(1);
            e.Handled = true;
        }

        #endregion
    }
}

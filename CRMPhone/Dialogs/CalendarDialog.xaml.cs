using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using CRMPhone.ViewModel;
using RudiGrobler.Calendar.Common;
using RudiGrobler.Controls;
using RudiGrobler.Samples.Calendar;

namespace CRMPhone.Dialogs
{
    /// <summary>
    /// Interaction logic for AttachmentDialog.xaml
    /// </summary>
    public partial class CalendarDialog : Window
    {
        ObservableCollection<Appointment> appointments;
        private CalendarDialogViewModel _model;
        public CalendarDialog(CalendarDialogViewModel model)
        {
            InitializeComponent();
            _model = model;
            appointments = model.ScheduleTaskList;
        }
        private void Calendar_AddAppointment(object sender, RoutedEventArgs e)
        {
            CalendarTimeslotItem item = e.OriginalSource as CalendarTimeslotItem;
            if (item != null)
            {
                Appointment appointment = new Appointment();
                appointment.StartTime = item.StartTime;
                appointment.EndTime = item.StartTime + TimeSpan.FromMinutes(30);
                appointment.Subject = "Новая задача";

                NewAppointment napp = new NewAppointment();
                napp.BuildTimeComboBox(appointment.StartTime, appointment.EndTime);
                napp.DataContext = appointment;
                if (napp.ShowDialog() == true)
                {
                    appointment.StartTime = napp.ComposeStartTime();
                    appointment.EndTime = napp.ComposeEndTime();
                    var toRemove = appointments.Where(a => a.RequestId == null).ToList();
                    foreach (var app in toRemove)
                    {
                        appointments.Remove(app);
                    }
                    appointments.Add(appointment);
                }
            }
        }

        private void On_Loaded(object sender, RoutedEventArgs e)
        {
            calendar.Appointments = appointments;
        }

        private void Calendar_OnEditAppointment(object sender, RoutedEventArgs e)
        {
            return;
            CalendarAppointmentItem item = e.OriginalSource as CalendarAppointmentItem;
            if (item != null)
            {
                var element = item.Content as Appointment;
                NewAppointment napp = new NewAppointment();
                napp.BuildTimeComboBox(element.StartTime, element.EndTime);
                napp.DataContext = element;
                if (napp.ShowDialog() == true)
                {
                    element.StartTime = napp.ComposeStartTime();
                    element.EndTime = napp.ComposeEndTime();
                }
            }
        }
    }
}

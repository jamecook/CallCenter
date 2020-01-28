using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;
using RudiGrobler.Calendar.Common;

namespace CRMPhone.ViewModel
{
    public class CalendarDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private int? _requestId;
        private ObservableCollection<Appointment> _scheduleTaskList;
        private AttachmentDto _selectedNoteItem;


        public CalendarDialogViewModel(int? requestId)
        {
            _requestId = requestId;
            ScheduleTaskList = new ObservableCollection<Appointment>(GetList());
        }

        public ObservableCollection<Appointment> ScheduleTaskList
        {
            get { return _scheduleTaskList; }
            set { _scheduleTaskList = value; OnPropertyChanged(nameof(ScheduleTaskList));}
        }
        public void Refresh()
        {
            ScheduleTaskList = new ObservableCollection<Appointment>(GetList());
        }

        private IList<Appointment> GetList()
        {
            var retVal = new List<Appointment>();
            retVal.Add(new Appointment()
            {
                Id=1,
                Body = "Тест",
                Subject = "eeeee",
                StartTime = DateTime.Now.Date.AddHours(8),
                EndTime = DateTime.Now.Date.AddHours(10)
            });
            retVal.Add(new Appointment()
            {
                Id=2,
                Body = "Тест22",
                Subject = "eeee22",
                StartTime = DateTime.Now.Date.AddHours(11),
                EndTime = DateTime.Now.Date.AddHours(12)
            });
            retVal.Add(new Appointment()
            {
                Id=3,
                Body = "Тест333",
                Subject = "eeeee33",
                StartTime = DateTime.Now.Date.AddHours(14),
                EndTime = DateTime.Now.Date.AddHours(18)
            });
            return retVal;
        }

        private ICommand _addCommand;
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new CommandHandler(Add, true)); } }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(Delete, true)); } }
        private void Add()
        {

        }
        private void Delete()
        {
            //if (SelectedNoteItem != null)
            //{
            //    _requestService.DeleteAttachment(SelectedNoteItem.Id);
            //    Refresh();
            //}
        }

        public AttachmentDto SelectedNoteItem
        {
            get { return _selectedNoteItem; }
            set { _selectedNoteItem = value; OnPropertyChanged(nameof(SelectedNoteItem));}
        }


        public void SetView(Window view)
        {
            _view = view;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ClientPhone.Services;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class WorkerInfoViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private ObservableCollection<WorkerDto> _workersList;
        private WorkerDto _selectedWorker;
        private int _requestId;

        public WorkerInfoViewModel(int workerId,int requestId)
        {
            WorkersList = new ObservableCollection<WorkerDto>(RestRequestService.GetWorkerInfoWithParrents(AppSettings.CurrentUser.Id, workerId));
            _requestId = requestId;
        }

        public ObservableCollection<WorkerDto> WorkersList
        {
            get { return _workersList; }
            set { _workersList = value; OnPropertyChanged(nameof(WorkersList));}
        }
        public void Refresh()
        {
            WorkersList = new ObservableCollection<WorkerDto>();
        }


        private ICommand _dialCommand;
        public ICommand DialCommand { get { return _dialCommand ?? (_dialCommand = new RelayCommand(Dial)); } }

        private void Dial(object obj)
        {
            var master = obj as WorkerDto;
            if (master is null)
                return;

            ContextSaver.CrmContext.SipPhone = master.Phone;
            ContextSaver.CrmContext.Call();
            Thread.Sleep(500);

            var lastCallId = AppSettings.LastCallId;
            if (string.IsNullOrEmpty(lastCallId))
            {
                MessageBox.Show("ОШИБКА прикрепление звонка! Пустой номер последнего звонка!");
                return;
            }
            var callUniqueId = RestRequestService.GetActiveCallUniqueIdByCallId(AppSettings.CurrentUser.Id, lastCallId);

            if (!string.IsNullOrEmpty(callUniqueId))
            {
                RestRequestService.AddCallToRequest(AppSettings.CurrentUser.Id, _requestId, callUniqueId);
                RestRequestService.AddCallHistory(_requestId, callUniqueId, AppSettings.CurrentUser.Id, AppSettings.LastCallId,"WorkerInfoDial");

            }

        }


        public WorkerDto SelectedWorker
        {
            get { return _selectedWorker; }
            set { _selectedWorker = value; OnPropertyChanged(nameof(SelectedWorker));}
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
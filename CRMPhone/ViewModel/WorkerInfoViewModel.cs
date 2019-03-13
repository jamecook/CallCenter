using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class WorkerInfoViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private ObservableCollection<WorkerDto> _workersList;
        private WorkerDto _selectedWorker;
        private int _requestId;

        public WorkerInfoViewModel(RequestServiceImpl.RequestService requestService, int workerId,int requestId)
        {
            _requestService = requestService;
            WorkersList = new ObservableCollection<WorkerDto>(requestService.GetWorkerInfoWithParrents(workerId));
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
            var callUniqueId = _requestService.GetActiveCallUniqueIdByCallId(lastCallId);

            if (!string.IsNullOrEmpty(callUniqueId))
            {
                _requestService.AddCallToRequest(_requestId, callUniqueId);
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
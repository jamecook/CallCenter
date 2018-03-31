using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class CallsHistoryDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private int _requestId;
        private ObservableCollection<CallsListDto> _callsList;
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly RequestServiceImpl.RequestService _requestService;

        public CallsHistoryDialogViewModel(RequestServiceImpl.RequestService requestService, int requestId)
        {
            _requestId = requestId;
            _requestService = requestService;
            RefreshLists();
        }

        public void RefreshLists()
        {
            CallsList = new ObservableCollection<CallsListDto>(_requestService.GetCallListByRequestId(_requestId));
            SmsList = new ObservableCollection<SmsListDto>(_requestService.GetSmsByRequestId(_requestId));
        }
        public ObservableCollection<SmsListDto> SmsList
        {
            get { return _smsList; }
            set { _smsList = value; OnPropertyChanged(nameof(SmsList));}
        }

        public ObservableCollection<CallsListDto> CallsList
        {
            get { return _callsList; }
            set
            {
                _callsList = value;
                OnPropertyChanged(nameof(CallsList));
            }
        }
        private ICommand _playCommand;
        public ICommand PlayCommand { get { return _playCommand ?? (_playCommand = new RelayCommand(PlayRecord)); } }
        private ICommand _closeCommand;
        private ObservableCollection<SmsListDto> _smsList;
        public ICommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(Close)); } }

        private ICommand _sendSmsToCitizenCommand;
        public ICommand SendSmsToCitizenCommand { get { return _sendSmsToCitizenCommand ?? (_sendSmsToCitizenCommand = new RelayCommand(SendSmsToCitizen)); } }

        private void SendSmsToCitizen(object obj)
        {
            var request = _requestService.GetRequest(_requestId);
            var smsSettings = _requestService.GetSmsSettingsForServiceCompany(request.ServiceCompanyId);
            if (smsSettings.SendToClient && request.Contacts.Any(c => c.IsMain))
            {
                _requestService.SendSms(request.Id, smsSettings.Sender,
                    request.Contacts.FirstOrDefault(c => c.IsMain)?.PhoneNumber,
                    $"Заявка № {request.Id}. {request.Type.ParentName} - {request.Type.Name}", true);
                MessageBox.Show(Application.Current.MainWindow, "Сообщение поставлено в очередь на отправку!", "Сообщение");
                RefreshLists();
            }
        }

        private ICommand _sendSmsToWorkerCommand;
        public ICommand SendSmsToWorkerCommand { get { return _sendSmsToWorkerCommand ?? (_sendSmsToWorkerCommand = new RelayCommand(SendSmsToWorker)); } }

        private void SendSmsToWorker(object obj)
        {
            var request = _requestService.GetRequest(_requestId);
            var smsSettings = _requestService.GetSmsSettingsForServiceCompany(request.ServiceCompanyId);
            if (!request.MasterId.HasValue)
                return;
            var worker = _requestService.GetWorkerById(request.MasterId.Value);
            string phones = "";
            if (request.Contacts != null && request.Contacts.Length > 0)
                phones = request.Contacts.OrderBy(c=>c.IsMain).Select(c =>
                        {
                            var retVal = c.PhoneNumber.Length == 10 ? "8" + c.PhoneNumber : c.PhoneNumber;
                            if (!string.IsNullOrEmpty(c.Name))
                            {
                                retVal += $" - {c.Name}";
                            }
                            return retVal;
                        }
                )
                        .Aggregate((i, j) => i + ";" + j);
            if (smsSettings.SendToWorker)
            {
                _requestService.SendSms(request.Id, smsSettings.Sender, worker.Phone,
                    $"№ {request.Id}. {request.Type.ParentName}/{request.Type.Name}({request.Description}) {request.Address.FullAddress}. {phones}.",
                    false);
                MessageBox.Show(Application.Current.MainWindow, "Сообщение поставлено в очередь на отправку!", "Сообщение");
                RefreshLists();
            }
        }

        private void Close(object sender)
        {
            _view.DialogResult = true;
        }

        private void PlayRecord(object obj)
        {
            var record = obj as CallsListDto;
            var serverIpAddress = ConfigurationManager.AppSettings["CallCenterIP"];
            _requestService.PlayRecord(serverIpAddress, record.MonitorFileName);

            /*
            var localFileName = record.MonitorFileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\");
            Process.Start(localFileName);
            */
        }
        public void SetView(Window view)
        {
            _view = view;
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using Microsoft.Win32;
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
        private ICommand _downloadRecordCommand;
        public ICommand DownloadRecordCommand { get { return _downloadRecordCommand ?? (_downloadRecordCommand = new RelayCommand(DownloadRecord)); } }
        private ICommand _deleteRecordCommand;
        public ICommand DeleteRecordCommand { get { return _deleteRecordCommand ?? (_deleteRecordCommand = new RelayCommand(DeleteRecord)); } }
        private ICommand _closeCommand;
        private ObservableCollection<SmsListDto> _smsList;
        public ICommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(Close)); } }

        private ICommand _sendSmsToCitizenCommand;
        public ICommand SendSmsToCitizenCommand { get { return _sendSmsToCitizenCommand ?? (_sendSmsToCitizenCommand = new RelayCommand(SendSmsToCitizen)); } }

        private void SendSmsToCitizen(object obj)
        {
            _requestService.SendSmsToClient(_requestId);
            MessageBox.Show(Application.Current.MainWindow, "Сообщение поставлено в очередь на отправку!", "Сообщение");
            RefreshLists();
        }

        private ICommand _sendSmsToWorkerCommand;
        public ICommand SendSmsToWorkerCommand { get { return _sendSmsToWorkerCommand ?? (_sendSmsToWorkerCommand = new RelayCommand(SendSmsToWorker)); } }
        private ICommand _sendSmsToExecutorCommand;
        public ICommand SendSmsToExecutorCommand { get { return _sendSmsToExecutorCommand ?? (_sendSmsToExecutorCommand = new RelayCommand(SendSmsToExecutor)); } }

        private void SendSmsToWorker(object obj)
        {
            _requestService.SendSmsToWorker(_requestId, true, false);
            MessageBox.Show(Application.Current.MainWindow, "Сообщение поставлено в очередь на отправку!", "Сообщение");
            RefreshLists();
        }
        private void SendSmsToExecutor(object obj)
        {
            _requestService.SendSmsToWorker(_requestId, false, true);
            MessageBox.Show(Application.Current.MainWindow, "Сообщение поставлено в очередь на отправку!", "Сообщение");
            RefreshLists();

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
        private void DownloadRecord(object obj)
        {
            var record = obj as CallsListDto;
            var serverIpAddress = ConfigurationManager.AppSettings["CallCenterIP"];
            var saveDialog = new SaveFileDialog();
            saveDialog.AddExtension = true;
            saveDialog.DefaultExt = ".wav";
            saveDialog.Filter = "Audio файл|*.wav";
            if (saveDialog.ShowDialog() == true)
            {
                var localFileName = record.MonitorFileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\").Replace("/", "\\");
                var localFileNameMp3 = localFileName.Replace(".wav", ".mp3");
                if (File.Exists(localFileNameMp3))
                    File.Copy(localFileNameMp3,saveDialog.FileName);
                else if (File.Exists(localFileName))
                    File.Copy(localFileName, saveDialog.FileName);
                //todo Можно убрать после перетаскивая записей
                localFileName = record.MonitorFileName.Replace("/raid/monitor/", $"\\\\192.168.1.130\\mixmonitor\\").Replace("/", "\\");
                localFileNameMp3 = record.MonitorFileName.Replace("/raid/monitor/", $"\\\\192.168.1.130\\mixmonitor\\mp3\\").Replace("/", "\\").Replace(".wav", ".mp3");

                if (File.Exists(localFileNameMp3))
                    File.Copy(localFileNameMp3,saveDialog.FileName);
                else if (File.Exists(localFileName))
                    File.Copy(localFileName, saveDialog.FileName);
            }
        }
        private void DeleteRecord(object obj)
        {
            var record = obj as CallsListDto;
            if (record == null)
                return;
            if (MessageBox.Show(_view, "Вы уверены что хотите удалить эту запись?", "Удалить",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _requestService.DeleteCallListRecord(record.Id);
                CallsList = new ObservableCollection<CallsListDto>(_requestService.GetCallListByRequestId(_requestId));
            }
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
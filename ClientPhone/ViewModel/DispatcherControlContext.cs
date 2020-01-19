using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ClientPhone.Services;
using CRMPhone.Annotations;
using CRMPhone.Dialogs;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class DispatcherControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<DispatcherStatDto> _dispatcherList;
        private RequestService _requestService;
        private readonly DispatcherTimer _refreshTimer;

        private void RefreshDispatchers()
        {
            var dispatcherStatistics = RestRequestService.GetDispatcherStat(AppSettings.CurrentUser.Id);
            var remotedChannels = DispatcherList.Where(n => dispatcherStatistics.All(c => c.Id != n.Id || c.IpAddress != n.IpAddress)).ToList();
            var newChannels = dispatcherStatistics.Where(n => DispatcherList.All(c => c.Id != n.Id && c.IpAddress != n.IpAddress)).ToList();
            newChannels.ForEach(c => DispatcherList.Add(c));
            remotedChannels.ForEach(c => DispatcherList.Remove(c));

            foreach (var statDto in dispatcherStatistics)
            {
                var dto = DispatcherList.FirstOrDefault(c => c.Id == statDto.Id && c.IpAddress == statDto.IpAddress);
                if (dto != null)
                {
                    dto.Direction = statDto.Direction;
                    dto.PhoneNumber = statDto.PhoneNumber;
                    dto.UniqueId = statDto.UniqueId;
                    dto.TalkTime = statDto.TalkTime;
                    dto.WaitingTime = statDto.WaitingTime;
                    dto.OnLine = statDto.OnLine;
                    dto.Version = statDto.Version;
                }
            }

            OnPropertyChanged(nameof(DispatcherList));
        }
        private ICommand _screenCommand;
        public ICommand ScreenCommand { get { return _screenCommand ?? (_screenCommand = new RelayCommand(ScreenShot)); } }

        private void ScreenShot(object obj)
        {
            var item = obj as DispatcherStatDto;
            if (item == null)
                return;
            var commandId = _requestService.DispatcherSendCommand(item.IpAddress, 1);
            for (int t = 1; t <= 6; t++)
            {
                var stream = _requestService.DispatcherGetScreenShot(commandId.Value);
                if (stream != null)
                {
                    stream.Position = 0;
                    var fileName = Path.GetTempPath() + "screen.jpg";
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    var file = File.Create(fileName);
                    stream.WriteTo(file);
                    file.Close();
                    Process.Start(fileName);
                    return;
                }
                Thread.Sleep(500);
            }

            MessageBox.Show("Не удалось получить скриншот за отведенное время!");
            //var serverIpAddress = ConfigurationManager.AppSettings["CallCenterIP"];
            //var fileName = _requestService.GetRecordFileNameByUniqueId(item.RecordUniqueId);
            //_requestService.PlayRecord(serverIpAddress, fileName);
        }

        public DispatcherControlContext()
        {
            _refreshTimer = new DispatcherTimer();
            DispatcherList = new ObservableCollection<DispatcherStatDto>();
        }

        public void InitCollections()
        {
            _requestService = new RequestService(AppSettings.DbConnection);

            if (AppSettings.CurrentUser.Roles.Exists(r => r.Name == "admin"))
            {
                _refreshTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
                _refreshTimer.Tick += RefreshTimerOnTick;
                _refreshTimer.Start();
            }
        }

        private void RefreshTimerOnTick(object sender, EventArgs e)
        {
            RefreshDispatchers();
        }

        public ObservableCollection<DispatcherStatDto> DispatcherList
        {
            get { return _dispatcherList; }
            set { _dispatcherList = value; OnPropertyChanged(nameof(DispatcherList));}
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
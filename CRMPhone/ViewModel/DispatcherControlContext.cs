using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
            var dispatcherStatistics = _requestService.GetDispatcherStatistics();
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
                }
            }

            OnPropertyChanged(nameof(DispatcherList));
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
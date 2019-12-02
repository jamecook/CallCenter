using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dialogs;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class AlertAndWorkControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<AlertDto> _alertList;
        private RequestService _requestService;

        private ICommand _refreshRequestCommand;
        public ICommand RefreshRequestCommand { get { return _refreshRequestCommand ?? (_refreshRequestCommand = new CommandHandler(RefreshAlerts, true)); } }
        private ICommand _newCommand;
        public ICommand NewCommand { get { return _newCommand ?? (_newCommand = new CommandHandler(CreateNew, true)); } }

        private void CreateNew()
        {
            var model = new AlertAndWorkDialogViewModel(null);
            var view = new AlertAndWorkDialog
            {
                DataContext = model,
                Owner = Application.Current.MainWindow
            };
            model.SetView(view);
            view.ShowDialog();
            RefreshAlerts();
        }

        private int _alertCount;

        private ICommand _openCommand;
        private bool _onlyActive;
        private DateTime _fromDate;
        private DateTime _toDate;
        public ICommand OpenCommand { get { return _openCommand ?? (_openCommand = new RelayCommand(Open));} }


        private void Open(object sender)
        {
            var selectedItem = sender as AlertDto;
            if (selectedItem == null)
                return;

            var model = new AlertAndWorkDialogViewModel(selectedItem);
            var view = new AlertAndWorkDialog
            {
                DataContext = model,
                Owner = Application.Current.MainWindow
            };
            model.SetView(view);
            if(view.ShowDialog() == true)
                RefreshAlerts();
        }

        private void RefreshAlerts()
        {
            AlertList.Clear();
            var alerts = _requestService.GetAlerts(FromDate,ToDate,null,OnlyActive);
            foreach (var alert in alerts)
            {
                AlertList.Add(alert);
            }
            AlertCount = AlertList.Count;
            OnPropertyChanged(nameof(AlertList));
        }

        public int AlertCount
        {
            get { return _alertCount; }
            set { _alertCount = value; OnPropertyChanged(nameof(AlertCount));}
        }

        public bool OnlyActive
        {
            get { return _onlyActive; }
            set {
                _onlyActive = value;
                OnPropertyChanged(nameof(OnlyActive));
                OnPropertyChanged(nameof(CanSelectDate));
                }
        }
        public bool CanSelectDate => !OnlyActive;

        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; OnPropertyChanged(nameof(FromDate));}
        }

        public DateTime ToDate
        {
            get { return _toDate; }
            set { _toDate = value; OnPropertyChanged(nameof(ToDate)); }
        }

        public AlertAndWorkControlContext()
        {
            AlertList = new ObservableCollection<AlertDto>();
            OnlyActive = true;
            FromDate = DateTime.Today.AddDays(-7);
            ToDate = DateTime.Today.AddDays(1).AddSeconds(-1);
        }

        public void InitCollections()
        {
            _requestService = new RequestService(AppSettings.DbConnection);
            var currentDate = _requestService.GetCurrentDate().Date;
            FromDate = currentDate.AddDays(-10);
            ToDate = currentDate.AddDays(1).AddSeconds(-1);
            RefreshAlerts();
        }

        public ObservableCollection<AlertDto> AlertList
        {
            get { return _alertList; }
            set { _alertList = value; OnPropertyChanged(nameof(AlertList));}
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
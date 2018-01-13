using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dialogs.Admins;
using CRMPhone.ViewModel.Admins;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class RingUpAdminControlContext : INotifyPropertyChanged
    {
        private RequestService _requestService;
        private RequestService RequestService => _requestService ?? (_requestService = new RequestService(AppSettings.DbConnection));

        private ObservableCollection<RingUpHistoryDto> _ringUpList;
        private ICommand _ediCommand;
        public ICommand EditCommand { get { return _ediCommand ?? (_ediCommand = new CommandHandler(EditPhone, true)); } }
        private ICommand _refreshCommand;
        private RingUpHistoryDto _currentRingUp;
        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new CommandHandler(Refresh, true)); } }
        private ICommand _newCommand;
        public ICommand NewCommand { get { return _newCommand ?? (_newCommand = new CommandHandler(NewRingUp, true)); } }

        private void NewRingUp()
        {
            var model = new RingUpNewDialogViewModel(RequestService);
            var view = new RingUpNewDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                Refresh();
            }
        }

        public RingUpHistoryDto CurrentRingUp
        {
            get { return _currentRingUp; }
            set { _currentRingUp = value; OnPropertyChanged(nameof(CurrentRingUp));}
        }

        public ObservableCollection<RingUpHistoryDto> RingUpList
        {
            get { return _ringUpList; }
            set { _ringUpList = value; OnPropertyChanged(nameof(RingUpList)); }
        }

        private void EditPhone()
        {

            var model = new PhoneDialogViewModel(RequestService);
            var view = new PhoneAddOrUpdateDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                Refresh();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh()
        {
            RingUpList = new ObservableCollection<RingUpHistoryDto>(RequestService.GetRingUpHistory());
        }
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Dialogs.Admins;
using CRMPhone.ViewModel.Admins;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class BlackListControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<BlackListPhoneDto> _blackListPhones;
        public ObservableCollection<BlackListPhoneDto> BlackListPhones
        {
            get { return _blackListPhones; }
            set { _blackListPhones = value; OnPropertyChanged(nameof(BlackListPhones)); }
        }

        public BlackListPhoneDto SelectedPhone
        {
            get { return _selectedPhone; }
            set { _selectedPhone = value; OnPropertyChanged(nameof(SelectedPhone)); }
        }

        private RequestService _requestService;
        private ICommand _addNewCommand;
        public ICommand AddNewCommand { get { return _addNewCommand ?? (_addNewCommand = new CommandHandler(AddPhone, true)); } }
        private ICommand _deleteCommand;
        private BlackListPhoneDto _selectedPhone;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(DeletePhone, true)); } }

        public BlackListControlContext()
        {
            BlackListPhones = new ObservableCollection<BlackListPhoneDto>();
        }
        private void DeletePhone()
        {
            if (SelectedPhone == null)
                return;
            if (_requestService == null)
                _requestService = new RequestService(AppSettings.DbConnection);
            if (MessageBox.Show(Application.Current.MainWindow,$"Вы уверены что хотите удалить номер {SelectedPhone.Phone} из черного списка?","Черный список",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _requestService.DeletePhoneFromBlackList(SelectedPhone);
                RefreshList();
            }
        }

        public void RefreshList()
        {
            if (_requestService == null)
                _requestService = new RequestService(AppSettings.DbConnection);
            BlackListPhones.Clear();

            _requestService.GetBlackListPhones().ForEach(c => BlackListPhones.Add(c));
            
            OnPropertyChanged(nameof(BlackListPhones));
        }

        private void AddPhone()
        {
            var model = new BlackListPhoneDialogViewModel();
            var view = new BlackListPhoneAddDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true && !string.IsNullOrEmpty(model.PhoneNumber))
            {
                _requestService.AddPhoneToBlackList(model.PhoneNumber);
                RefreshList();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
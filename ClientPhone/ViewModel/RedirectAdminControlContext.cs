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
    public class RedirectAdminControlContext : INotifyPropertyChanged
    {
        private RequestService _requestService;
        private RequestService RequestService => _requestService ?? (_requestService = new RequestService(AppSettings.DbConnection));

        private ICommand _ediCommand;
        private string _phoneNumber;
        public ICommand EditCommand { get { return _ediCommand ?? (_ediCommand = new CommandHandler(EditPhone, true)); } }

        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber));}
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
            PhoneNumber = RequestService.GetRedirectPhone();
        }
    }
}
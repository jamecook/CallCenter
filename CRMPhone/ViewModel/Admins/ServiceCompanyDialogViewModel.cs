using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;

namespace CRMPhone.ViewModel.Admins
{
    public class ServiceCompanyDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int? _serviceCompanyId;
        private ICommand _saveCommand;
        private string _serviceName;
        private string _serviceCompanyInfo;

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; OnPropertyChanged(nameof(ServiceName));}
        }
        public string ServiceCompanyInfo
        {
            get { return _serviceCompanyInfo; }
            set { _serviceCompanyInfo = value; OnPropertyChanged(nameof(ServiceCompanyInfo)); }
        }

        public ServiceCompanyDialogViewModel(RequestServiceImpl.RequestService requestService, int? serviceCompanyId)
        {
            _requestService = requestService;
            _serviceCompanyId = serviceCompanyId;
            if (serviceCompanyId.HasValue)
            {
                var serviceCompany = _requestService.GetServiceCompanyById(serviceCompanyId.Value);
                ServiceName = serviceCompany.Name;
                ServiceCompanyInfo = serviceCompany.Info;
            }
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            _requestService.SaveServiceCompany(_serviceCompanyId,ServiceName,ServiceCompanyInfo);
            _view.DialogResult = true;
        }

      
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
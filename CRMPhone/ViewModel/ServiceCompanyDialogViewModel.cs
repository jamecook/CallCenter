using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.ViewModel;
using RequestServiceImpl.Dto;

namespace CRMPhone
{
    public class ServiceCompanyDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int? _serviceCompanyId;
        private ICommand _saveCommand;
        private string _serviceName;

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; OnPropertyChanged(nameof(ServiceName));}
        }

        public ServiceCompanyDialogViewModel(RequestServiceImpl.RequestService requestService, int? serviceCompanyId)
        {
            _requestService = requestService;
            _serviceCompanyId = serviceCompanyId;
            if (serviceCompanyId.HasValue)
            {
                var serviceCompany = _requestService.GetServiceCompanyById(serviceCompanyId.Value);
                ServiceName = serviceCompany.Name;
            }
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            _requestService.SaveServiceCompany(_serviceCompanyId,ServiceName);
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
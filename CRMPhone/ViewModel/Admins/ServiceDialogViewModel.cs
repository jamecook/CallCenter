using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class ServiceDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int? _serviceId;
        private int? _parentId;
        private ICommand _saveCommand;
        private string _serviceName;
        private bool _immediate;
        private bool _availableForClient;

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; OnPropertyChanged(nameof(ServiceName));}
        }

        public bool Immediate
        {
            get { return _immediate; }
            set { _immediate = value; OnPropertyChanged(nameof(Immediate));}
        }

        public bool AvailableForClient
        {
            get => _availableForClient;
            set { _availableForClient = value;  OnPropertyChanged(nameof(AvailableForClient));}
        }

        public ServiceDialogViewModel(RequestServiceImpl.RequestService requestService, int? serviceId, int? parentId)
        {
            _requestService = requestService;
            _serviceId = serviceId;
            _parentId = parentId;
            if (serviceId.HasValue)
            {
                var service = _requestService.GetServiceById(serviceId.Value);
                ServiceName = service.Name;
                Immediate = service.Immediate;
                AvailableForClient = service.AvailableForClient;
            }
        }

        public Visibility CanSetImmediate => _parentId.HasValue ? Visibility.Visible : Visibility.Collapsed;
        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            _requestService.SaveService(_serviceId, _parentId, ServiceName,Immediate,AvailableForClient);
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
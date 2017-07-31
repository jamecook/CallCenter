using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.ViewModel;
using RequestServiceImpl.Dto;

namespace CRMPhone
{
    public class WorkerAdminDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int? _workerId;
        private ICommand _saveCommand;
        private string _surName;
        private string _firstName;
        private string _patrName;
        private string _phone;
        private ObservableCollection<ServiceCompanyDto> _serviceCompanyList;
        private ServiceCompanyDto _selectedServiceCompany;

        public ObservableCollection<ServiceCompanyDto> ServiceCompanyList
        {
            get => _serviceCompanyList;
            set { _serviceCompanyList = value; OnPropertyChanged(nameof(ServiceCompanyList)); }
        }

        public ServiceCompanyDto SelectedServiceCompany
        {
            get => _selectedServiceCompany;
            set { _selectedServiceCompany = value; OnPropertyChanged(nameof(SelectedServiceCompany)); }
        }

        public string SurName
        {
            get { return _surName; }
            set { _surName = value; OnPropertyChanged(nameof(SurName)); }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); }
        }

        public string PatrName
        {
            get { return _patrName; }
            set { _patrName = value; OnPropertyChanged(nameof(PatrName)); }
        }

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; OnPropertyChanged(nameof(Phone)); }
        }

        public WorkerAdminDialogViewModel(RequestServiceImpl.RequestService requestService, int? workerId)
        {
            _requestService = requestService;
            _workerId = workerId;
            ServiceCompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
            if (workerId.HasValue)
            {
                var worker = _requestService.GetWorkerById(workerId.Value);
                SurName = worker.SurName;
                FirstName = worker.FirstName;
                PatrName = worker.PatrName;
                Phone = worker.Phone;
                SelectedServiceCompany = ServiceCompanyList.SingleOrDefault(s => s.Id == worker.ServiceCompanyId);
            }
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            if (SelectedServiceCompany != null && !string.IsNullOrEmpty(SurName))
            {
                _requestService.SaveWorker(_workerId, SelectedServiceCompany.Id, SurName, FirstName, PatrName, Phone);
                _view.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Необходимо заполнить УК и фамилию!");
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dialogs.Admins;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class WorkerAdminDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int? _workerId;
        private ICommand _saveCommand;
        private ICommand _addressesCommand;
        private string _surName;
        private string _firstName;
        private string _patrName;
        private string _phone;
        private ObservableCollection<ServiceCompanyDto> _serviceCompanyList;
        private ServiceCompanyDto _selectedServiceCompany;
        private ObservableCollection<SpecialityDto> _specialityList;
        private SpecialityDto _selectedSpeciality;
        private ObservableCollection<WorkerDto> _parentWorkerList;
        private WorkerDto _selectedParentWorker;
        private bool _canAssign;
        private bool _isMaster;
        private bool _sendSms;
        private bool _isExecuter;
        private bool _isDispetcher;
        private string _login;
        private string _password;
        private bool _appNotification;

        public ObservableCollection<SpecialityDto> SpecialityList
        {
            get { return _specialityList; }
            set { _specialityList = value; OnPropertyChanged(nameof(SpecialityList));}
        }

        public SpecialityDto SelectedSpeciality
        {
            get { return _selectedSpeciality; }
            set { _selectedSpeciality = value; OnPropertyChanged(nameof(SelectedSpeciality));}
        }

        public ObservableCollection<ServiceCompanyDto> ServiceCompanyList
        {
            get => _serviceCompanyList;
            set { _serviceCompanyList = value; OnPropertyChanged(nameof(ServiceCompanyList)); }
        }

        public ServiceCompanyDto SelectedServiceCompany
        {
            get => _selectedServiceCompany;
            set { _selectedServiceCompany = value;
                //ChangeParentWorkerList(value);
                OnPropertyChanged(nameof(SelectedServiceCompany)); }
        }

        private void ChangeParentWorkerList(ServiceCompanyDto serviceCompany)
        {
            ParentWorkerList.Clear();
            ParentWorkerList.Add(new WorkerDto(){Id= 0, SurName = "Нет руководителя" });
            _requestService.GetExecuters(serviceCompany.Id).ToList().ForEach(w=>ParentWorkerList.Add(w));
        }

        public ObservableCollection<WorkerDto> ParentWorkerList
        {
            get { return _parentWorkerList; }
            set { _parentWorkerList = value; OnPropertyChanged(nameof(ParentWorkerList));}
        }

        public WorkerDto SelectedParentWorker
        {
            get { return _selectedParentWorker; }
            set { _selectedParentWorker = value; OnPropertyChanged(nameof(SelectedParentWorker));}
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

        public string Login
        {
            get { return _login; }
            set { _login = value; OnPropertyChanged(nameof(Login));}
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(nameof(Password));}
        }

        public bool CanAssign
        {
            get { return _canAssign; }
            set { _canAssign = value; OnPropertyChanged(nameof(CanAssign)); }
        }

        public bool IsMaster
        {
            get { return _isMaster; }
            set { _isMaster = value; OnPropertyChanged(nameof(IsMaster));}
        }

        public bool IsExecuter
        {
            get { return _isExecuter; }
            set { _isExecuter = value; OnPropertyChanged(nameof(IsExecuter));}
        }

        public bool IsDispetcher
        {
            get { return _isDispetcher; }
            set { _isDispetcher = value; OnPropertyChanged(nameof(IsDispetcher));}
        }
        public bool CanCreateRequest { get; set; }
        public bool ShowAllRequest { get; set; }
        public bool CanCloseRequest { get; set; }
        public bool CanSetRating { get; set; }
        public bool CanShowStatistic { get; set; }
        public bool CanChangeExecutor { get; set; }
        public bool ShowOnlyGaranty { get; set; }
        public bool FilterByHouses { get; set; }

        public bool SendSms
        {
            get { return _sendSms; }
            set { _sendSms = value; OnPropertyChanged(nameof(SendSms));}
        }

        public bool AppNotification
        {
            get { return _appNotification; }
            set { _appNotification = value; OnPropertyChanged(nameof(AppNotification));}
        }

        public WorkerAdminDialogViewModel(RequestServiceImpl.RequestService requestService, int? workerId)
        {
            _requestService = requestService;
            _workerId = workerId;
            ServiceCompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
            SpecialityList = new ObservableCollection<SpecialityDto>(_requestService.GetSpecialities());
            ParentWorkerList = new ObservableCollection<WorkerDto>();
            ParentWorkerList.Clear();
            ParentWorkerList.Add(new WorkerDto() { Id = 0, SurName = "Нет руководителя" });
            _requestService.GetAllWorkers(null).ToList().ForEach(w => ParentWorkerList.Add(w));
            if (workerId.HasValue)
            {
                var worker = _requestService.GetWorkerById(workerId.Value);
                SurName = worker.SurName;
                FirstName = worker.FirstName;
                PatrName = worker.PatrName;
                Phone = worker.Phone;
                Login = worker.Login;
                Password = worker.Password;
                CanAssign = worker.CanAssign;
                IsMaster = worker.IsMaster;
                IsExecuter = worker.IsExecuter;
                IsDispetcher = worker.IsDispetcher;
                SendSms = worker.SendSms;
                AppNotification = worker.AppNotification;

                CanSetRating = worker.CanSetRating;
                CanChangeExecutor = worker.CanChangeExecutor;
                CanCreateRequest = worker.CanCreateRequest;
                ShowAllRequest = worker.ShowAllRequest;
                CanCloseRequest = worker.CanCloseRequest;
                CanShowStatistic = worker.CanShowStatistic;
                ShowOnlyGaranty = worker.ShowOnlyGaranty;
                FilterByHouses = worker.FilterByHouses;

                SelectedServiceCompany = ServiceCompanyList.SingleOrDefault(s => s.Id == worker.ServiceCompanyId);
                SelectedSpeciality = SpecialityList.SingleOrDefault(s => s.Id == worker.SpecialityId);
                var selectParentWorkerId = worker.ParentWorkerId ?? 0;
                SelectedParentWorker = ParentWorkerList.SingleOrDefault(w => w.Id == selectParentWorkerId);
            }
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }
        public ICommand AddressesCommand { get { return _addressesCommand ?? (_addressesCommand = new RelayCommand(AddressesBinding)); } }

        private void AddressesBinding(object obj)
        {
            if (!_workerId.HasValue)
            {
                MessageBox.Show(_view, "Адреса можно привязывать только предварительно сохранив исполнителя!(Сохраните, закройте и войдите повторно в это окно)");
                return;
            }
            var model = new BindAddressToWorkerDialogViewModel(_requestService,_workerId.Value);
            var view = new BindAddressToWorkerDialog();
            view.DataContext = model;
            view.Owner = _view;
            model.SetView(view);
            view.ShowDialog();
        }

        private void Save(object sender)
        {
            if (SelectedServiceCompany != null && !string.IsNullOrEmpty(SurName) && SelectedSpeciality != null)
            {
                _requestService.SaveWorker(_workerId, SelectedServiceCompany.Id, SurName, FirstName, PatrName, Phone, SelectedSpeciality.Id,CanAssign,
                    IsMaster, IsExecuter, IsDispetcher, SendSms, Login, Password, (SelectedParentWorker!=null && SelectedParentWorker.Id>0)? SelectedParentWorker.Id :(int?) null,
                     CanSetRating, CanCloseRequest, CanChangeExecutor, CanCreateRequest, CanShowStatistic, FilterByHouses, ShowAllRequest, ShowOnlyGaranty,AppNotification);
                _view.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Необходимо заполнить УК, фамилию и специальность!");
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
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
        private ObservableCollection<SpecialityDto> _specialityList;
        private SpecialityDto _selectedSpeciality;
        private ObservableCollection<WorkerDto> _parentWorkerList;
        private WorkerDto _selectedParentWorker;
        private bool _canAssign;

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
                ChangeParentWorkerList(value);
                OnPropertyChanged(nameof(SelectedServiceCompany)); }
        }

        private void ChangeParentWorkerList(ServiceCompanyDto serviceCompany)
        {
            ParentWorkerList.Clear();
            ParentWorkerList.Add(new WorkerDto(){Id= 0, SurName = "Нет руководителя" });
            _requestService.GetWorkers(serviceCompany.Id).ToList().ForEach(w=>ParentWorkerList.Add(w));
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

        public bool CanAssign
        {
            get { return _canAssign; }
            set { _canAssign = value; OnPropertyChanged(nameof(CanAssign)); }
        }

        public WorkerAdminDialogViewModel(RequestServiceImpl.RequestService requestService, int? workerId)
        {
            _requestService = requestService;
            _workerId = workerId;
            ServiceCompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
            SpecialityList = new ObservableCollection<SpecialityDto>(_requestService.GetSpecialities());
            ParentWorkerList = new ObservableCollection<WorkerDto>();
            if (workerId.HasValue)
            {
                var worker = _requestService.GetWorkerById(workerId.Value);
                SurName = worker.SurName;
                FirstName = worker.FirstName;
                PatrName = worker.PatrName;
                Phone = worker.Phone;
                CanAssign = worker.CanAssign;
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

        private void Save(object sender)
        {
            if (SelectedServiceCompany != null && !string.IsNullOrEmpty(SurName) && SelectedSpeciality != null)
            {
                _requestService.SaveWorker(_workerId, SelectedServiceCompany.Id, SurName, FirstName, PatrName, Phone, SelectedSpeciality.Id, CanAssign, SelectedParentWorker.Id>0? SelectedParentWorker.Id :(int?) null);
                _view.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Необходимо заполнить УК и фамилию и специальность!");
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
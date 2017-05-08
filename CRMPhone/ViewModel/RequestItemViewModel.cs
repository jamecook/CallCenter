using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CRMPhone.Annotations;
using CRMPhone.Dto;

namespace CRMPhone.ViewModel
{
    public class RequestItemViewModel : INotifyPropertyChanged
    {
        private readonly RequestService _requestService;
        private ObservableCollection<WorkerDto> _workerList;
        private WorkerDto _selectedWorker;
        private ObservableCollection<ServiceDto> _parentServiceList;
        private ServiceDto _selectedParentService;
        private ObservableCollection<ServiceDto> _serviceList;
        private ServiceDto _selectedService;
        private DateTime? _selectedDateTime;

        private string _uniqueId;
        private bool _isChargeable;
        private bool _isImmediate;
        private ObservableCollection<ServiceCompanyDto> _companyList;
        private ServiceCompanyDto _selectedCompany;
        private int? _requestId;
        private ObservableCollection<PeriodDto> _periodList;
        private PeriodDto _selectedPeriod;

        public RequestItemViewModel()
        {
            _requestService = new RequestService(AppSettings.DbConnection);
            ServiceList = new ObservableCollection<ServiceDto>();
            WorkerList = new ObservableCollection<WorkerDto>(_requestService.GetWorkers(null));
            ParentServiceList = new ObservableCollection<ServiceDto>(_requestService.GetServices(null));
            SelectedParentService = ParentServiceList.FirstOrDefault();
            PeriodList = new ObservableCollection<PeriodDto>(_requestService.GetPeriods());
            SelectedPeriod = PeriodList.FirstOrDefault();
            CompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
            SelectedCompany = CompanyList.FirstOrDefault();
        }

        public string UniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; OnPropertyChanged(nameof(UniqueId)); }
        }

        public int? RequestId
        {
            get { return _requestId; }
            set { _requestId = value; OnPropertyChanged(nameof(RequestId)); }
        }

        public ObservableCollection<ServiceCompanyDto> CompanyList
        {
            get { return _companyList; }
            set { _companyList = value; OnPropertyChanged(nameof(CompanyList));}
        }

        public ServiceCompanyDto SelectedCompany
        {
            get { return _selectedCompany; }
            set { _selectedCompany = value; OnPropertyChanged(nameof(SelectedCompany));}
        }

        public ObservableCollection<ServiceDto> ParentServiceList
        {
            get { return _parentServiceList; }
            set { _parentServiceList = value; OnPropertyChanged(nameof(ParentServiceList)); }
        }

        public bool IsChargeable
        {
            get { return _isChargeable; }
            set { _isChargeable = value; OnPropertyChanged(nameof(IsChargeable)); }
        }

        public bool IsImmediate
        {
            get { return _isImmediate; }
            set { _isImmediate = value; OnPropertyChanged(nameof(IsImmediate)); }
        }

        public ServiceDto SelectedParentService
        {
            get { return _selectedParentService; }
            set
            {
                _selectedParentService = value;
                ChangeParentService(value?.Id);
                OnPropertyChanged(nameof(SelectedParentService));
            }
        }

        public DateTime? SelectedDateTime
        {
            get { return _selectedDateTime; }
            set { _selectedDateTime = value; OnPropertyChanged(nameof(SelectedDateTime)); }
        }

        public ObservableCollection<ServiceDto> ServiceList
        {
            get { return _serviceList; }
            set { _serviceList = value; OnPropertyChanged(nameof(ServiceList)); }
        }

        public ServiceDto SelectedService
        {
            get { return _selectedService; }
            set { _selectedService = value; OnPropertyChanged(nameof(SelectedService)); }
        }

        public ObservableCollection<WorkerDto> WorkerList
        {
            get { return _workerList; }
            set { _workerList = value; OnPropertyChanged(nameof(WorkerList)); }
        }

        public WorkerDto SelectedWorker
        {
            get { return _selectedWorker; }
            set { _selectedWorker = value; OnPropertyChanged(nameof(SelectedWorker)); }
        }

        public ObservableCollection<PeriodDto> PeriodList
        {
            get { return _periodList; }
            set { _periodList = value; OnPropertyChanged(nameof(PeriodList)); }
        }

        public PeriodDto SelectedPeriod
        {
            get { return _selectedPeriod; }
            set { _selectedPeriod = value; OnPropertyChanged(nameof(SelectedPeriod)); }
        }

        private void ChangeParentService(int? parentServiceId)
        {
            ServiceList.Clear();
            if (!parentServiceId.HasValue)
                return;
            foreach (var source in _requestService.GetServices(parentServiceId.Value).OrderBy(s => s.Name))
            {
                ServiceList.Add(source);
            }
            OnPropertyChanged(nameof(ServiceList));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
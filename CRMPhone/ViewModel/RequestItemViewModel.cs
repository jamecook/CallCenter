using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CRMPhone.Annotations;
using System.Windows;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class RequestItemViewModel : INotifyPropertyChanged
    {
        private readonly RequestServiceImpl.RequestService _requestService;
        private ObservableCollection<WorkerDto> _workerList;
        private WorkerDto _selectedWorker;
        private ObservableCollection<ServiceDto> _parentServiceList;
        private ServiceDto _selectedParentService;
        private ObservableCollection<ServiceDto> _serviceList;
        private ServiceDto _selectedService;
        private DateTime? _selectedDateTime;

        private bool _isChargeable;
        private bool _isImmediate;
        private ObservableCollection<ServiceCompanyDto> _companyList;
        private ServiceCompanyDto _selectedCompany;
        private int? _requestId;
        private ObservableCollection<PeriodDto> _periodList;
        private PeriodDto _selectedPeriod;
        private string _description;
        private DateTime? _requestDate;
        private string _requestCreator;
        private string _requestState;
        private RequestRatingDto _rating;
        private DateTime? _fromDate;
        private DateTime? _toDate;


        public RequestItemViewModel()
        {
            _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);
            ServiceList = new ObservableCollection<ServiceDto>();
            WorkerList = new ObservableCollection<WorkerDto>(_requestService.GetWorkers(null));
            ParentServiceList = new ObservableCollection<ServiceDto>(_requestService.GetServices(null));
            SelectedParentService = ParentServiceList.FirstOrDefault();
            PeriodList = new ObservableCollection<PeriodDto>(_requestService.GetPeriods());
            SelectedPeriod = PeriodList.FirstOrDefault();
            CompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
            SelectedCompany = CompanyList.FirstOrDefault();
            Rating = new RequestRatingDto();
        }

        public int? RequestId
        {
            get { return _requestId; }
            set { _requestId = value;
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(RequestId)); }
        }

        public RequestRatingDto Rating
        {
            get { return _rating; }
            set { _rating = value; OnPropertyChanged(nameof(Rating)); OnPropertyChanged(nameof(CanAddRating)); OnPropertyChanged(nameof(ShowRating)); }
        }

        public bool CanAddRating { get { return CanEdit && Rating.Id == 0; } }
        public Visibility ShowRating
        {
            get
            {
                return Rating.Id > 0 ? Visibility.Visible : Visibility.Hidden;
            }
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

        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged(nameof(Description)); }
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

        public DateTime? RequestDate
        {
            get { return _requestDate; }
            set { _requestDate = value; OnPropertyChanged(nameof(RequestDate));}
        }

        public string RequestCreator
        {
            get { return _requestCreator; }
            set { _requestCreator = value; OnPropertyChanged(nameof(RequestCreator));}
        }

        public string RequestState
        {
            get { return _requestState; }
            set { _requestState = value; OnPropertyChanged(nameof(RequestState));}
        }

        public bool CanSave
        {
            get
            {
                return RequestId == null;
            }
        }
        public bool CanEdit
        {
            get
            {
                return !CanSave;
            }
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
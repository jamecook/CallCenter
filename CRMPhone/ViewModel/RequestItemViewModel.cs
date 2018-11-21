using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CRMPhone.Annotations;
using System.Windows;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using RudiGrobler.Calendar.Common;

namespace CRMPhone.ViewModel
{
    public class RequestItemViewModel : INotifyPropertyChanged
    {
        private readonly RequestServiceImpl.RequestService _requestService;
        private ObservableCollection<WorkerDto> _masterList;
        private ObservableCollection<WorkerDto> _executerList;
        private WorkerDto _selectedMaster;
        private WorkerDto _selectedExecuter;
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
        private bool _isBadWork;
        private DateTime? _alertTime;
        private bool _gatanty;
        private bool _isRetry;
        private ObservableCollection<EquipmentDto> _equipmentList;
        private EquipmentDto _selectedEquipment;
        private bool _showAllMasters;
        private bool _showAllExecuters;
        private int? _selectedHouseId;
        private DateTime? _termOfExecution;
        private ObservableCollection<GarantyDto> _garantyList;
        private GarantyDto _selectedGaranty;


        public RequestItemViewModel()
        {
            _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);
            ServiceList = new ObservableCollection<ServiceDto>();
            MasterList = new ObservableCollection<WorkerDto>(_requestService.GetMasters(null));
            ExecuterList = new ObservableCollection<WorkerDto>(_requestService.GetExecuters(null));
            EquipmentList = new ObservableCollection<EquipmentDto>(_requestService.GetEquipments());
            ParentServiceList = new ObservableCollection<ServiceDto>(_requestService.GetServices(null));
            SelectedParentService = ParentServiceList.FirstOrDefault();
            PeriodList = new ObservableCollection<PeriodDto>(_requestService.GetPeriods());
            SelectedPeriod = PeriodList.FirstOrDefault();
            CompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
            SelectedCompany = CompanyList.FirstOrDefault();
            Rating = new RequestRatingDto();
            GarantyList = new ObservableCollection<GarantyDto>(new GarantyDto[] {
                new GarantyDto{Id=0,Name = "Обычная"},
            //new GarantyDto{Id=2,Name = "Вероятно гарантия"},
            new GarantyDto{Id=1,Name = "Гарантия"},
            });
            SelectedGaranty = GarantyList.FirstOrDefault();
        }

        private Appointment _selectedAppointment;
        public Appointment OpenAppointment { get; set; }

        public Appointment SelectedAppointment
        {
            get { return _selectedAppointment; }
            set { _selectedAppointment = value; OnPropertyChanged(nameof(SelectedAppointment)); }
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

        public bool CanAddRating { get { return CanEdit /*&& Rating.Id == 0*/; } }
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

        public int? SelectedHouseId
        {
            get { return _selectedHouseId; }
            set
            {
                _selectedHouseId = value;
                UpdateMastets();
                OnPropertyChanged(nameof(SelectedHouseId));
            }
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

        public bool IsRetry
        {
            get { return _isRetry; }
            set { _isRetry = value; OnPropertyChanged(nameof(IsRetry));}
        }

        public bool IsBadWork
        {
            get { return _isBadWork; }
            set { _isBadWork = value; OnPropertyChanged(nameof(IsBadWork));}
        }

        public ObservableCollection<GarantyDto> GarantyList
        {
            get { return _garantyList; }
            set { _garantyList = value; OnPropertyChanged(nameof(GarantyList)); }
        }

        public GarantyDto SelectedGaranty
        {
            get { return _selectedGaranty; }
            set { _selectedGaranty = value; OnPropertyChanged(nameof(SelectedGaranty)); }
        }

        public bool Gatanty
        {
            get { return _gatanty; }
            set { _gatanty = value; OnPropertyChanged(nameof(Gatanty));}
        }

        public bool IsImmediate
        {
            get { return _isImmediate; }
            set { _isImmediate = value; OnPropertyChanged(nameof(IsImmediate)); }
        }

        public bool ShowAllMasters
        {
            get { return _showAllMasters; }
            set
            {

                _showAllMasters = value;
                UpdateMastets();
                OnPropertyChanged(nameof(ShowAllMasters));
            }
        }

        private void UpdateMastets()
        {
            var selectedMaster = SelectedMaster?.Id;
            //Какой - то магический кастыль. Иногда Clear не очищает список, а делает первый и единственный элемент = null
            var i = 0;
            do
            {
                MasterList.Clear();
                i++;
            } while (MasterList.Count > 0 && i < 10);
            if (_showAllMasters)
            {
                foreach (var master in _requestService.GetMasters(null))
                {
                    MasterList.Add(master);
                }
                SelectedMaster = MasterList.FirstOrDefault(m => m.Id == selectedMaster);
            }
            else
            {
                if (_selectedHouseId.HasValue)
                {
                    foreach (var master in _requestService.GetMastersByHouseAndService(_selectedHouseId.Value, SelectedParentService.Id))
                    {
                        MasterList.Add(master);
                    }
                    SelectedMaster = MasterList.FirstOrDefault();
                }
            }

        }

        public bool ShowAllExecuters
        {
            get { return _showAllExecuters; }
            set { _showAllExecuters = value; OnPropertyChanged(nameof(ShowAllExecuters));}
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public DateTime? AlertTime
        {
            get { return _alertTime; }
            set { _alertTime = value; OnPropertyChanged(nameof(AlertTime));}
        }

        public ServiceDto SelectedParentService
        {
            get { return _selectedParentService; }
            set
            {
                _selectedParentService = value;
                ChangeParentService(value?.Id);
                UpdateMastets();
                OnPropertyChanged(nameof(SelectedParentService));
            }
        }

        public DateTime? SelectedDateTime
        {
            get { return _selectedDateTime; }
            set { _selectedDateTime = value; OnPropertyChanged(nameof(SelectedDateTime)); }
        }

        public DateTime? TermOfExecution
        {
            get { return _termOfExecution; }
            set { _termOfExecution = value; OnPropertyChanged(nameof(TermOfExecution));}
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

        public ObservableCollection<WorkerDto> MasterList
        {
            get { return _masterList; }
            set { _masterList = value; OnPropertyChanged(nameof(MasterList)); }
        }

        public ObservableCollection<EquipmentDto> EquipmentList
        {
            get { return _equipmentList; }
            set { _equipmentList = value; OnPropertyChanged(nameof(EquipmentList));}
        }

        public ObservableCollection<WorkerDto> ExecuterList
        {
            get { return _executerList; }
            set { _executerList = value; OnPropertyChanged(nameof(ExecuterList)); }
        }

        public WorkerDto SelectedMaster
        {
            get { return _selectedMaster; }
            set { _selectedMaster = value;
                if (value!= null && !MasterList.Contains(_selectedMaster))
                {
                    MasterList.Add(_selectedMaster);
                }
                OnPropertyChanged(nameof(SelectedMaster)); }
        }

        public EquipmentDto SelectedEquipment
        {
            get { return _selectedEquipment; }
            set { _selectedEquipment = value; OnPropertyChanged(nameof(SelectedEquipment));}
        }

        public WorkerDto SelectedExecuter
        {
            get { return _selectedExecuter; }
            set { _selectedExecuter = value; OnPropertyChanged(nameof(SelectedExecuter)); }
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
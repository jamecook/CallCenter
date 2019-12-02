using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class BindCompanyToWorkerDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _workerId;
        private ICommand _addCommand;
        private ICommand _deleteCommand;
        private ObservableCollection<ServiceCompanyDto> _companyList;
        private ServiceCompanyDto _selectedCompany;
        private ObservableCollection<SpecialityDto> _specialityList;
        private SpecialityDto _selectedSpeciality;
        private ObservableCollection<WorkerCompanyDto> _bindedCompanyList;
        private WorkerCompanyDto _selectedBindedCompany;


        public BindCompanyToWorkerDialogViewModel(RequestServiceImpl.RequestService requestService, int workerId)
        {
            _requestService = requestService;
            _workerId = workerId;
            SpecialityList = new ObservableCollection<SpecialityDto>(_requestService.GetSpecialities());
            CompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
            RefreshList();
            if (CompanyList.Count > 0)
            {
                SelectedCompany = CompanyList.FirstOrDefault();
            }

        }

        public void SetView(Window view)
        {
            _view = view;
        }

        public ObservableCollection<ServiceCompanyDto> CompanyList
        {
            get { return _companyList; }
            set { _companyList = value; OnPropertyChanged(nameof(CompanyList)); }
        }

        public void RefreshList()
        {
            BindedCompanyList = new ObservableCollection<WorkerCompanyDto>(_requestService.GetBindedToWorkerCompany(_workerId));
        }
        public ServiceCompanyDto SelectedCompany
        {
            get { return _selectedCompany; }
            set
            {
                _selectedCompany = value;
                OnPropertyChanged(nameof(SelectedCompany));
            }
        }

        public ObservableCollection<SpecialityDto> SpecialityList
        {
            get { return _specialityList; }
            set { _specialityList = value; OnPropertyChanged(nameof(SpecialityList)); }
        }

        public SpecialityDto SelectedSpeciality
        {
            get { return _selectedSpeciality; }
            set
            {
                _selectedSpeciality = value;
                OnPropertyChanged(nameof(SelectedSpeciality));
            }
        }


        public ObservableCollection<WorkerCompanyDto> BindedCompanyList
        {
            get { return _bindedCompanyList; }
            set { _bindedCompanyList = value; OnPropertyChanged(nameof(BindedCompanyList)); }
        }

        public WorkerCompanyDto SelectedBindedCompany
        {
            get { return _selectedBindedCompany; }
            set { _selectedBindedCompany = value; OnPropertyChanged(nameof(SelectedBindedCompany)); }
        }

        private void Delete(object sender)
        {
            var item = sender as WorkerCompanyDto;
            if (item is null)
                return;
            if (MessageBox.Show(_view, $"Вы уверены что хотите удалить запись?", "Привязка", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _requestService.DeleteBindedToWorkerCompany(_workerId, item.Id);
                RefreshList();
            }
        }
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(Delete)); } }


        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(AddCompany)); } }
        private void AddCompany(object obj)
        {
            try
            {
                _requestService.AddBindedToWorkerCompany(_workerId, SelectedCompany.Id, SelectedSpeciality?.Id);
            }
            catch
            {
            }
            RefreshList();
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
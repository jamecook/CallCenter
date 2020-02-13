using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class BindWorkerToWorkerDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _workerId;
        private ICommand _addCommand;
        private ICommand _deleteCommand;
        private ObservableCollection<ServiceCompanyDto> _companyList;
        private ServiceCompanyDto _selectedCompany;
        private ObservableCollection<WorkerDto> _workerList;
        private WorkerDto _selectedWorker;
        private ObservableCollection<WorkerDto> _bindedWorkerList;
        private WorkerDto _selectedBindedWorker;


        public BindWorkerToWorkerDialogViewModel(RequestServiceImpl.RequestService requestService, int workerId)
        {
            _requestService = requestService;
            _workerId = workerId;
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
            BindedWorkerList = new ObservableCollection<WorkerDto>(_requestService.GetBindedWorkers(_workerId));
        }
        public ServiceCompanyDto SelectedCompany
        {
            get { return _selectedCompany; }
            set
            {
                _selectedCompany = value;
                if (_selectedCompany != null)
                {
                    WorkerList = new ObservableCollection<WorkerDto>(_requestService.GetAllWorkers(_selectedCompany.Id));

                }
                else
                {
                    WorkerList = new ObservableCollection<WorkerDto>();
                }
                OnPropertyChanged(nameof(SelectedCompany));
            }
        }

        public ObservableCollection<WorkerDto> WorkerList
        {
            get { return _workerList; }
            set { _workerList = value; OnPropertyChanged(nameof(WorkerList)); }
        }

        public WorkerDto SelectedWorker
        {
            get { return _selectedWorker; }
            set
            {
                _selectedWorker = value;
                OnPropertyChanged(nameof(SelectedWorker));
            }
        }


        public ObservableCollection<WorkerDto> BindedWorkerList
        {
            get { return _bindedWorkerList; }
            set { _bindedWorkerList = value; OnPropertyChanged(nameof(BindedWorkerList)); }
        }

        public WorkerDto SelectedBindedWorker
        {
            get { return _selectedBindedWorker; }
            set { _selectedBindedWorker = value; OnPropertyChanged(nameof(SelectedBindedWorker)); }
        }

        private void Delete(object sender)
        {
            var item = sender as WorkerDto;
            if (item is null)
                return;
            if (MessageBox.Show(_view, $"Вы уверены что хотите удалить запись?", "Привязка", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _requestService.DeleteBindedWorker(_workerId, item.Id);
                RefreshList();
            }
        }
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(Delete)); } }


        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(AddCompany)); } }
        private void AddCompany(object obj)
        {
            try
            {
                _requestService.BindWorkerToWorker(_workerId, SelectedWorker.Id);
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
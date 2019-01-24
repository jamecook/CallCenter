using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class EditExecuterBindDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _serviceCompanyId;
        private ICommand _deleteCommand;
        private ICommand _addCommand;
        private ICommand _closeCommand;

        private ObservableCollection<WorkerDto> _executerList;
        private WorkerDto _selectedExecuter;
        private ObservableCollection<RequestRatingListDto> _requestRatingHistory;
        private ObservableCollection<ServiceDto> _typeList;
        private ServiceDto _selectedType;
        private ObservableCollection<ExecuterToServiceCompanyDto> _executerBinding;
        private ExecuterToServiceCompanyDto _selectedBinding;
        private int _weigth;

        public EditExecuterBindDialogViewModel(RequestServiceImpl.RequestService requestService,int serviceCompanyId)
        {
            _requestService = requestService;
            _serviceCompanyId = serviceCompanyId;
            ExecuterList = new ObservableCollection<WorkerDto>(_requestService.GetExecuters(null));
            TypeList = new ObservableCollection<ServiceDto>(_requestService.GetServices(null));
            Weigth = 100;
            Refresh();
        }

        public void Refresh()
        {
            ExecuterBinding = new ObservableCollection<ExecuterToServiceCompanyDto>(_requestService.LoadExecuterBinding(_serviceCompanyId));

        }
        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(Add)); } }
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(Drop)); } }

        private void Drop(object obj)
        {
            var bind = obj as ExecuterToServiceCompanyDto;
            if(bind == null)
                return;
            _requestService.DropExecuterBinding(bind.Id);
            Refresh();
        }

        private void Add(object sender)
        {
            if(SelectedExecuter == null || SelectedType == null)
                return;
            
            _requestService.AddExecuterBinding(_serviceCompanyId,SelectedType.Id,SelectedExecuter.Id, Weigth);
            Refresh();
        }

        public int Weigth
        {
            get { return _weigth; }
            set { _weigth = value; OnPropertyChanged(nameof(Weigth));}
        }

        public ICommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(Close)); } }

        private void Close(object obj)
        {
            _view.DialogResult = false;
        }

        public ObservableCollection<ExecuterToServiceCompanyDto> ExecuterBinding
        {
            get { return _executerBinding; }
            set { _executerBinding = value; OnPropertyChanged(nameof(ExecuterBinding)); }
        }

        public ExecuterToServiceCompanyDto SelectedBinding
        {
            get { return _selectedBinding; }
            set { _selectedBinding = value; OnPropertyChanged(nameof(SelectedBinding));}
        }

        public ObservableCollection<ServiceDto> TypeList
        {
            get { return _typeList; }
            set { _typeList = value; OnPropertyChanged(nameof(TypeList));}
        }

        public ServiceDto SelectedType
        {
            get { return _selectedType; }
            set { _selectedType = value; OnPropertyChanged(nameof(SelectedType));}
        }

        public ObservableCollection<WorkerDto> ExecuterList
        {
            get { return _executerList; }
            set { _executerList = value; OnPropertyChanged(nameof(ExecuterList)); }
        }


        public WorkerDto SelectedExecuter
        {
            get { return _selectedExecuter; }
            set { _selectedExecuter = value; OnPropertyChanged(nameof(SelectedExecuter)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class ExecuterBindDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _serviceCompanyId;
        private ObservableCollection<ExecuterToServiceCompanyDto> _bindList;
        private ExecuterToServiceCompanyDto _selectedBindItem;


        public ExecuterBindDialogViewModel(RequestServiceImpl.RequestService requestService, int serviceCompanyId)
        {
            _requestService = requestService;
            _serviceCompanyId = serviceCompanyId;
            Refresh();
        }

        public ObservableCollection<ExecuterToServiceCompanyDto> BindList
        {
            get { return _bindList; }
            set { _bindList = value; OnPropertyChanged(nameof(BindList));}
        }
        public void Refresh()
        {
            BindList = new ObservableCollection<ExecuterToServiceCompanyDto>(_requestService.LoadExecuterBinding(_serviceCompanyId));
        }


        private ICommand _addCommand;
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new CommandHandler(Add, true)); } }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(Delete, true)); } }


        private ICommand _refreshCommand;
        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new CommandHandler(Refresh, true)); } }

        private void Add()
        {
            var openDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Все файлы|*.*"
            };
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Refresh();
            }

        }
        private void Delete()
        {
            if (SelectedBindItem != null)
            {
                _requestService.DeleteAttachment(SelectedBindItem.Id);
                Refresh();
            }
        }

        public ExecuterToServiceCompanyDto SelectedBindItem
        {
            get { return _selectedBindItem; }
            set { _selectedBindItem = value; OnPropertyChanged(nameof(SelectedBindItem));}
        }


        public void SetView(Window view)
        {
            _view = view;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
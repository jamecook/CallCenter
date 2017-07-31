using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dialogs;
using CRMPhone.Dialogs.Admins;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class WorkerAdminControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<WorkerDto> _workersList;
        public ObservableCollection<WorkerDto> WorkersList
        {
            get => _workersList;
            set { _workersList = value; OnPropertyChanged(nameof(WorkersList)); }
        }

        private RequestService _requestService;
        private ICommand _addNewCommand;
        public ICommand AddNewCommand { get { return _addNewCommand ?? (_addNewCommand = new CommandHandler(AddWorker, true)); } }
        private ICommand _editCommand;
        public ICommand EditCommand { get { return _editCommand ?? (_editCommand = new RelayCommand(EditCompany)); } }

        public WorkerAdminControlContext()
        {
            WorkersList = new ObservableCollection<WorkerDto>();
        }
        private void EditCompany(object sender)
        {
            var selectedItem = sender as WorkerDto;
            if (selectedItem == null)
                return;
            if (_requestService == null)
                _requestService = new RequestService(AppSettings.DbConnection);

            var model = new WorkerAdminDialogViewModel(_requestService, selectedItem.Id);
            var view = new WorkerAddOrEditDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                RefreshList();
            }
        }

        public void RefreshList()
        {
            if (_requestService == null)
                _requestService = new RequestService(AppSettings.DbConnection);
            WorkersList.Clear();

            _requestService.GetWorkers(null).ToList().ForEach(w => WorkersList.Add(w));

            OnPropertyChanged(nameof(WorkersList));
        }

        private void AddWorker()
        {
            var model = new WorkerAdminDialogViewModel(_requestService, null);
            var view = new WorkerAddOrEditDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                RefreshList();
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
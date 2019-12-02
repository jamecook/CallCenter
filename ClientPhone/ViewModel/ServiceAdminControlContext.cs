using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dialogs.Admins;
using CRMPhone.ViewModel.Admins;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class ServiceAdminControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<ServiceDto> _parentServiceList;
        public ObservableCollection<ServiceDto> ParentServiceList
        {
            get => _parentServiceList;
            set { _parentServiceList = value; OnPropertyChanged(nameof(ParentServiceList)); }
        }
        private ServiceDto _selectedParentService;
        public ServiceDto SelectedParentService
        {
            get { return _selectedParentService; }
            set { _selectedParentService = value;
                RefreshServiceList();
                OnPropertyChanged(nameof(SelectedParentService));}
        }
        private ObservableCollection<ServiceDto> _serviceList;
        public ObservableCollection<ServiceDto> ServiceList
        {
            get => _serviceList;
            set { _serviceList = value; OnPropertyChanged(nameof(ServiceList)); }
        }
        private ServiceDto _selectedService;
        public ServiceDto SelectedService
        {
            get { return _selectedService; }
            set {
                _selectedService = value;
                OnPropertyChanged(nameof(SelectedService));}
        }

        private RequestService _requestService;
        private RequestService RequestService => _requestService ?? (_requestService = new RequestService(AppSettings.DbConnection));

        private ICommand _addNewParentServiceCommand;
        public ICommand AddNewParentServiceCommand { get { return _addNewParentServiceCommand ?? (_addNewParentServiceCommand = new CommandHandler(AddParentService, true)); } }
        private ICommand _deleteParentServiceCommand;
        public ICommand DeleteParentServiceCommand { get { return _deleteParentServiceCommand ?? (_deleteParentServiceCommand = new CommandHandler(DeleteParentService, true)); } }
        private ICommand _editParentServiceCommand;
        public ICommand EditParentServiceCommand { get { return _editParentServiceCommand ?? (_editParentServiceCommand = new RelayCommand(EditParentService)); } }

        private ICommand _addNewServiceCommand;
        public ICommand AddNewServiceCommand { get { return _addNewServiceCommand ?? (_addNewServiceCommand = new CommandHandler(AddService, true)); } }

        private ICommand _deleteServiceCommand;
        public ICommand DeleteServiceCommand { get { return _deleteServiceCommand ?? (_deleteServiceCommand = new CommandHandler(DeleteService, true)); } }

        private ICommand _ediServiceCommand;
        public ICommand EditServiceCommand { get { return _ediServiceCommand ?? (_ediServiceCommand = new RelayCommand(EditService)); } }

        private void DeleteService()
        {
            if (SelectedService != null)
            {
                if (MessageBox.Show(Application.Current.MainWindow,
                        $"Вы действительно хотите удалить причину {SelectedService.Name}", "Удалить",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    RequestService.DeleteService(SelectedService.Id);
                    RefreshServiceList();
                }
            }
        }

        private void DeleteParentService()
        {
            if (SelectedParentService != null)
            {
                if (MessageBox.Show(Application.Current.MainWindow,
                        $"Вы действительно хотите удалить услугу {SelectedParentService.Name}", "Удалить",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    RequestService.DeleteService(SelectedParentService.Id);
                    RefreshParentServiceList();
                }
            }
            
        }
        public ServiceAdminControlContext()
        {
            ParentServiceList = new ObservableCollection<ServiceDto>();
            ServiceList = new ObservableCollection<ServiceDto>();
        }
        private void AddParentService()
        {
            ShowEditDialog(null,null);
        }
        private void EditParentService(object sender)
        {
            var selectedItem = sender as ServiceDto;
            if (selectedItem == null)
                return;
            
            ShowEditDialog(selectedItem, null);
        }
        private void AddService()
        {
            if (SelectedParentService != null)
            {
                ShowEditDialog(null, SelectedParentService.Id);
            }
        }
        private void EditService(object sender)
        {
            var selectedItem = sender as ServiceDto;
            if (selectedItem == null)
                return;
            if (SelectedParentService != null)
            {
                ShowEditDialog(selectedItem, SelectedParentService.Id);
            }
        }

        private void ShowEditDialog(ServiceDto selectedItem, int? parentId)
        {
            var model = new ServiceDialogViewModel(RequestService, selectedItem?.Id, parentId);
            var view = new ServiceAddOrEditDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                if (parentId == null)
                {
                    RefreshParentServiceList();
                }
                else
                {
                    RefreshServiceList();
                }
            }
        }

        public void RefreshParentServiceList()
        {
            ParentServiceList.Clear();

            RequestService.GetServices(null).ToList().ForEach(w => ParentServiceList.Add(w));

            OnPropertyChanged(nameof(ParentServiceList));
        }

        private void RefreshServiceList()
        {
            ServiceList.Clear();
            if (SelectedParentService != null)
            {
                RequestService.GetServices(SelectedParentService.Id).ToList().ForEach(w => ServiceList.Add(w));
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
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dialogs;
using CRMPhone.ViewModel.Admins;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using ServiceCompanyAddOrEditDialog = CRMPhone.Dialogs.Admins.ServiceCompanyAddOrEditDialog;

namespace CRMPhone.ViewModel
{
    public class ServiceCompanyControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<ServiceCompanyDto> _serviceCompanyList;
        public ObservableCollection<ServiceCompanyDto> ServiceCompanyList
        {
            get { return _serviceCompanyList; }
            set { _serviceCompanyList = value; OnPropertyChanged(nameof(ServiceCompanyList)); }
        }

        private RequestService _requestService;
        private ICommand _addNewCommand;
        public ICommand AddNewCommand { get { return _addNewCommand ?? (_addNewCommand = new CommandHandler(AddCompany, true)); } }
        private ICommand _editCommand;
        public ICommand EditCommand { get { return _editCommand ?? (_editCommand = new RelayCommand(EditCompany)); } }

        public ServiceCompanyControlContext()
        {
            ServiceCompanyList = new ObservableCollection<ServiceCompanyDto>();
        }
        private void EditCompany(object sender)
        {
            var selectedItem = sender as ServiceCompanyDto;
            if (selectedItem == null)
                return;
            if (_requestService == null)
                _requestService = new RequestService(AppSettings.DbConnection);

            var model = new ServiceCompanyDialogViewModel(_requestService, selectedItem.Id);
            var view = new ServiceCompanyAddOrEditDialog();
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
            ServiceCompanyList.Clear();

            _requestService.GetServiceCompanies().ForEach(c => ServiceCompanyList.Add(c));
            
            OnPropertyChanged(nameof(ServiceCompanyList));
        }

        private void AddCompany()
        {
            var model = new ServiceCompanyDialogViewModel(_requestService, null);
            var view = new ServiceCompanyAddOrEditDialog();
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
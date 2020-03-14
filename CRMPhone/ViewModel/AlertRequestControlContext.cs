using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using CRMPhone.Annotations;
using Microsoft.Win32;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class AlertRequestControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<RequestForListDto> _requestList;
        private RequestService _requestService;

        private ICommand _refreshRequestCommand;
        public ICommand RefreshRequestCommand { get { return _refreshRequestCommand ?? (_refreshRequestCommand = new CommandHandler(RefreshRequest, true)); } }
        private ICommand _clearFiltersCommand;
        public ICommand ClearFiltersCommand { get { return _clearFiltersCommand ?? (_clearFiltersCommand = new CommandHandler(ClearFilters, true)); } }

        private void ClearFilters()
        {
            _serviceCompanyList = null;
            _showDoned = false;
            OnPropertyChanged(nameof(ServiceCompanyList));
            OnPropertyChanged(nameof(ShowDoned));
            RefreshRequest();
        }

        public bool ShowDoned
        {
            get { return _showDoned; }
            set { _showDoned = value; OnPropertyChanged(nameof(ShowDoned)); RefreshRequest();}
        }

        public ServiceCompanyDto SelectedServiceCompany
        {
            get { return _selectedServiceCompany; }
            set { _selectedServiceCompany = value; OnPropertyChanged(nameof(SelectedServiceCompany)); RefreshRequest(); }
        }

        public ObservableCollection<ServiceCompanyDto> ServiceCompanyList
        {
            get { return _serviceCompanyList; }
            set { _serviceCompanyList = value; OnPropertyChanged(nameof(ServiceCompanyList)); }
        }

        private int _requestCount;

        private ICommand _openRequestCommand;
        private ServiceCompanyDto _selectedServiceCompany;
        private ObservableCollection<ServiceCompanyDto> _serviceCompanyList;
        private bool _showDoned;
        public ICommand OpenRequestCommand { get { return _openRequestCommand ?? (_openRequestCommand = new RelayCommand(OpenRequest));} }


        private void OpenRequest(object sender)
        {
            var selectedItem = sender as RequestForListDto;
            if (selectedItem == null)
                return;
            var request = _requestService.GetRequest(selectedItem.Id);
            if (request == null)
            {
                MessageBox.Show("Произошла непредвиденная ошибка!");
                return;
            }

            var viewModel = new RequestDialogViewModel(request);
            var view = new RequestDialog(viewModel);
            viewModel.SetView(view);
            
            view.Show();

        }

        private void RefreshRequest()
        {
            RequestList.Clear();
            var requests = _requestService.GetAlertRequestList(SelectedServiceCompany?.Id, ShowDoned).ToArray();
            foreach (var request in requests)
            {
                RequestList.Add(request);
            }
            RequestCount = RequestList.Count;
            OnPropertyChanged(nameof(RequestList));
        }

        public int RequestCount
        {
            get { return _requestCount; }
            set { _requestCount = value; OnPropertyChanged(nameof(RequestCount));}
        }

        public AlertRequestControlContext()
        {
            RequestList = new ObservableCollection<RequestForListDto>();
        }

        public void InitCollections()
        {
            _requestService = new RequestService(AppSettings.DbConnection);
            ServiceCompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
            RefreshRequest();
        }

        public ObservableCollection<RequestForListDto> RequestList
        {
            get { return _requestList; }
            set { _requestList = value; OnPropertyChanged(nameof(RequestList));}
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
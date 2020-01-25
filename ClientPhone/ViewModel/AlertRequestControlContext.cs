using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ClientPhone.Services;
using CRMPhone.Annotations;
using Microsoft.Win32;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class AlertRequestControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<RequestForListShortDto> _requestList;

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
        /*
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

            var viewModel = new RequestDialogViewModel();
            var view = new RequestDialog(viewModel);
            viewModel.SetView(view);
            viewModel.SelectedCity = viewModel.CityList.SingleOrDefault(i=>i.Id == request.Address.CityId);
            viewModel.SelectedStreet = viewModel.StreetList.SingleOrDefault(i => i.Id == request.Address.StreetId);
            viewModel.StreetName = request.Address.StreetName;
            viewModel.SelectedHouse = viewModel.HouseList.SingleOrDefault(i=>i.Id == request.Address.HouseId);
            if (viewModel.FlatList.All(i => i.Id != request.Address.Id))
            {
                viewModel.FlatList.Add(new FlatDto()
                {
                    Id = request.Address.Id,
                    Flat = request.Address.Flat,
                    TypeId = request.Address.TypeId,
                    TypeName = request.Address.Type
                });
            }
            viewModel.SelectedFlat =  viewModel.FlatList.SingleOrDefault(i=>i.Id == request.Address.Id);
            viewModel.Floor = request.Floor;
            viewModel.Entrance = request.Entrance;
            viewModel.FromTime = request.FromTime;
            viewModel.ToTime = request.ToTime;
            var requestModel = viewModel.RequestList.FirstOrDefault();
            requestModel.SelectedParentService = requestModel.ParentServiceList.FirstOrDefault(i => i.Id == request.Type.ParentId);
            requestModel.SelectedService = requestModel.ServiceList.FirstOrDefault(i => i.Id == request.Type.Id);
            requestModel.Description = request.Description;
            requestModel.IsChargeable = request.IsChargeable;
            requestModel.IsImmediate = request.IsImmediate;
            requestModel.RequestCreator = request.CreateUser.ShortName;
            requestModel.RequestDate = request.CreateTime;
            requestModel.RequestState = request.State.Description;
            requestModel.SelectedMaster = request.MasterId.HasValue ? _requestService.GetWorkerById(request.MasterId.Value) : null;
            requestModel.SelectedExecuter = request.ExecuterId.HasValue ? _requestService.GetWorkerById(request.ExecuterId.Value) : null;
            requestModel.RequestId = request.Id;
            requestModel.Rating = request.Rating;
            if (request.ServiceCompanyId.HasValue)
            {
                requestModel.SelectedCompany = requestModel.CompanyList.FirstOrDefault(c => c.Id == request.ServiceCompanyId.Value);
            }
            if (request.ExecuteDate.HasValue && request.ExecuteDate.Value.Date > DateTime.MinValue)
            {
                requestModel.SelectedDateTime = request.ExecuteDate.Value.Date;
                requestModel.SelectedPeriod = requestModel.PeriodList.SingleOrDefault(i => i.Id == request.PeriodId);
            }
            requestModel.TermOfExecution = request.TermOfExecution;
            viewModel.RequestId = request.Id;
            viewModel.ContactList = new ObservableCollection<ContactDto>(request.Contacts);
            view.Show();

        }
        */

        private void RefreshRequest()
        {
            RequestList.Clear();
             var requests = RestRequestService.GetAlertRequests(AppSettings.CurrentUser.Id);
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
            RequestList = new ObservableCollection<RequestForListShortDto>();
        }

        public void InitCollections()
        {
            //ServiceCompanyList = new ObservableCollection<ServiceCompanyDto>(RestRequestService.GetFilterServiceCompanies(AppSettings.CurrentUser.Id));
            RefreshRequest();
        }

        public ObservableCollection<RequestForListShortDto> RequestList
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
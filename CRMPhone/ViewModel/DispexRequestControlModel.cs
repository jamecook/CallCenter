using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class DispexRequestControlModel : INotifyPropertyChanged
    {
        private ObservableCollection<DispexForListDto> _requestList;
        private RequestServiceImpl.RequestService _requestService;

        private ICommand _refreshRequestCommand;
        public ICommand RefreshRequestCommand { get { return _refreshRequestCommand ?? (_refreshRequestCommand = new CommandHandler(RefreshRequest, true)); } }

        private void RefreshRequest()
        {
            if (_requestService == null)
                _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);
            RequestList.Clear();
            var requests = _requestService.GetDispexRequests();
            foreach (var request in requests)
            {
                RequestList.Add(request);
            }
            RequestCount = RequestList.Count;
            OnPropertyChanged(nameof(RequestList));
        }

        private ICommand _openRequestCommand;
        private int _requestCount;
        public ICommand OpenRequestCommand { get { return _openRequestCommand ?? (_openRequestCommand = new RelayCommand(OpenRequest)); } }

        private void OpenRequest(object sender)
        {
            return;
            var selectedItem = sender as RequestForListDto;
            if (selectedItem == null)
                return;
            if (_requestService == null)
                _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);

            var request = _requestService.GetRequest(selectedItem.Id);
            if (request == null)
            {
                MessageBox.Show("Произошла непредвиденная ошибка!");
                return;
            }

            var viewModel = new RequestDialogViewModel();
            var view = new RequestDialog(viewModel);
            viewModel.SetView(view);
            viewModel.RequestId = request.Id;
            viewModel.SelectedCity = viewModel.CityList.SingleOrDefault(i => i.Id == request.Address.CityId);
            viewModel.SelectedStreet = viewModel.StreetList.SingleOrDefault(i => i.Id == request.Address.StreetId);
            viewModel.SelectedHouse = viewModel.HouseList.SingleOrDefault(i => i.Id == request.Address.HouseId);
            viewModel.SelectedFlat = viewModel.FlatList.SingleOrDefault(i => i.Id == request.Address.Id);
            viewModel.Floor = request.Floor;
            viewModel.Entrance = request.Entrance;
            viewModel.FromTime = request.FromTime;
            viewModel.ToTime = request.ToTime;
            var requestModel = viewModel.RequestList.FirstOrDefault();
            requestModel.SelectedParentService = requestModel.ParentServiceList.SingleOrDefault(i => i.Id == request.Type.ParentId);
            requestModel.SelectedService = requestModel.ServiceList.SingleOrDefault(i => i.Id == request.Type.Id);
            requestModel.Description = request.Description;
            requestModel.IsChargeable = request.IsChargeable;
            requestModel.IsImmediate = request.IsImmediate;
            requestModel.IsBadWork = request.IsBadWork;
            requestModel.IsRetry = request.IsRetry;
            requestModel.Gatanty = request.Garanty;
            requestModel.RequestCreator = request.CreateUser.ShortName;
            requestModel.RequestDate = request.CreateTime;
            requestModel.RequestState = request.State.Description;
            requestModel.SelectedMaster = requestModel.MasterList.SingleOrDefault(w => w.Id == request.MasterId);
            requestModel.SelectedExecuter = requestModel.ExecuterList.SingleOrDefault(w => w.Id == request.ExecuterId);
            requestModel.RequestId = request.Id;
            requestModel.Rating = request.Rating;
            requestModel.AlertTime = request.AlertTime;
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
            viewModel.ContactList = new ObservableCollection<ContactDto>(request.Contacts);
            view.Show();

        }

        public int RequestCount
        {
            get { return _requestCount; }
            set { _requestCount = value; OnPropertyChanged(nameof(RequestCount)); }
        }

        public DispexRequestControlModel()
        {
            RequestList = new ObservableCollection<DispexForListDto>();
        }

        public void InitCollections()
        {
            _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);
        }
        public ObservableCollection<DispexForListDto> RequestList
        {
            get { return _requestList; }
            set { _requestList = value; OnPropertyChanged(nameof(RequestList)); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
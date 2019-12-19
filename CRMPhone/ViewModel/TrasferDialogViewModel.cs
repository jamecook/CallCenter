using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class TrasferDialogViewModel : INotifyPropertyChanged
    {
        private TransferIntoDto _clientPhone;
        private Window _view;
        private RequestServiceImpl.RequestService _requestService;
        private ServiceCompanyDto _selectedOutgoingCompany;
        private ObservableCollection<ServiceCompanyDto> _forOutcoinCallsCompanyList;


        private bool _canExecute = true;
        private ICommand _transferCommand;
        private string _transferPhone;
        private ObservableCollection<TransferIntoDto> _phonesList;
        public ICommand TransferCommand { get { return _transferCommand ?? (_transferCommand = new CommandHandler(Transfer, _canExecute)); } }

        private void Transfer()
        {
            _view.DialogResult = true;
        }
        public ObservableCollection<ServiceCompanyDto> ForOutcoinCallsCompanyList
        {
            get { return _forOutcoinCallsCompanyList; }
            set { _forOutcoinCallsCompanyList = value; OnPropertyChanged(nameof(ForOutcoinCallsCompanyList)); }
        }

        public ServiceCompanyDto SelectedOutgoingCompany
        {
            get { return _selectedOutgoingCompany; }
            set { _selectedOutgoingCompany = value;
                UpdatePhones(value);
                OnPropertyChanged(nameof(SelectedOutgoingCompany)); }
        }

        private void UpdatePhones(ServiceCompanyDto value)
        {
            if (value == null)
            {
                PhonesList?.Clear();
            }
            else
            {
                PhonesList = new ObservableCollection<TransferIntoDto>(_requestService.GetTransferList(value.Id));
            }

            ClientPhone = PhonesList?.FirstOrDefault();
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public TrasferDialogViewModel(int? companyId)
        {
            _requestService = new RequestService(AppSettings.DbConnection);
            ForOutcoinCallsCompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompaniesForCalls());
            SelectedOutgoingCompany = ForOutcoinCallsCompanyList.FirstOrDefault(i=>i.Id == companyId);
        }

        public ObservableCollection<TransferIntoDto> PhonesList
        {
            get => _phonesList;
            set
            {
                _phonesList = value;
                OnPropertyChanged(nameof(PhonesList));
            }
        }

        public string TransferPhone
        {
            get { return _transferPhone; }
            set { _transferPhone = value; OnPropertyChanged(nameof(TransferPhone));}
        }

        public TransferIntoDto ClientPhone
        {
            get { return _clientPhone; }
            set
            {
                _clientPhone = value;
                OnPropertyChanged(nameof(ClientPhone));
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
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using CRMPhone.Controls.Admins;
using CRMPhone.Dialogs.Admins;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class HouseTypeAddAndEditDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private ICommand _addCommand;
        private ICommand _cancelCommand;
        private ObservableCollection<StreetDto> _streetList;
        private StreetDto _selectedStreet;
        private ObservableCollection<ServiceCompanyDto> _companyList;
        private AdditionInfoDto _item;
        public ObservableCollection<HouseDto> HouseList
        {
            get => _houseList;
            set
            {
                _houseList = value; 
                OnPropertyChanged(nameof(HouseList));
            }
        }

        public HouseDto SelectedHouse
        {
            get => _selectedHouse;
            set { _selectedHouse = value; OnPropertyChanged(nameof(SelectedHouse)); }
        }

        private ObservableCollection<ServiceDto> _parentServiceList;

        public ServiceDto SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged(nameof(SelectedService));
            }
        }

        public ServiceDto SelectedParentService
        {
            get => _selectedParentService;
            set { _selectedParentService = value;
                ChangeParentService(value?.Id);
                OnPropertyChanged(nameof(SelectedParentService));

            }
        }
        private void ChangeStreet(int? streetId)
        {
            HouseList.Clear();
            if (!streetId.HasValue)
                return;
            foreach (var house in _requestService.GetHouses(streetId.Value).OrderBy(s => s.Building?.PadLeft(6, '0')).ThenBy(s => s.Corpus?.PadLeft(6, '0')))
            {
                HouseList.Add(house);
            }
            OnPropertyChanged(nameof(HouseList));
        }

        public ObservableCollection<ServiceDto> ParentServiceList
        {
            get { return _parentServiceList; }
            set { _parentServiceList = value; OnPropertyChanged(nameof(ParentServiceList)); }
        }
        public string AddButtonText { get; set; }
        public bool CanEdit { get; set; }
        public HouseTypeAddAndEditDialogViewModel(RequestServiceImpl.RequestService requestService, AdditionInfoDto item, HouseTypeAddAndEditDialog view)
        {
            CompanyList = new ObservableCollection<ServiceCompanyDto>(requestService.GetServiceCompanies());
            _requestService = requestService;

            ParentServiceList = new ObservableCollection<ServiceDto>(_requestService.GetServices(null));
            ServiceList = new ObservableCollection<ServiceDto>();
            HouseList = new ObservableCollection<HouseDto>();
            StreetList = new ObservableCollection<StreetDto>();
            _view = view;
            _item = item;
            if (item != null)
            {
                AddButtonText = "Изменить";
                CanEdit = false;
                SelectedCompany = CompanyList.FirstOrDefault(c => c.Id == item.CompanyId);
                SelectedStreet = StreetList.FirstOrDefault(s => s.Id == item.StreetId);
                SelectedHouse = HouseList.FirstOrDefault(h => h.Id == item.HouseId);
                SelectedParentService = ParentServiceList.FirstOrDefault(p => p.Id == item.ParentId);
                SelectedService = ServiceList.FirstOrDefault(s => s.Id == item.TypeId);
                var flowDoc = ((HouseTypeAddAndEditDialog)_view).FlowInfo.Document;

                var flowDocument = _requestService.GetInfo(item.InfoId);
                
                var content = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
                if (content.CanLoad(System.Windows.DataFormats.Xaml))
                {
                    using (var stream = new MemoryStream())
                    {
                        var buffer = Encoding.Default.GetBytes(flowDocument);
                        stream.Write(buffer, 0, buffer.Length);
                        if (stream.Length > 0)
                        {
                            content.Load(stream, System.Windows.DataFormats.Xaml);
                        }
                        else
                        {
                            content.Text = "";
                        }
                    }
                }
            }
            else
            {
                AddButtonText = "Добавить";
                CanEdit = true;
            }
        }

        public ServiceCompanyDto SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                _selectedCompany = value;
                LoadStreets(_selectedCompany);
                OnPropertyChanged(nameof(SelectedCompany));
            }
        }

        private void LoadStreets(ServiceCompanyDto selectedCompany)
        {
            StreetList.Clear();
            foreach (var street in _requestService.GetStreets(-1, selectedCompany?.Id).OrderBy(s => s.Name))
            {
                StreetList.Add(street);
            }
            OnPropertyChanged(nameof(StreetList));
        }

        public ObservableCollection<ServiceCompanyDto> CompanyList
        {
            get { return _companyList; }
            set
            {
                _companyList = value;
                OnPropertyChanged(nameof(CompanyList)); 
            }
        }

        public ObservableCollection<StreetDto> StreetList
        {
            get { return _streetList; }
            set { _streetList = value; OnPropertyChanged(nameof(StreetList)); }
        }

        public StreetDto SelectedStreet
        {
            get { return _selectedStreet; }
            set
            {
                _selectedStreet = value;
                ChangeStreet(value?.Id);
                OnPropertyChanged(nameof(SelectedStreet));
            }
        }

        private ObservableCollection<ServiceDto> _serviceList;
        private ServiceCompanyDto _selectedCompany;
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;
        private ServiceDto _selectedParentService;
        private ServiceDto _selectedService;

        public ObservableCollection<ServiceDto> ServiceList
        {
            get { return _serviceList; }
            set { _serviceList = value; OnPropertyChanged(nameof(ServiceList)); }
        }
        private void ChangeParentService(int? parentServiceId)
        {
            ServiceList.Clear();
            if (!parentServiceId.HasValue)
                return;

            ServiceList = new ObservableCollection<ServiceDto>(_requestService.GetServices(parentServiceId.Value).OrderBy(s => s.Name));
            OnPropertyChanged(nameof(ServiceList));
        }


        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(Add)); } }
        public ICommand CancelCommand { get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(Cancel)); } }


        private void Add(object sender)
        {
            if (SelectedService == null || SelectedCompany == null)
            {
                MessageBox.Show("УК и Причина обязательны к заполнению!");
                return;
            }
            _view.DialogResult = true;
        }
        private void Cancel(object sender)
        {
            _view.DialogResult = false;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
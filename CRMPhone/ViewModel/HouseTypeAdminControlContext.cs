using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;
using CRMPhone.Annotations;
using CRMPhone.Controls.Admins;
using CRMPhone.Dialogs.Admins;
using CRMPhone.ViewModel.Admins;
using Microsoft.Win32;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class HouseTypeAdminControlContext : INotifyPropertyChanged
    {
        private RequestService _requestService;
        private RequestService RequestService => _requestService ?? (_requestService = new RequestService(AppSettings.DbConnection));
        private ObservableCollection<CityDto> _cityList;
        private CityDto _selectedCity;
        private ObservableCollection<AdditionInfoDto> _bindingList;
        private AdditionInfoDto _selectedBinding;
        private ObservableCollection<ServiceCompanyDto> _companyList;
        private ServiceCompanyDto _selectedCompany;
        private string _streetSearch;
        private HouseTypeAdminControl _view;

        public HouseTypeAdminControlContext()
        {
            CityList = new ObservableCollection<CityDto>();
            BindingList = new ObservableCollection<AdditionInfoDto>();
            CompanyList = new ObservableCollection<ServiceCompanyDto>();
        }

        public void SetView(HouseTypeAdminControl view)
        {
            _view = view;
        }
        public ObservableCollection<CityDto> CityList
        {
            get { return _cityList; }
            set { _cityList = value; OnPropertyChanged(nameof(CityList)); }
        }

        public CityDto SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                _selectedCity = value;
                OnPropertyChanged(nameof(SelectedCity));
            }
        }

        public ObservableCollection<ServiceCompanyDto> CompanyList
        {
            get { return _companyList; }
            set { _companyList = value; OnPropertyChanged(nameof(CompanyList)); }
        }

        public ServiceCompanyDto SelectedCompany
        {
            get { return _selectedCompany; }
            set
            {
                _selectedCompany = value;
                if (_selectedCompany != null)
                {
                    RefreshBinding(_selectedCompany);
                }
                else
                {
                    BindingList.Clear();
                }
                OnPropertyChanged(nameof(SelectedCompany));
            }
        }

        private void RefreshBinding(ServiceCompanyDto company)
        {
            BindingList.Clear();
            RequestService.GetServiceTypeInfo(company.Id).ForEach(s => BindingList.Add(s));
            var filter = _bindingView?.Filter;
            _bindingView = new ListCollectionView(BindingList);
            _bindingView.Filter = filter;
            OnPropertyChanged(nameof(BindingView));
        }

        private ListCollectionView _bindingView;
        public ICollectionView BindingView
        {
            get { return _bindingView; }
        }
        public ObservableCollection<AdditionInfoDto> BindingList
        {
            get { return _bindingList; }
            set { _bindingList = value; OnPropertyChanged(nameof(BindingList)); }
        }

        public string StreetSearch
        {
            get { return _streetSearch; }
            set
            {
                _streetSearch = value; OnPropertyChanged(nameof(StreetSearch));
                if (String.IsNullOrEmpty(value))
                    BindingView.Filter = null;
                else
                    BindingView.Filter = new Predicate<object>(o => ((AdditionInfoDto)o).StreetName.ToUpper().Contains(value.ToUpper()));

            }
        }

        public AdditionInfoDto SelectedBinding
        {
            get { return _selectedBinding; }
            set
            {
                _selectedBinding = value;
                LoadServiceCompanyInfo(value?.HouseId,value?.TypeId);
                OnPropertyChanged(nameof(SelectedBinding));
            }
        }
        private void LoadServiceCompanyInfo(int? houseId, int? typeId)
        {
            if (_view == null)
                return;

            var flowDoc = ((HouseTypeAdminControl)_view).FlowInfo.Document;

            var flowDocument = houseId.HasValue && typeId.HasValue ? _requestService.GetHouseTypeInfo(houseId.Value, typeId.Value) : "";
            if (string.IsNullOrEmpty(flowDocument))
            {
                flowDocument = typeId.HasValue ? _requestService.GetServiceCompanyTypeInfo(SelectedCompany.Id, typeId.Value) : "";
            }
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
        public void RefreshCities()
        {
            CityList.Clear();
            RequestService.GetCities().ToList().ForEach(c => CityList.Add(c));
            OnPropertyChanged(nameof(CityList));
            SelectedCity = CityList.FirstOrDefault();

            CompanyList.Clear();
            RequestService.GetServiceCompanies().ToList().ForEach(c => CompanyList.Add(c));
            OnPropertyChanged(nameof(CompanyList));
        }

        private void RefreshAddress(HouseDto house)
        {
            throw new System.NotImplementedException();
        }
        private ICommand _addBindingCommand;
        public ICommand AddBindingCommand { get { return _addBindingCommand ?? (_addBindingCommand = new CommandHandler(AddBinding, true)); } }
        private ICommand _editBindingCommand;
        public ICommand EditBindingCommand { get { return _editBindingCommand ?? (_editBindingCommand = new RelayCommand(EditBinding)); } }
        private ICommand _deleteBindingCommand;
        public ICommand DeleteBindingCommand { get { return _deleteBindingCommand ?? (_deleteBindingCommand = new RelayCommand(DeleteBinding)); } }

        private void AddBinding()
        {
            ShowStreetEditDialog(null);
        }

        private void EditBinding(object sender)
        {
            var selectedItem = sender as AdditionInfoDto;
            if (selectedItem == null)
                return;
            ShowStreetEditDialog(selectedItem);
        }

        private void DeleteBinding(object sender)
        {
            var selectedItem = sender as AdditionInfoDto;
            if (selectedItem == null)
                return;
            _requestService.DeleteServiceTypeInfo(selectedItem.Type,selectedItem.Id);
            RefreshBinding(SelectedCompany);
        }




        private void ShowStreetEditDialog(AdditionInfoDto selectedItem)
        {
            var view = new HouseTypeAddAndEditDialog();

            var model = new HouseTypeAddAndEditDialogViewModel(RequestService,selectedItem,view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                var flowDoc = view.FlowInfo.Document;
                var content = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
                if (content.CanSave(System.Windows.DataFormats.Xaml))
                {
                    using (var stream = new MemoryStream())
                    {
                        content.Save(stream, System.Windows.DataFormats.Xaml);
                        stream.Position = 0;
                        var flowDocument = Encoding.Default.GetString(stream.GetBuffer());
                        _requestService.SaveServiceTypeInfo(selectedItem, model.SelectedCompany.Id,model.SelectedHouse?.Id,model.SelectedService.Id, flowDocument);
                    }
                }

                RefreshBinding(SelectedCompany);
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
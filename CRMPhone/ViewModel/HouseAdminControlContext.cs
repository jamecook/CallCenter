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
    public class HouseAdminControlContext : INotifyPropertyChanged
    {
        private RequestService _requestService;
        private RequestService RequestService => _requestService ?? (_requestService = new RequestService(AppSettings.DbConnection));
        private ObservableCollection<CityDto> _cityList;
        private CityDto _selectedCity;
        private ObservableCollection<StreetDto> _streetList;
        private StreetDto _selectedStreet;
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;


        public HouseAdminControlContext()
        {
            CityList = new ObservableCollection<CityDto>();
            StreetList = new ObservableCollection<StreetDto>();
            HouseList = new ObservableCollection<HouseDto>();
        }

        public ObservableCollection<CityDto> CityList
        {
            get { return _cityList; }
            set { _cityList = value; OnPropertyChanged(nameof(CityList));}
        }

        public CityDto SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                _selectedCity = value;
                OnPropertyChanged(nameof(SelectedCity));
                RefreshStreets(value);
            }
        }

    private void RefreshStreets(CityDto city)
        {
            StreetList.Clear();
            if (city == null)
                return;
            RequestService.GetStreets(city.Id).ToList().ForEach(s => StreetList.Add(s)); 
        }

        public ObservableCollection<StreetDto> StreetList
        {
            get { return _streetList; }
            set { _streetList = value; OnPropertyChanged(nameof(StreetList));}
        }

        public StreetDto SelectedStreet
        {
            get { return _selectedStreet; }
            set
            {
                _selectedStreet = value;
                OnPropertyChanged(nameof(SelectedStreet));
                RefreshHouses(value);
            }
        }

        private void RefreshHouses(StreetDto street)
        {

            HouseList.Clear();
            if(street== null)
                return;
            RequestService.GetHouses(street.Id).ToList().ForEach(h=>HouseList.Add(h));
        }

        public ObservableCollection<HouseDto> HouseList
        {
            get { return _houseList; }
            set { _houseList = value; OnPropertyChanged(nameof(HouseList));}
        }

        public HouseDto SelectedHouse
        {
            get { return _selectedHouse; }
            set
            {
                _selectedHouse = value;
                //RefreshAddress(value);
                OnPropertyChanged(nameof(SelectedHouse));
            }

        }

        public void RefreshCities()
        {
            CityList.Clear();
            RequestService.GetCities().ToList().ForEach(c=>CityList.Add(c));
            OnPropertyChanged(nameof(CityList));
            SelectedCity = CityList.FirstOrDefault();
        }

        private void RefreshAddress(HouseDto house)
        {
            throw new System.NotImplementedException();
        }
        private ICommand _addStreetCommand;
        public ICommand AddStreetCommand { get { return _addStreetCommand ?? (_addStreetCommand = new CommandHandler(AddStreet, true)); } }
        private ICommand _editStreetCommand;
        public ICommand EditStreetCommand { get { return _editStreetCommand ?? (_editStreetCommand = new RelayCommand(EditStreet)); } }
        private ICommand _deleteStreetCommand;
        public ICommand DeleteStreetCommand { get { return _deleteStreetCommand ?? (_deleteStreetCommand = new CommandHandler(DeleteStreet, true)); } }

        private void DeleteStreet()
        {
            if (SelectedStreet != null)
            {
                if (MessageBox.Show($"Вы действительно хотите удалить улицу {SelectedStreet.Name}", "Удалить",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    RequestService.DeleteStreet(SelectedStreet.Id);
                    RefreshStreets(SelectedCity);
                }
            }
        }

        private void EditStreet(object sender)
        {
            var selectedItem = sender as StreetDto;
            if (selectedItem == null)
                return;
            ShowStreetEditDialog(selectedItem);
        }


        private void AddStreet()
        {
            ShowStreetEditDialog(null);
        }
        private void ShowStreetEditDialog(StreetDto selectedItem)
        {
            var model = new StreetAdminDialogViewModel(RequestService, selectedItem?.Id);
            var view = new StreetAddOrEditDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                RefreshStreets(SelectedCity);
            }
        }

        private ICommand _addHouseCommand;
        public ICommand AddHouseCommand { get { return _addHouseCommand ?? (_addHouseCommand = new CommandHandler(AddHouse, true)); } }


        private ICommand _editHouseCommand;
        public ICommand EditHouseCommand { get { return _editHouseCommand ?? (_editHouseCommand = new RelayCommand(EditHouse)); } }
        private ICommand _deleteHouseCommand;
        public ICommand DeleteHouseCommand { get { return _deleteHouseCommand ?? (_deleteHouseCommand = new CommandHandler(DeleteHouse, true)); } }

        private void AddHouse()
        {
            ShowHouseEditDialog(null);
        }
        private void EditHouse(object sender)
        {
            var selectedItem = sender as HouseDto;
            if (selectedItem == null)
                return;
            ShowHouseEditDialog(selectedItem);
        }
        private void ShowHouseEditDialog(HouseDto selectedItem)
        {
            if (SelectedStreet == null)
                return;
            var model = new HouseAdminDialogViewModel(RequestService, SelectedStreet.Id, selectedItem?.Id);
            var view = new HouseAddOrEditDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                RefreshHouses(SelectedStreet);
            }
        }

        private void DeleteHouse()
        {
            if (SelectedHouse != null)
            {
                if (MessageBox.Show($"Вы действительно хотите удалить дом {SelectedHouse.FullName}", "Удалить",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    RequestService.DeleteHouse(SelectedHouse.Id);
                    RefreshHouses(SelectedStreet);
                }
            }

        }


        /*
                private ICommand _addNewParentServiceCommand;
                public ICommand AddNewStreetCommand { get { return _addNewParentServiceCommand ?? (_addNewParentServiceCommand = new CommandHandler(AddParentService, true)); } }
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
                private void AddParentService()
                {
                    ShowStreetEditDialog(null,null);
                }
                private void EditParentService(object sender)
                {
                    var selectedItem = sender as ServiceDto;
                    if (selectedItem == null)
                        return;

                    ShowStreetEditDialog(selectedItem, null);
                }
                private void AddService()
                {
                    if (SelectedParentService != null)
                    {
                        ShowStreetEditDialog(null, SelectedParentService.Id);
                    }
                }
                private void EditService(object sender)
                {
                    var selectedItem = sender as ServiceDto;
                    if (selectedItem == null)
                        return;
                    if (SelectedParentService != null)
                    {
                        ShowStreetEditDialog(selectedItem, SelectedParentService.Id);
                    }
                }

                private void ShowStreetEditDialog(ServiceDto selectedItem, int? parentId)
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
                */




        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
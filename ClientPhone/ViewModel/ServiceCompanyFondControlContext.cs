using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using RudiGrobler.Calendar.Common;
using Stimulsoft.Report;

namespace CRMPhone.ViewModel
{
    public class ServiceCompanyFondControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<RequestForListDto> _requestList;
        private RequestServiceImpl.RequestService _requestService;
        private ICommand _refreshRequestCommand;
        public ICommand RefreshRequestCommand { get { return _refreshRequestCommand ?? (_refreshRequestCommand = new CommandHandler(RefreshRequest, true)); } }
        private ICommand _clearFiltersCommand;
        public ICommand ClearFiltersCommand { get { return _clearFiltersCommand ?? (_clearFiltersCommand = new CommandHandler(ClearFilters, true)); } }

        private void ClearFilters()
        {
            foreach (var street in FilterStreetList)
            {
                street.Selected = false;
            }
            StreetText = "";
            RefreshRequest();
        }

        public string StreetText
        {
            get { return _streetText; }
            set { _streetText = value; OnPropertyChanged(nameof(StreetText)); }
        }
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;
        private ObservableCollection<FlatDto> _flatList;
        private FlatDto _selectedFlat;
        private ObservableCollection<FieldForFilterDto> _filterStreetList;
        private string _streetText;
        private ObservableCollection<FondDto> _fondList;

        public ObservableCollection<FieldForFilterDto> FilterStreetList
        {
            get { return _filterStreetList; }
            set { _filterStreetList = value; OnPropertyChanged(nameof(FilterStreetList)); }
        }

        public ObservableCollection<HouseDto> HouseList
        {
            get { return _houseList; }
            set { _houseList = value; OnPropertyChanged(nameof(HouseList)); }
        }

        public HouseDto SelectedHouse
        {
            get { return _selectedHouse; }
            set
            {
                _selectedHouse = value;
                ChangeHouse(value?.Id);
                OnPropertyChanged(nameof(SelectedHouse));
            }
        }

        public ObservableCollection<FlatDto> FlatList
        {
            get { return _flatList; }
            set { _flatList = value; OnPropertyChanged(nameof(FlatList)); }
        }

        public FlatDto SelectedFlat
        {
            get { return _selectedFlat; }
            set { _selectedFlat = value; OnPropertyChanged(nameof(SelectedFlat)); }
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
        private void ChangeHouse(int? houseId)
        {
            FlatList.Clear();
            if (!houseId.HasValue)
                return;
            foreach (var flat in _requestService.GetFlats(houseId.Value).OrderBy(s => s.TypeId).ThenBy(s => s.Flat?.PadLeft(6, '0')))
            {
                FlatList.Add(flat);
            }
            OnPropertyChanged(nameof(FlatList));
        }

        private void ChangeCity(int? cityId)
        {
            foreach (var street in FilterStreetList)
            {
                street.PropertyChanged -= StreetOnPropertyChanged;
            }
            FilterStreetList.Clear();
            if (!cityId.HasValue)
                return;
            foreach (var street in _requestService.GetStreets(cityId.Value).OrderBy(s => s.Name).Select(w => new FieldForFilterDto()
            {
                Id = w.Id,
                Name = w.NameWithPrefix,
                Selected = false
            }))
            {
                FilterStreetList.Add(street);
            }
            foreach (var street in FilterStreetList)
            {
                street.PropertyChanged += StreetOnPropertyChanged;
            }
            OnPropertyChanged(nameof(FilterStreetList));
        }

        private void StreetOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var item = sender as FieldForFilterDto;
            if (item != null)
            {
                if (FilterStreetList.Count(f => f.Selected) == 1)
                {
                    ChangeStreet(FilterStreetList.FirstOrDefault(f => f.Selected)?.Id);
                }
                else
                {
                    ChangeStreet(null);
                }
            }

        }

        public ObservableCollection<FondDto> FondList
        {
            get { return _fondList; }
            set { _fondList = value; OnPropertyChanged(nameof(FondList));}
        }

        private void RefreshRequest()
        {
            if (_requestService == null)
                _requestService = new RequestService(AppSettings.DbConnection);
            FondList.Clear();
            var fonds = _requestService.GetServiceCompanyFondList(
                FilterStreetList.Where(w => w.Selected).Select(x => x.Id).ToArray(),
                _selectedHouse?.Id, SelectedFlat?.Id);
            foreach (var address in fonds)
            {
                FondList.Add(address);
            }
        }


        public void InitCollections()
        {
            _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);
            FilterStreetList = new ObservableCollection<FieldForFilterDto>();
            HouseList = new ObservableCollection<HouseDto>();
            FlatList = new ObservableCollection<FlatDto>();
            FondList = new ObservableCollection<FondDto>();

            ChangeCity(_requestService.GetCities().FirstOrDefault().Id);
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
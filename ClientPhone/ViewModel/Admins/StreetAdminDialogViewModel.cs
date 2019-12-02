using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class StreetAdminDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestService _requestService;
        private int? _streetId;
        private ICommand _saveCommand;
        private string _streetName;
        private ObservableCollection<StreetPrefixDto> _streetPrefixList;
        private StreetPrefixDto _selectedStreetPrefix;
        private ObservableCollection<CityDto> _cityList;
        private CityDto _selectedCity;

        public string StreetName
        {
            get { return _streetName; }
            set { _streetName = value; OnPropertyChanged(nameof(StreetName)); }
        }

        public ObservableCollection<StreetPrefixDto> StreetPrefixList
        {
            get { return _streetPrefixList; }
            set { _streetPrefixList = value; OnPropertyChanged(nameof(StreetPrefixList)); }
        }

        public ObservableCollection<CityDto> CityList
        {
            get { return _cityList; }
            set { _cityList = value; OnPropertyChanged(nameof(CityList)); }
        }

        public CityDto SelectedCity
        {
            get { return _selectedCity; }
            set { _selectedCity = value; OnPropertyChanged(nameof(SelectedCity));}
        }

        public StreetPrefixDto SelectedStreetPrefix
        {
            get { return _selectedStreetPrefix; }
            set { _selectedStreetPrefix = value; OnPropertyChanged(nameof(SelectedStreetPrefix));}
        }

        public StreetAdminDialogViewModel(RequestService requestService, int? streetId)
        {
            _requestService = requestService;
            _streetId = streetId;
            StreetPrefixList = new ObservableCollection<StreetPrefixDto>(_requestService.GetStreetPrefixes());
            CityList = new ObservableCollection<CityDto>(_requestService.GetCities());
            if (streetId.HasValue)
            {
                var street = _requestService.GetStreetById(streetId.Value);
                StreetName = street.Name;
                SelectedStreetPrefix = StreetPrefixList.FirstOrDefault(p => p.Id == street.Prefix.Id);
                SelectedCity = CityList.FirstOrDefault(c => c.Id == street.CityId);
            }
            else
            {
                SelectedStreetPrefix = StreetPrefixList.FirstOrDefault();
                SelectedCity = CityList.FirstOrDefault();
            }
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            if (SelectedCity == null || SelectedStreetPrefix == null)
            {
                MessageBox.Show("Необходимо выбрать город, тип и название!", "Улицы");
                return;
            }
            _requestService.SaveStreet(_streetId, StreetName,SelectedCity.Id,SelectedStreetPrefix.Id);
            _view.DialogResult = true;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
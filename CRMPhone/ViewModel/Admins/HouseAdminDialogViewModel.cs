using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using conaito;
using CRMPhone.Annotations;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class HouseAdminDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestService _requestService;
        private int _streetId;
        private int? _houseId;
        private ICommand _saveCommand;
        private string _streetName;
        private string _buildingNumber;
        private string _corpus;
        private int? _entranceCount;
        private int? _floorCount;
        private int? _flatsCount;
        private ObservableCollection<ServiceCompanyDto> _serviceCompanyList;
        private ServiceCompanyDto _selectedServiceCompany;

        public string StreetName
        {
            get => _streetName;
            set { _streetName = value; OnPropertyChanged(nameof(StreetName)); }
        }

        public ObservableCollection<ServiceCompanyDto> ServiceCompanyList
        {
            get { return _serviceCompanyList; }
            set { _serviceCompanyList = value; OnPropertyChanged(nameof(ServiceCompanyList));}
        }

        public ServiceCompanyDto SelectedServiceCompany
        {
            get { return _selectedServiceCompany; }
            set { _selectedServiceCompany = value; OnPropertyChanged(nameof(SelectedServiceCompany));}
        }

        public string BuildingNumber
        {
            get => _buildingNumber;
            set { _buildingNumber = value; OnPropertyChanged(nameof(BuildingNumber));}
        }

        public string Corpus
        {
            get => _corpus;
            set { _corpus = value; OnPropertyChanged(nameof(Corpus));}
        }

        public int? EntranceCount
        {
            get => _entranceCount;
            set { _entranceCount = value; OnPropertyChanged(nameof(EntranceCount));}
        }

        public int? FloorCount
        {
            get => _floorCount;
            set { _floorCount = value; OnPropertyChanged(nameof(FloorCount));}
        }

        public int? FlatsCount
        {
            get => _flatsCount;
            set { _flatsCount = value; OnPropertyChanged(nameof(FlatsCount));}
        }

        public HouseAdminDialogViewModel(RequestService requestService, int streetId, int? houseId)
        {
            _requestService = requestService;
            _streetId = streetId;
            ServiceCompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
            var street = _requestService.GetStreetById(streetId);
            StreetName = street.Name;
            _houseId = houseId;
              if (houseId.HasValue)
              {
                  var house = _requestService.GetHouseById(houseId.Value);
                  BuildingNumber = house.Building;
                  Corpus = house.Corpus;
                  EntranceCount = house.EntranceCount;
                  FlatsCount = house.FlatCount;
                  FloorCount = house.FloorCount;
                  SelectedServiceCompany = ServiceCompanyList.FirstOrDefault(s => s.Id == house.ServiceCompanyId);
              }
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            if (SelectedServiceCompany == null || string.IsNullOrEmpty(BuildingNumber))
            {
                MessageBox.Show(Application.Current.MainWindow, "Необходимо выбрать УК и номер дома!", "Дома");
                return;
            }
            var corpus = Corpus?.Trim();
            if (_houseId == null)
            {
                var findedhouse = _requestService.FindHouse(_streetId, BuildingNumber, corpus);
                if (findedhouse != null)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Такой дом уже существует в системе!", "Дома");
                    return;
                }
            }
            _requestService.SaveHouse(_houseId, _streetId, BuildingNumber, corpus, SelectedServiceCompany.Id, EntranceCount, FloorCount, FlatsCount);
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
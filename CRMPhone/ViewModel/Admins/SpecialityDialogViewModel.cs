using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;

namespace CRMPhone.ViewModel.Admins
{
    public class SpecialityDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int? _specialityId;
        private ICommand _saveCommand;
        private string _specialityName;

        public string SpecialityName
        {
            get { return _specialityName; }
            set { _specialityName = value; OnPropertyChanged(nameof(SpecialityName));}
        }

        public SpecialityDialogViewModel(RequestServiceImpl.RequestService requestService, int? specialityId)
        {
            _requestService = requestService;
            _specialityId = specialityId;
            if (specialityId.HasValue)
            {
                var serviceCompany = _requestService.GetSpecialityById(specialityId.Value);
                SpecialityName = serviceCompany.Name;
            }
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            _requestService.SaveSpeciality(_specialityId,SpecialityName);
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
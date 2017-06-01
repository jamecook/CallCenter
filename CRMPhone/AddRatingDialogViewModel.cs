using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dto;
using CRMPhone.ViewModel;

namespace CRMPhone
{
    public class AddRatingDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestService _requestService;
        private int _requestId;
        private ObservableCollection<RequestRatingDto> _ratingList;
        private RequestRatingDto _selectedRating;

        public AddRatingDialogViewModel(RequestService requestService, int requestId)
        {
            _requestService = requestService;
            _requestId = requestId;
            RatingList = new ObservableCollection<RequestRatingDto>(_requestService.GetRequestRating());
            var request = _requestService.GetRequest(_requestId);
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            _requestService.SetRating(_requestId, SelectedRating.Id, Description);
            _view.DialogResult = true;
        }

        public ObservableCollection<RequestRatingDto> RatingList
        {
            get { return _ratingList; }
            set { _ratingList = value; OnPropertyChanged(nameof(RatingList)); }
        }
        public string Description { get; set; }

        public RequestRatingDto SelectedRating
        {
            get { return _selectedRating; }
            set { _selectedRating = value; OnPropertyChanged(nameof(SelectedRating)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
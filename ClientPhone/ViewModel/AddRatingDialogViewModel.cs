using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ClientPhone.Services;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class AddRatingDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private int _requestId;
        private ObservableCollection<RequestRatingDto> _ratingList;
        private RequestRatingDto _selectedRating;

        public AddRatingDialogViewModel( int requestId)
        {
            _requestId = requestId;
            RatingList = new ObservableCollection<RequestRatingDto>(RestRequestService.GetRequestRating(AppSettings.CurrentUser.Id));
            var request = RestRequestService.GetRequest(AppSettings.CurrentUser.Id, _requestId);
            Refresh(null);
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        private ICommand _saveCommand;
        private ObservableCollection<RequestRatingListDto> _requestRatingHistory;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private void Save(object sender)
        {
            RestRequestService.SetRating(AppSettings.CurrentUser.Id, _requestId, SelectedRating.Id, Description);
            _view.DialogResult = true;
        }

        public ObservableCollection<RequestRatingDto> RatingList
        {
            get { return _ratingList; }
            set { _ratingList = value; OnPropertyChanged(nameof(RatingList)); }
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(DeleteRating)); } }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new RelayCommand(Refresh)); } }

        private void Refresh(object obj)
        {
            RequestRatingHistory = new ObservableCollection<RequestRatingListDto>(RestRequestService.GetRequestRatings(AppSettings.CurrentUser.Id, _requestId));
        }
        private void DeleteRating(object obj)
        {
            var item = obj as RequestRatingListDto;
            if (item == null)
                return;
            if (MessageBox.Show(_view, "Удалить выбранную оценку?", "Удаление", MessageBoxButton.YesNo) ==
                MessageBoxResult.Yes)
            {
                RestRequestService.DeleteRequestRatingById(AppSettings.CurrentUser.Id, item.Id);
                Refresh(null);
            }
        }

        public bool CanDelete => AppSettings.CurrentUser != null && AppSettings.CurrentUser.Roles.Exists(r => r.Name == "admin");
        public ObservableCollection<RequestRatingListDto> RequestRatingHistory
        {
            get { return _requestRatingHistory; }
            set { _requestRatingHistory = value; OnPropertyChanged(nameof(RequestRatingHistory));}
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
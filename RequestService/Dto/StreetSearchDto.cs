using System;
using System.ComponentModel;
using System.Windows.Data;

namespace RequestServiceImpl.Dto
{
    public class StreetSearchDto : INotifyPropertyChanged
    {
        private string _streetSearch;
        public string StreetSearch
        {
            get { return _streetSearch; }
            set
            {
                _streetSearch = value; OnPropertyChanged(nameof(StreetSearch));
                if (String.IsNullOrEmpty(value))
                    StreetView.Filter = null;
                else
                    StreetView.Filter = new Predicate<object>(o => ((FieldForFilterDto)o).Name.ToUpper().Contains(value.ToUpper()));
            }
        }


        public ICollectionView StreetView
        {
            get => _streetView;
            set
            {
                _streetView = value;
                OnPropertyChanged(nameof(StreetView));
            }
        }
        private ICollectionView _streetView;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
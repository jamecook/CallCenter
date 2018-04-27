using System.ComponentModel;

namespace RequestServiceImpl.Dto
{
    public class FieldForFilterDto : INotifyPropertyChanged
    {
        private bool _selected;
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; OnPropertyChanged(nameof(Selected)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
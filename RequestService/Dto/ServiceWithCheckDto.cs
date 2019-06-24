using System.ComponentModel;

namespace RequestServiceImpl.Dto
{
    public class ServiceWithCheckDto : INotifyPropertyChanged
    {
        private bool _checked;
        public int Id { get; set; }

        public bool Checked
        {
            get { return _checked; }
            set { _checked = value; OnPropertyChanged(nameof(Checked));}
        }

        public string Name { get; set; }
        public bool CanSendSms { get; set; }
        public bool Immediate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public override string ToString()
        {
            return $"{Id}:{Name}";
        }

    }
}
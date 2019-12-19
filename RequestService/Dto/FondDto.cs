using System;
using System.ComponentModel;

namespace RequestServiceImpl.Dto
{
    public class FondDto
    {
        private string _name;
        private string _phones;
        public int Id { get; set; }
        public string Flat { get; set; }
        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Phones
        {
            get => _phones;
            set
            { 
                _phones = value;
                OnPropertyChanged(nameof(Phones));
            }
        }

        public DateTime? KeyDate { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
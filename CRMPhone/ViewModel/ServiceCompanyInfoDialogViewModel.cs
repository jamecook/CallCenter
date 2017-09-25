using CRMPhone.Annotations;
using System.ComponentModel;
using System.Windows;

namespace CRMPhone.ViewModel
{
    public class ServiceCompanyInfoDialogViewModel : INotifyPropertyChanged
    {
        public ServiceCompanyInfoDialogViewModel(string name,string info)
        {
            ServiceCompanyInfo = info;
            ServiceCompanyName = name;
        }
        private Window _view;
        public string ServiceCompanyName { get; set; }
        public string ServiceCompanyInfo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
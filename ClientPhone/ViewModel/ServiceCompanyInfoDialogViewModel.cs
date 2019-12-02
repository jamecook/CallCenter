using CRMPhone.Annotations;
using System.ComponentModel;
using System.Windows;
using RequestServiceImpl.Dto;

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
        public void SetView(Window view)
        {
            _view = view;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
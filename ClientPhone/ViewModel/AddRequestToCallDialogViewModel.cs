using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class AddRequestToCallDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        public void SetView(Window view)
        {
            _view = view;
        }
        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        private ICommand _cancelCommand;
        public ICommand CancelCommand { get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(Cancel)); } }

        private void Cancel(object obj)
        {
            _view.DialogResult = false;
        }

        private void Save(object sender)
        {
            _view.DialogResult = true;
        }

        public int RequestId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
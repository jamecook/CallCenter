using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dialogs;
using CRMPhone.Dialogs.Admins;
using CRMPhone.ViewModel.Admins;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using ServiceCompanyAddOrEditDialog = CRMPhone.Dialogs.Admins.ServiceCompanyAddOrEditDialog;

namespace CRMPhone.ViewModel
{
    public class SpecialityControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<SpecialityDto> _specialityList;
        public ObservableCollection<SpecialityDto> SpecialityList
        {
            get { return _specialityList; }
            set { _specialityList = value; OnPropertyChanged(nameof(SpecialityList)); }
        }

        private RequestService _requestService;
        private ICommand _addNewCommand;
        public ICommand AddNewCommand { get { return _addNewCommand ?? (_addNewCommand = new CommandHandler(AddCompany, true)); } }
        private ICommand _editCommand;
        public ICommand EditCommand { get { return _editCommand ?? (_editCommand = new RelayCommand(EditCompany)); } }

        public SpecialityControlContext()
        {
            SpecialityList = new ObservableCollection<SpecialityDto>();
        }
        private void EditCompany(object sender)
        {
            var selectedItem = sender as SpecialityDto;
            if (selectedItem == null)
                return;
            if (_requestService == null)
                _requestService = new RequestService(AppSettings.DbConnection);

            var model = new SpecialityDialogViewModel(_requestService, selectedItem.Id);
            var view = new SpecialityAddOrEditDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                RefreshList();
            }
        }

        public void RefreshList()
        {
            if (_requestService == null)
                _requestService = new RequestService(AppSettings.DbConnection);
            SpecialityList.Clear();

            _requestService.GetSpecialities().ForEach(c => SpecialityList.Add(c));

            OnPropertyChanged(nameof(SpecialityList));
        }

        private void AddCompany()
        {
            var model = new SpecialityDialogViewModel(_requestService, null);
            var view = new SpecialityAddOrEditDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                RefreshList();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
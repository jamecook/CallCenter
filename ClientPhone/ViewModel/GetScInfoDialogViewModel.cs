using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.Dialogs;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class GetScInfoDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;
        private int? _serviceCompanyId;
        private ServiceCompanyDto _selectedServiceCompany;
        private ObservableCollection<ServiceCompanyDto> _serviceCompanyList;
        private ICommand _closeCommand;
        private ICommand _saveCommand;
        private FlowDocument _scInfo;


        private RequestServiceImpl.RequestService _requestService;


        public GetScInfoDialogViewModel(RequestServiceImpl.RequestService requestService, int? serviceCompanyId,Window view)
        {
            _view = view;
            _requestService = requestService;
            _serviceCompanyId = serviceCompanyId;
            ServiceCompanyList = new ObservableCollection<ServiceCompanyDto>(_requestService.GetServiceCompanies());
            SelectedServiceCompany = ServiceCompanyList.FirstOrDefault(s => s.Id == _serviceCompanyId);
            if (SelectedServiceCompany == null)
                SelectedServiceCompany = ServiceCompanyList.FirstOrDefault();
        }
        public ServiceCompanyDto SelectedServiceCompany
        {
            get { return _selectedServiceCompany; }
            set { _selectedServiceCompany = value; OnPropertyChanged(nameof(SelectedServiceCompany));
                LoadServiceCompanyInfo(value); }
        }
        public ObservableCollection<ServiceCompanyDto> ServiceCompanyList
        {
            get { return _serviceCompanyList; }
            set { _serviceCompanyList = value; OnPropertyChanged(nameof(ServiceCompanyList)); }
        }

        private void LoadServiceCompanyInfo(ServiceCompanyDto selectedServiceCompany)
        {
            if(_view == null)
                return;
            if(selectedServiceCompany == null)
                return;

            var flowDoc = ((GetScInfoDialog)_view).FlowInfo.Document;

            var flowDocument = _requestService.GetServiceCompanyAdvancedInfo(selectedServiceCompany.Id)??"";
            var content = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
            if (content.CanLoad(System.Windows.DataFormats.Xaml))
            {
                using (var stream = new MemoryStream())
                {
                    var buffer = Encoding.Default.GetBytes(flowDocument);
                    stream.Write(buffer, 0, buffer.Length);
                    if (stream.Length > 0)
                        content.Load(stream, System.Windows.DataFormats.Xaml);
                    else
                        content.Text = "";
                }
            }
        }
        public FlowDocument ScInfo
        {
            get { return _scInfo; }
            set { _scInfo = value; OnPropertyChanged(nameof(ScInfo)); }
        }

        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new CommandHandler(Save, true)); } }

        private void Save()
        {
            if(SelectedServiceCompany == null)
                return;

            var flowDoc = ((GetScInfoDialog) _view).FlowInfo.Document;
            var content = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
            if (content.CanSave(System.Windows.DataFormats.Xaml))
            {
                using (var stream = new MemoryStream())
                {
                    content.Save(stream, System.Windows.DataFormats.Xaml);
                    stream.Position = 0;
                    var savedFlow = Encoding.Default.GetString(stream.GetBuffer());
                    _requestService.SaveServiceCompanyAdvancedInfo(SelectedServiceCompany.Id, savedFlow);
                }
            }
            System.Windows.MessageBox.Show("Данные успешно сохранены!", "Информация");

        }

        public Visibility VisibleSave
        {
            get
            {
                if (AppSettings.CurrentUser != null && AppSettings.CurrentUser.Roles.Exists(r => r.Name == "admin"))
                { return Visibility.Visible; }
                return Visibility.Collapsed;
            }
        }

        public ICommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new CommandHandler(Close, true)); } }

        private void Close()
        {
            _view.Close();
        }

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
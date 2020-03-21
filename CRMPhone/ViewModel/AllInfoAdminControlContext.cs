using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using CRMPhone.Controls.Admins;
using CRMPhone.Dialogs.Admins;
using CRMPhone.ViewModel.Admins;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class AllInfoAdminControlContext : INotifyPropertyChanged
    {
        private RequestService _requestService;
        private RequestService RequestService => _requestService ?? (_requestService = new RequestService(AppSettings.DbConnection));
        private AllInfoAdminControl _view;

        public AllInfoAdminControlContext()
        {
        }

        public void SetView(AllInfoAdminControl view)
        {
            _view = view;
        }


        public void GetInfo()
        {
            var flowDoc = _view.FlowInfo.Document;

            var flowDocument = RequestService.GetInfoForAll();

            var content = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
            if (content.CanLoad(System.Windows.DataFormats.Xaml))
            {
                using (var stream = new MemoryStream())
                {
                    var buffer = Encoding.Default.GetBytes(flowDocument);
                    stream.Write(buffer, 0, buffer.Length);
                    if (stream.Length > 0)
                    {
                        content.Load(stream, System.Windows.DataFormats.Xaml);
                    }
                    else
                    {
                        content.Text = "";
                    }
                }
            }
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new CommandHandler(Save, true)); } }

        private void Save()
        {
            var flowDoc = _view.FlowInfo.Document;
            var content = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
            var contentText = content.Text;
            if (content.CanSave(System.Windows.DataFormats.Xaml))
            {
                using (var stream = new MemoryStream())
                {
                    content.Save(stream, System.Windows.DataFormats.Xaml);
                    stream.Position = 0;
                    var flowDocument = Encoding.Default.GetString(stream.GetBuffer());
                    RequestService.SaveInfoForAll(AppSettings.CurrentUser.Id, flowDocument, contentText);
                    MessageBox.Show("Данные успешно сохранены!");
                }
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
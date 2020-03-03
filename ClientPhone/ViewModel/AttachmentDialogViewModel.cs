using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using ClientPhone.Services;
using CRMPhone.Annotations;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace CRMPhone.ViewModel
{
    public class AttachmentDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private int _requestId;
        private ObservableCollection<AttachmentDto> _attachmentList;
        private AttachmentDto _selectedAttachmentItem;


        public AttachmentDialogViewModel(int requestId)
        {
            _requestId = requestId;
            AttachmentList = new ObservableCollection<AttachmentDto>(RestRequestService.GetAttachments(AppSettings.CurrentUser.Id, requestId));
        }

        public ObservableCollection<AttachmentDto> AttachmentList
        {
            get { return _attachmentList; }
            set { _attachmentList = value; OnPropertyChanged(nameof(AttachmentList));}
        }
        public void Refresh()
        {
            AttachmentList = new ObservableCollection<AttachmentDto>(RestRequestService.GetAttachments(AppSettings.CurrentUser.Id, _requestId));
        }


        private ICommand _addCommand;
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new CommandHandler(Add, true)); } }

        private ICommand _openAttachmentCommand;
        public ICommand OpenAttachmentCommand { get { return _openAttachmentCommand ?? (_openAttachmentCommand = new RelayCommand(OpenAttachment)); } }

        private void OpenAttachment(object obj)
        {
            var item = obj as AttachmentDto;
            if (item == null)
                return;
            var extention = Path.GetExtension(item.FileName);
            var tempFileName = $"{Path.GetTempPath()}{Guid.NewGuid().ToString()}.{extention}";
            var buffer = RestRequestService.GetFile(AppSettings.CurrentUser.Id, item.RequestId,
                item.FileName);
            if (buffer == null || buffer.Length == 0)
                return;
            File.WriteAllBytes(tempFileName, buffer);
            Process.Start(tempFileName);
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(Delete, true)); } }


        private ICommand _downloadCommand;
        public ICommand DownloadCommand { get { return _downloadCommand ?? (_downloadCommand = new CommandHandler(Download, true)); } }

        private void Download()
        {
            if (SelectedAttachmentItem == null)
                return;
            var saveDialog = new SaveFileDialog
            {
                FileName = SelectedAttachmentItem.Name,
                Filter = "Все файлы|*.*"
            };
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var buffer = RestRequestService.GetFile(AppSettings.CurrentUser.Id, SelectedAttachmentItem.RequestId,
                    SelectedAttachmentItem.FileName);
                if(buffer == null || buffer.Length==0)
                    return;
                File.WriteAllBytes(saveDialog.FileName,buffer);
            }
        }

        private void Add()
        {
            var openDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Все файлы|*.*"
            };
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                RestRequestService.AddAttachmentToRequest(AppSettings.CurrentUser.Id, _requestId, openDialog.FileName);
                Refresh();
            }

        }
        private void Delete()
        {
            if (SelectedAttachmentItem != null)
            {
                RestRequestService.DeleteAttachment(AppSettings.CurrentUser.Id, SelectedAttachmentItem.Id);
                Refresh();
            }
        }

        public AttachmentDto SelectedAttachmentItem
        {
            get { return _selectedAttachmentItem; }
            set { _selectedAttachmentItem = value; OnPropertyChanged(nameof(SelectedAttachmentItem));}
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
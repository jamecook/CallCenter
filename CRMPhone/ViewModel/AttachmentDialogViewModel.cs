using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace CRMPhone.ViewModel
{
    public class AttachmentDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _requestId;
        private ObservableCollection<AttachmentDto> _attachmentList;
        private AttachmentDto _selectedAttachmentItem;


        public AttachmentDialogViewModel(RequestServiceImpl.RequestService requestService, int requestId)
        {
            _requestService = requestService;
            _requestId = requestId;
            AttachmentList = new ObservableCollection<AttachmentDto>(_requestService.GetAttachments(requestId));
        }

        public ObservableCollection<AttachmentDto> AttachmentList
        {
            get { return _attachmentList; }
            set { _attachmentList = value; OnPropertyChanged(nameof(AttachmentList));}
        }
        public void Refresh()
        {
            AttachmentList = new ObservableCollection<AttachmentDto>(_requestService.GetAttachments(_requestId));
        }


        private ICommand _addCommand;
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new CommandHandler(Add, true)); } }

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
                File.WriteAllBytes(saveDialog.FileName,_requestService.GetFile(SelectedAttachmentItem.RequestId,SelectedAttachmentItem.FileName));
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
                _requestService.AddAttachmentToRequest(_requestId,openDialog.FileName);
                Refresh();
            }

        }
        private void Delete()
        {
            if (SelectedAttachmentItem != null)
            {
                _requestService.DeleteAttachment(SelectedAttachmentItem.Id);
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
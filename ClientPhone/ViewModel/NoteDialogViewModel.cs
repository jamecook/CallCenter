using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ClientPhone.Services;
using CRMPhone.Annotations;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class NoteDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private int _requestId;
        private ObservableCollection<NoteDto> _noteList;
        private AttachmentDto _selectedNoteItem;


        public NoteDialogViewModel( int requestId)
        {
            _requestId = requestId;
            NoteList = new ObservableCollection<NoteDto>(RestRequestService.GetNotes(AppSettings.CurrentUser.Id, requestId));
        }

        public ObservableCollection<NoteDto> NoteList
        {
            get { return _noteList; }
            set { _noteList = value; OnPropertyChanged(nameof(NoteList)); }
        }
        public void Refresh()
        {
            NoteList = new ObservableCollection<NoteDto>(RestRequestService.GetNotes(AppSettings.CurrentUser.Id, _requestId));
        }

        public string Comment { get => _comment;
            set
            { _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }
        private ICommand _addCommand;
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new CommandHandler(Add, true)); } }

        private ICommand _deleteCommand;
        private string _comment;

        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(Delete, true)); } }
        private void Add()
        {
            RestRequestService.AddNewNote(AppSettings.CurrentUser.Id, _requestId, Comment);
            Comment = "";
            Refresh();
        }
        private void Delete()
        {
            //if (SelectedNoteItem != null)
            //{
            //    _requestService.DeleteAttachment(SelectedNoteItem.Id);
            //    Refresh();
            //}
        }

        public AttachmentDto SelectedNoteItem
        {
            get { return _selectedNoteItem; }
            set { _selectedNoteItem = value; OnPropertyChanged(nameof(SelectedNoteItem)); }
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
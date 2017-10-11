using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class NoteDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _requestId;
        private ObservableCollection<NoteDto> _noteList;
        private AttachmentDto _selectedNoteItem;


        public NoteDialogViewModel(RequestServiceImpl.RequestService requestService, int requestId)
        {
            _requestService = requestService;
            _requestId = requestId;
            NoteList = new ObservableCollection<NoteDto>(_requestService.GetNotes(requestId));
        }

        public ObservableCollection<NoteDto> NoteList
        {
            get { return _noteList; }
            set { _noteList = value; OnPropertyChanged(nameof(NoteList));}
        }
        public void Refresh()
        {
            NoteList = new ObservableCollection<NoteDto>(_requestService.GetNotes(_requestId));
        }


        private ICommand _addCommand;
        public ICommand AddCommand { get { return _addCommand ?? (_addCommand = new CommandHandler(Add, true)); } }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand { get { return _deleteCommand ?? (_deleteCommand = new CommandHandler(Delete, true)); } }
        private void Add()
        {

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
            set { _selectedNoteItem = value; OnPropertyChanged(nameof(SelectedNoteItem));}
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
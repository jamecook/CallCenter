using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using conaito;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class AudioSettingsDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;
        private UserAgent _sipAgent;
        private ObservableCollection<RequestRatingDto> _ratingList;
        private RequestRatingDto _selectedRating;

        public AudioSettingsDialogViewModel(UserAgent sipAgent)
        {
            _sipAgent = sipAgent;

            object names = null;
            object ids = null;
            _sipAgent.VoiceSettings.GetPlayers(out names, out ids);
            var playIds = ids as int[];
            var playNames = names as string[];

            PlayDeviceList = new ObservableCollection<AudioDeviceDto>();
            for (var i = 0; i < playIds?.Length; i++)
            {
                PlayDeviceList.Add(new AudioDeviceDto() {Id = playIds[i], Name = playNames[i]});
            }
            _sipAgent.VoiceSettings.GetRecorders(out names, out ids);
            var recordIds = ids as int[];
            var recordNames = names as string[];
            RecordDeviceList = new ObservableCollection<AudioDeviceDto>();
            for (var i = 0; i < recordIds?.Length; i++)
            {
                RecordDeviceList.Add(new AudioDeviceDto() { Id = recordIds[i], Name = recordNames[i] });
            }

            SelectedPlayDevice = PlayDeviceList.FirstOrDefault(p => p.Id == _sipAgent.VoiceSettings.PlayerDevice);
            SelectedRecordDevice = RecordDeviceList.FirstOrDefault(p => p.Id == _sipAgent.VoiceSettings.RecorderDevice);
        }

        public ObservableCollection<AudioDeviceDto> PlayDeviceList { get; set; }
        public ObservableCollection<AudioDeviceDto> RecordDeviceList { get; set; }

        public AudioDeviceDto SelectedPlayDevice
        {
            get
            {
                return _selectedPlayDevice;
            }
            set
            {
                _selectedPlayDevice = value;
                if (value != null)
                {
                    _sipAgent.VoiceSettings.PlayerDevice = value.Id;
                }
                OnPropertyChanged(nameof(SelectedPlayDevice));
            }
        }

        public AudioDeviceDto SelectedRecordDevice
        {
            get => _selectedRecordDevice;
            set
            {
                _selectedRecordDevice = value;
                if (value != null)
                {
                    _sipAgent.VoiceSettings.RecorderDevice = value.Id;
                }
            }
        }

        public int CurrentPlayValume
        {
            get
            {
                var valume = Convert.ToInt32(_sipAgent.VoiceSettings.SpkVolume);
                return valume;
            }
            set
            {
                _currentPlayValume = value;
                _sipAgent.VoiceSettings.SpkVolume = 2 * value;
                OnPropertyChanged(nameof(CurrentPlayValume));
            }
        }

        public int CurrentRecordValume
        {
            get
            {
                var valume = Convert.ToInt32(_sipAgent.VoiceSettings.MicVolume);
                return valume;
            }
            set
            {
                _currentRecordValume = value;
                _sipAgent.VoiceSettings.MicVolume = 2 * value;
                OnPropertyChanged(nameof(CurrentRecordValume));
            }
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        private ICommand _closeCommand;
        private AudioDeviceDto _selectedPlayDevice;
        private int _currentPlayValume;
        private int _currentRecordValume;
        private AudioDeviceDto _selectedRecordDevice;
        public ICommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(Close)); } }

        private void Close(object obj)
        {
            _view.DialogResult = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
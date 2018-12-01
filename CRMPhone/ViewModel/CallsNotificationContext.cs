using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using CRMPhone.Dialogs.Admins;
using CRMPhone.ViewModel.Admins;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using MessageBox = System.Windows.MessageBox;

namespace CRMPhone.ViewModel
{
    public class CallsNotificationContext : INotifyPropertyChanged
    {
        private RequestService _requestService;
        public CallsNotificationContext()
        {
        }
        public List<NotificationYesNo> NotificationList { get; set; }

        public NotificationYesNo Notification
        {
            get { return _notification; }
            set { _notification = value; OnPropertyChanged(nameof(Notification));}
        }

        private ICommand _uploadCommand;
        public ICommand UploadCommand { get { return _uploadCommand ?? (_uploadCommand = new CommandHandler(Upload, true)); } }
        private ICommand _playCommand;
        public ICommand PlayCommand { get { return _playCommand ?? (_playCommand = new CommandHandler(Play, true)); } }
        private ICommand _saveCommand;
        private NotificationYesNo _notification;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new CommandHandler(Save, true)); } }
        private void Upload()
        {
            var openDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Wav файлы|*.wav"
            };
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                File.Copy(openDialog.FileName, @"\\192.168.1.130\ivr\ukrus.wav", true);
                MessageBox.Show("Файл успешно загружен на сервер!");
            }

        }
        private void Save()
        {
            if (Notification == null)
                return;
            _requestService.SaveIvrNotification(54,Notification.Id == 1);
            MessageBox.Show("Данные успешно сохранены!");
        }
        private void Play()
        {
            Process.Start(@"\\192.168.1.130\ivr\ukrus.wav");
        }


        public void Init()
        {
            _requestService = new RequestService(AppSettings.DbConnection);
            NotificationList = new List<NotificationYesNo>();
            NotificationList.Add(new NotificationYesNo()
            {
                Id = 0,
                Name = "Отключен"
            });
            NotificationList.Add(new NotificationYesNo()
            {
                Id = 1,
                Name = "Включен"
            });
            var isEnabled = _requestService.IsEnableIvrNotification(54);
            Notification = NotificationList.FirstOrDefault(n => n.Id == (isEnabled ? 1 : 0));
            OnPropertyChanged(nameof(NotificationList));

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using CRMPhone.Annotations;
using CRMPhone.Dialogs.Admins;
using CRMPhone.ViewModel.Admins;
using Microsoft.Win32;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class RingUpAdminControlContext : INotifyPropertyChanged
    {
        public RingUpAdminControlContext()
        {
            FromDate = DateTime.Today.AddDays(-30);
        }
        private RequestService _requestService;
        private RequestService RequestService => _requestService ?? (_requestService = new RequestService(AppSettings.DbConnection));

        private ObservableCollection<RingUpHistoryDto> _ringUpList;
        private ICommand _ediCommand;
        public ICommand EditCommand { get { return _ediCommand ?? (_ediCommand = new CommandHandler(EditPhone, true)); } }
        private ICommand _refreshCommand;
        private RingUpHistoryDto _currentRingUp;
        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new CommandHandler(Refresh, true)); } }
        private ICommand _newCommand;
        public ICommand NewCommand { get { return _newCommand ?? (_newCommand = new CommandHandler(NewRingUp, true)); } }
        private ICommand _abortCommand;
        private ObservableCollection<RingUpInfoDto> _ringUpInfoList;
        public ICommand AbortCommand { get { return _abortCommand ?? (_abortCommand = new CommandHandler(AbortRingUp, true)); } }
        private ICommand _continueCommand;
        public ICommand ContinueCommand { get { return _continueCommand ?? (_continueCommand = new CommandHandler(ContinueRingUp, true)); } }

        private ICommand _exportCommand;
        public ICommand ExportCommand { get { return _exportCommand ?? (_exportCommand = new CommandHandler(Export, true)); } }

        private ICommand _exportListsCommand;
        public ICommand ExportListsCommand { get { return _exportListsCommand ?? (_exportListsCommand = new CommandHandler(ExportLists, true)); } }

        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; OnPropertyChanged(nameof(FromDate)); }
        }

        private void AbortRingUp()
        {
            if (CurrentRingUp == null)
            {
                MessageBox.Show($"Необходимо выделить один элемент из списка обзвонов!","Автообзвон");
                return;
            }
            if (MessageBox.Show($"Вы уверены что хотите прервать обзвон № {CurrentRingUp.Id}", "Автообзвон",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                RequestService.AbortRingUp(CurrentRingUp.Id);
                Refresh();
            }
        }
        private void ContinueRingUp()
        {
            if (CurrentRingUp == null)
            {
                MessageBox.Show($"Необходимо выделить один элемент из списка обзвонов!","Автообзвон");
                return;
            }
            if (MessageBox.Show($"Вы уверены что хотите продолжить обзвон № {CurrentRingUp.Id}", "Автообзвон",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                RequestService.ContinueRingUp(CurrentRingUp.Id);
                Refresh();
            }
        }
        private DateTime _fromDate;

        private void Export()
        {
            if (RingUpInfoList.Count == 0)
            {
                MessageBox.Show("Нельзя экспортировать пустой список!", "Ошибка");
                return;
            }
            try
            {
                var saveDialog = new SaveFileDialog();
                saveDialog.AddExtension = true;
                saveDialog.DefaultExt = ".xml";
                saveDialog.Filter = "XML Файл|*.xml";
                if (saveDialog.ShowDialog() == true)
                {
                    var fileName = saveDialog.FileName;
                    if (fileName.EndsWith(".xml"))
                    {
                        XElement root = new XElement("Records");
                        foreach (var record in RingUpInfoList)
                        {
                            root.AddFirst(
                                new XElement("Record",
                                    new[]
                                    {
                                        new XElement("Телефон", record.Phone),
                                        new XElement("ВремяПоследнегоЗвонка", record.LastCallTime?.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("Продолжительность", record.LastCallLength),
                                        new XElement("Попыток", record.CalledCount),
                                        new XElement("Результат", record.DoneCalls),
                                    }));
                        }
                        var saver = new FileStream(fileName, FileMode.Create);
                        root.Save(saver);
                        saver.Close();
                    }
                    MessageBox.Show("Данные сохранены в файл\r\n" + fileName);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Произошла ошибка:\r\n" + exc.Message);
            }

        }
        private void ExportLists()
        {
            if (RingUpList.Count == 0)
            {
                MessageBox.Show("Нельзя экспортировать пустой список!", "Ошибка");
                return;
            }
            try
            {
                var saveDialog = new SaveFileDialog();
                saveDialog.AddExtension = true;
                saveDialog.DefaultExt = ".xml";
                saveDialog.Filter = "XML Файл|*.xml";
                if (saveDialog.ShowDialog() == true)
                {
                    var fileName = saveDialog.FileName;
                    if (fileName.EndsWith(".xml"))
                    {
                        XElement root = new XElement("Records");
                        foreach (var record in RingUpList)
                        {
                            root.AddFirst(
                                new XElement("Record",
                                    new[]
                                    {
                                        new XElement("Id", record.Id),
                                        new XElement("Статус", record.State),
                                        new XElement("Дата", record.CallTime.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("Название", record.Name),
                                        new XElement("Телефон", record.FromPhone),
                                        new XElement("ВсегоНомеров", record.PhoneCount),
                                        new XElement("Ответило", record.DoneCalls),
                                        new XElement("НеОтветило", record.NotDoneCalls),
                                        new XElement("ДатаНачала", record.StartTime?.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("ДатаОкончания", record.EndTime?.ToString("dd.MM.yyyy HH:mm")),
                                    }));
                        }
                        var saver = new FileStream(fileName, FileMode.Create);
                        root.Save(saver);
                        saver.Close();
                    }
                    MessageBox.Show("Данные сохранены в файл\r\n" + fileName);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Произошла ошибка:\r\n" + exc.Message);
            }

        }
        private void NewRingUp()
        {
            var model = new RingUpNewDialogViewModel(RequestService);
            var view = new RingUpNewDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                Refresh();
            }
        }

        public RingUpHistoryDto CurrentRingUp
        {
            get { return _currentRingUp; }
            set { _currentRingUp = value;
                SetRingUpInfo(value);
                OnPropertyChanged(nameof(CurrentRingUp));}
        }

        private void SetRingUpInfo(RingUpHistoryDto value)
        {
            if(value == null)
                return;
            RingUpInfoList = new ObservableCollection<RingUpInfoDto>(_requestService.GetRingUpInfo(value.Id));
        }

        public ObservableCollection<RingUpHistoryDto> RingUpList
        {
            get { return _ringUpList; }
            set { _ringUpList = value; OnPropertyChanged(nameof(RingUpList)); }
        }

        public ObservableCollection<RingUpInfoDto> RingUpInfoList
        {
            get { return _ringUpInfoList; }
            set { _ringUpInfoList = value; OnPropertyChanged(nameof(RingUpInfoList));}
        }

        private void EditPhone()
        {

            var model = new PhoneDialogViewModel(RequestService);
            var view = new PhoneAddOrUpdateDialog();
            model.SetView(view);
            view.Owner = Application.Current.MainWindow;
            view.DataContext = model;
            if (view.ShowDialog() == true)
            {
                Refresh();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh()
        {
            RingUpInfoList = new ObservableCollection<RingUpInfoDto>();
            RingUpList = new ObservableCollection<RingUpHistoryDto>(RequestService.GetRingUpHistory(FromDate));
        }
    }
}
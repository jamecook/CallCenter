using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using CRMPhone.Dialogs;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class ReportControlModel : INotifyPropertyChanged
    {
        private RequestService _requestService;
        
        private ICommand _callsStatCommand;
        public ICommand CallsStatCommand { get { return _callsStatCommand ?? (_callsStatCommand = new RelayCommand(CallsStat)); } }
        private ICommand _callsIvrStatCommand;
        public ICommand CallsIvrStatCommand { get { return _callsIvrStatCommand ?? (_callsIvrStatCommand = new RelayCommand(CallsIvrStat)); } }

        private void CallsStat(object sender)
        {
            var model = new SelectPeriodDialogViewModel();
            model.FromDate = DateTime.Today.AddDays(-7);
            model.ToDate = DateTime.Today;
            var view = new SelectPeriodDialog();
            view.Owner = Application.Current.MainWindow;
            model.SetView(view);
            view.DataContext = model;
            if (view.ShowDialog()??false)
            {
                try
                {

                    var saveDialog = new SaveFileDialog();
                    saveDialog.AddExtension = true;
                    saveDialog.DefaultExt = ".xml";
                    saveDialog.Filter = "XML Файл|*.xml";
                    if (saveDialog.ShowDialog() == true)
                    {
                        List<StatCallListDto> results;
                        using (var con = new MySqlConnection(AppSettings.ConnectionString))
                        { 
                            _requestService = new RequestService(AppSettings.DbConnection);
                        results = _requestService.GetStatCalls(model.FromDate.Date,
                            model.ToDate.Date.AddDays(1).AddSeconds(-1)).ToList();
                        }
                        var fileName = saveDialog.FileName;
                        XElement root = new XElement("Records");
                        foreach (var record in results)
                        {
                            root.AddFirst(
                                new XElement("Record",
                                    new[]
                                    {
                                        new XElement("Направление", record.Direction),
                                        new XElement("Exten", record.Exten),
                                        new XElement("УК", record.ServiceCompany),
                                        new XElement("Номер", record.PhoneNum),
                                        new XElement("ДатаЗвонка", record.CreateDate),
                                        new XElement("ВремяЗвонка", record.CreateTime),
                                        new XElement("ДатаОтвета", record.BridgeDate),
                                        new XElement("ВремяОтвета", record.BridgeTime),
                                        new XElement("ДатаОкончания", record.EndDate),
                                        new XElement("ВремяОкончания", record.EndTime),
                                        new XElement("Ожидание", record.WaitSec),
                                        new XElement("Продолжительность", record.CallTime),
                                        new XElement("Оператор", record.Fio)
                                    }));
                        }
                        var saver = new FileStream(fileName, FileMode.Create);
                        root.Save(saver);
                        saver.Close();
                        MessageBox.Show("Данные сохранены в файл\r\n" + fileName);
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Произошла ошибка:\r\n" + exc.Message);
                }

            }
        }
        private void CallsIvrStat(object sender)
        {
            var model = new SelectPeriodDialogViewModel();
            model.FromDate = DateTime.Today.AddDays(-7);
            model.ToDate = DateTime.Today;
            var view = new SelectPeriodDialog();
            view.Owner = Application.Current.MainWindow;
            model.SetView(view);
            view.DataContext = model;
            if (view.ShowDialog()??false)
            {
                try
                {

                    var saveDialog = new SaveFileDialog();
                    saveDialog.AddExtension = true;
                    saveDialog.DefaultExt = ".xml";
                    saveDialog.Filter = "XML Файл|*.xml";
                    if (saveDialog.ShowDialog() == true)
                    {
                        List<StatIvrCallListDto> results;
                        using (var con = new MySqlConnection(AppSettings.ConnectionString))
                        { 
                            _requestService = new RequestService(AppSettings.DbConnection);
                        results = _requestService.GetIvrStatCalls(model.FromDate.Date,
                            model.ToDate.Date.AddDays(1).AddSeconds(-1)).ToList();
                        }
                        var fileName = saveDialog.FileName;
                        XElement root = new XElement("Records");
                        foreach (var record in results)
                        {
                            root.AddFirst(
                                new XElement("Record",
                                    new[]
                                    {
                                        new XElement("ID", record.LinkedId),
                                        new XElement("Номер", record.CallerIdNum),
                                        new XElement("ВхЗвонок", record.InCreateTime.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("ОтветНаВх", record.InBridgedTime?.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("ОкончанияВх", record.InEndTime.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("ВнутреннийНомер", record.Phone),
                                        new XElement("Ответ", record.Result),
                                        new XElement("ЗвонокВнут", record.CreateTime.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("ОтветВнут", record.BridgedTime?.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("ОкончанияВнут", record.EndTime.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("Ожидание", record.ClientWaitSec),
                                        new XElement("ВремяЗвонка", record.CallDuration),
                                        new XElement("ВремяРазговора", record.TalkDuration),
                                    }));
                        }
                        var saver = new FileStream(fileName, FileMode.Create);
                        root.Save(saver);
                        saver.Close();
                        MessageBox.Show("Данные сохранены в файл\r\n" + fileName);
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Произошла ошибка:\r\n" + exc.Message);
                }

            }
        }

        public ReportControlModel()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
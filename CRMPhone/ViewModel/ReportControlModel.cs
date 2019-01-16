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
                    saveDialog.Filter = "XML ����|*.xml";
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
                                        new XElement("�����������", record.Direction),
                                        new XElement("Exten", record.Exten),
                                        new XElement("��", record.ServiceCompany),
                                        new XElement("�����", record.PhoneNum),
                                        new XElement("����������", record.CreateDate),
                                        new XElement("�����������", record.CreateTime),
                                        new XElement("����������", record.BridgeDate),
                                        new XElement("�����������", record.BridgeTime),
                                        new XElement("�������������", record.EndDate),
                                        new XElement("��������������", record.EndTime),
                                        new XElement("��������", record.WaitSec),
                                        new XElement("�����������������", record.CallTime),
                                        new XElement("��������", record.Fio)
                                    }));
                        }
                        var saver = new FileStream(fileName, FileMode.Create);
                        root.Save(saver);
                        saver.Close();
                        MessageBox.Show("������ ��������� � ����\r\n" + fileName);
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("��������� ������:\r\n" + exc.Message);
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
                    saveDialog.Filter = "XML ����|*.xml";
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
                                        new XElement("�����", record.CallerIdNum),
                                        new XElement("��������", record.InCreateTime.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("���������", record.InBridgedTime?.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("�����������", record.InEndTime.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("���������������", record.Phone),
                                        new XElement("�����", record.Result),
                                        new XElement("����������", record.CreateTime.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("���������", record.BridgedTime?.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("�������������", record.EndTime.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("��������", record.ClientWaitSec),
                                        new XElement("�����������", record.CallDuration),
                                        new XElement("��������������", record.TalkDuration),
                                    }));
                        }
                        var saver = new FileStream(fileName, FileMode.Create);
                        root.Save(saver);
                        saver.Close();
                        MessageBox.Show("������ ��������� � ����\r\n" + fileName);
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("��������� ������:\r\n" + exc.Message);
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel.Admins
{
    public class RingUpNewDialogViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private RequestService _requestService;
        private ObservableCollection<RingUpConfigDto> _configList;
        private RingUpConfigDto _selectedConfig;
        private List<RingUpImportDto> _importedRecords;
        private List<RingUpImportDto> _errorRecords;
        public ObservableCollection<RingUpConfigDto> ConfigList
        {
            get { return _configList; }
            set { _configList = value; OnPropertyChanged(nameof(ConfigList));}
        }

        public RingUpConfigDto SelectedConfig
        {
            get { return _selectedConfig; }
            set { _selectedConfig = value; OnPropertyChanged(nameof(SelectedConfig));}
        }


        public RingUpNewDialogViewModel(RequestService requestService)
        {
            _requestService = requestService;
            _importedRecords = new List<RingUpImportDto>();
            _errorRecords = new List<RingUpImportDto>();
            ConfigList = new ObservableCollection<RingUpConfigDto>(_requestService.GetRingUpConfigs());
            SelectedConfig = ConfigList.FirstOrDefault();
            RecordCount = 0;
        }

        public int RecordCount
        {
            get { return _recordCount; }
            set { _recordCount = value; OnPropertyChanged(nameof(RecordCount));}
        }

        public void SetView(Window view)
        {
            _view = view;
        }
        private ICommand _saveCommand;

        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }
        private ICommand _cancelCommand;
        public ICommand CancelCommand { get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(Cancel)); } }


        private ICommand _loadCommand;
        private int _recordCount;
        public ICommand LoadCommand { get { return _loadCommand ?? (_loadCommand = new RelayCommand(Load)); } }

        private void Load(object obj)
        {
            var dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".xlsx";
            dialog.Filter = "Excel файл|*.xlsx";
            if (dialog.ShowDialog() == true)
            {
                _importedRecords = new List<RingUpImportDto>();
                try
                {
                using (var document = SpreadsheetDocument.Open(dialog.FileName, true))
                {
                    WorkbookPart workbookPart = document.WorkbookPart;
                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
                    var sharedStrings = new List<string>();
                    var sharedStringTablePart = workbookPart.SharedStringTablePart;
                    if (sharedStringTablePart != null)
                    {
                        var sharedStringTable = sharedStringTablePart.SharedStringTable;
                        foreach (var str in sharedStringTable)
                        {
                            sharedStrings.Add(str.InnerText);
                        }
                    }
                    var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    var rows = sheetData.Elements<Row>();
                    foreach (var row in rows)
                    {
                        var cells = row.Elements<Cell>();
                        var item = new RingUpImportDto();
                        item.RowIndex = row.RowIndex.Value;
                        foreach (var cell in cells)
                        {
                            var str = string.Empty;
                            if (cell.DataType == "s")
                                str = sharedStrings[Convert.ToInt32(cell.CellValue.Text)];
                            else
                                str = cell.CellValue?.Text;
                            if (cell.CellReference.Value.StartsWith("A"))
                                item.Phone = str?.Trim();
                            else if (cell.CellReference.Value.StartsWith("B"))
                                item.Dolg = str?.Trim();
                        }
                        _importedRecords.Add(item);
                    }
                }
                }
                catch (IOException exc)
                {
                    MessageBox.Show(exc.Message);
                }

                _errorRecords = new List<RingUpImportDto>();
                foreach (var item in _importedRecords)
                {
                    if (string.IsNullOrEmpty(item.Dolg))
                    {
                        _errorRecords.Add(item);
                    }
                    else if (string.IsNullOrEmpty(item.Phone) || (item.Phone.Length!= 6 && item.Phone.Length != 10 && item.Phone.Length != 11) || item.Phone.StartsWith("+"))
                    {
                        _errorRecords.Add(item);
                    }
                }
                if (_errorRecords.Count > 0)
                {
                    var errorMessage = "Обнаружены ошибки в формате данных для следующих записей:\r\n" +
                                       _errorRecords.Select(e => $"№ строки {e.RowIndex}. Телефон {e.Phone}. Долг {e.Dolg}.")
                                           .Aggregate((i, j) => i +"\r\n"+ j);
                    MessageBox.Show(errorMessage);
                    MessageBox.Show("Долг должен быть отличен от нуля, номер телефона должен состоять из 6, 10 или 11 символов!");
                }
                RecordCount = _importedRecords.Count;
            }
        }

        private void Cancel(object obj)
        {
            _view.DialogResult = false;
        }

        private void Save(object sender)
        {
            if (_importedRecords.Count > 0 && _errorRecords.Count > 0)
            {
                var errorMessage = "Обнаружены ошибки в формате данных для следующих записей:\r\n" +
                                   _errorRecords.Select(e => $"№ строки {e.RowIndex}. Телефон {e.Phone}. Долг {e.Dolg}.")
                                       .Aggregate((i, j) => i + "\r\n" + j);
                MessageBox.Show(errorMessage);
                MessageBox.Show("Загрузка невозможна!");
                return;
            }
            _requestService.SaveRingUpList(SelectedConfig.Id,_importedRecords);
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
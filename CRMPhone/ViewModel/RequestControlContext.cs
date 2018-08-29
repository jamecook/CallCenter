using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using Stimulsoft.Report;

namespace CRMPhone.ViewModel
{
    public class RequestControlContext : INotifyPropertyChanged
    {
        private ObservableCollection<RequestForListDto> _requestList;
        private RequestServiceImpl.RequestService _requestService;

        private ICommand _addRequestCommand;
        public ICommand AddRequestCommand { get { return _addRequestCommand ?? (_addRequestCommand = new CommandHandler(AddRequest, true)); } }
        private ICommand _refreshRequestCommand;
        public ICommand RefreshRequestCommand { get { return _refreshRequestCommand ?? (_refreshRequestCommand = new CommandHandler(RefreshRequest, true)); } }
        private ICommand _exportRequestCommand;
        public ICommand ExportRequestCommand { get { return _exportRequestCommand ?? (_exportRequestCommand = new CommandHandler(ExportRequest, true)); } }
        private ICommand _printActsCommand;
        public ICommand PrintActsCommand { get { return _printActsCommand ?? (_printActsCommand = new CommandHandler(PrintActs, true)); } }
        private ICommand _clearFiltersCommand;
        public ICommand ClearFiltersCommand { get { return _clearFiltersCommand ?? (_clearFiltersCommand = new CommandHandler(ClearFilters, true)); } }

        private ICommand _playCommand;
        public ICommand PlayCommand { get { return _playCommand ?? (_playCommand = new RelayCommand(RecordPlay)); } }

        private void RecordPlay(object obj)
        {
            var item = obj as RequestForListDto;
            if (item == null)
                return;
            var serverIpAddress = ConfigurationManager.AppSettings["CallCenterIP"];
            var fileName = _requestService.GetRecordFileNameByUniqueId(item.RecordUniqueId);
            _requestService.PlayRecord(serverIpAddress, fileName);

            /*
            var localFileName = fileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\").Replace("/","\\");
            var localFileNameMp3 = localFileName.Replace(".wav",".mp3");
            if(File.Exists(localFileNameMp3))
                Process.Start(localFileNameMp3);
            else if(File.Exists(localFileNameMp3))
                Process.Start(localFileName);
            else
                MessageBox.Show($"Файл с записью недоступен!\r\n{localFileNameMp3}", "Ошибка");
                */
        }


        private void ClearFilters()
        {
            RequestNum = string.Empty;
            SelectedPayment = null;
            ServiceCompanyBadWork = false;
            OnlyRetry = false;
            OnlyGaranty = false;
            ClientPhone = "";
            foreach (var status in FilterStatusList)
            {
                status.Selected = false;
            }
            StatusText = "";

            foreach (var serviceCompany in FilterServiceCompanyList)
            {
                serviceCompany.Selected = false;
            }
            ServiceCompanyText = "";

            foreach (var worker in FilterMasterList)
            {
                worker.Selected = false;
            }
            MasterText = "";

            foreach (var worker in FilterExecuterList)
            {
                worker.Selected = false;
            }
            ExecuterText = "";
            foreach (var user in FilterUserList)
            {
                user.Selected = false;
            }
            UserText = "";
            foreach (var rating in FilterRatingList)
            {
                rating.Selected = false;
            }
            RatingText = "";
            foreach (var street in FilterStreetList)
            {
                street.Selected = false;
            }
            StreetText = "";
            foreach (var service in FilterParentServiceList)
            {
                service.Selected = false;
            }
            ParentServiceText = "";
            RefreshRequest();
        }

        public string ParentServiceText
        {
            get { return _parentServiceText; }
            set { _parentServiceText = value; OnPropertyChanged(nameof(ParentServiceText));}
        }

        public string StreetText
        {
            get { return _streetText; }
            set { _streetText = value; OnPropertyChanged(nameof(StreetText));}
        }

        public string RatingText
        {
            get { return _ratingText; }
            set { _ratingText = value; OnPropertyChanged(nameof(RatingText)); }
        }

        public string UserText
        {
            get { return _userText; }
            set { _userText = value; OnPropertyChanged(nameof(UserText)); }
        }

        public string ServiceCompanyText
        {
            get { return _serviceCompanyText; }
            set { _serviceCompanyText = value; OnPropertyChanged(nameof(ServiceCompanyText)); }
        }

        public string StatusText
        {
            get { return _statusText; }
            set { _statusText = value; OnPropertyChanged(nameof(StatusText)); }
        }

        public string ExecuterText
        {
            get { return _executerText; }
            set { _executerText = value; OnPropertyChanged(nameof(ExecuterText)); }
        }

        public string MasterText
        {
            get { return _masterText; }
            set { _masterText = value; OnPropertyChanged(nameof(MasterText)); }
        }

        private void PrintActs()
        {
            if (RequestList.Count == 0)
            {
                MessageBox.Show("Нельзя экспортировать пустой список!", "Ошибка");
                return;
            }
            try
            {

                var saveDialog = new SaveFileDialog();
                saveDialog.AddExtension = true;
                saveDialog.DefaultExt = ".doc";
                saveDialog.Filter = "Word файл|*.doc";
                if (saveDialog.ShowDialog() == true)
                {
                    var fileName = saveDialog.FileName;
                    if (fileName.EndsWith(".doc"))
                    {
                        var stiReport = new StiReport();
                        stiReport.Load("templates\\act.mrt");
                        var requestsDto = RequestList.Select(x => new
                        {
                            RequestNumber = x.Id,
                            Address = x.FullAddress,
                            ClientName = x.MainFio,
                            ClientPhones = x.ContactPhones,
                            ParentService = x.ParentService,
                            Service = x.Service,
                        }).ToArray();
                        StiOptions.Engine.HideRenderingProgress = true;
                        //StiOptions.Engine.HideExceptions = true;
                        StiOptions.Engine.HideMessages = true;


                        stiReport.RegBusinessObject("", "Acts", requestsDto);
                        stiReport.Render();
                        var reportStream = new MemoryStream();
                        stiReport.ExportDocument(StiExportFormat.Rtf, reportStream);
                        //stiReport.ExportDocument(StiExportFormat.Pdf, reportStream);
                        reportStream.Position = 0;
                        File.WriteAllBytes(fileName, reportStream.GetBuffer());

                        MessageBox.Show("Данные сохранены в файл\r\n" + fileName);
                    }
                }
            }
            catch
                (Exception exc)
            {
                MessageBox.Show("Произошла ошибка:\r\n" + exc.Message);
            }

        }
        private void ExportRequest()
        {
            if (RequestList.Count == 0)
            {
                MessageBox.Show("Нельзя экспортировать пустой список!","Ошибка");
                return;
            }
            try
            {

                var saveDialog = new SaveFileDialog();
                saveDialog.AddExtension = true;
                saveDialog.DefaultExt = ".xlsx";
                saveDialog.Filter = "Excel файл|*.xlsx|XML Файл|*.xml";
                if (saveDialog.ShowDialog() == true)
                {
                    var fileName = saveDialog.FileName;
                    if (fileName.EndsWith(".xml"))
                    {
                        XElement root = new XElement("Records");
                        foreach (var request in RequestList)
                        {
                            root.AddFirst(
                                new XElement("Record",
                                    new[]
                                    {
                                        new XElement("Заявка", request.Id),
                                        new XElement("Статус", request.Status),
                                        new XElement("ДатаСоздания", request.CreateTime.ToString("dd.MM.yyyy HH:mm")),
                                        new XElement("Создатель", request.CreateUser.ShortName),
                                        new XElement("Улица", request.StreetName),
                                        new XElement("Дом", request.Building),
                                        new XElement("Корпус", request.Corpus),
                                        new XElement("Квартира", request.Flat),
                                        new XElement("Телефоны", request.ContactPhones),
                                        new XElement("ФИО", request.MainFio),
                                        new XElement("Услуга", request.ParentService),
                                        new XElement("Причина", request.Service),
                                        new XElement("Примечание", request.Description),
                                        new XElement("Дата", request.ExecuteTime?.Date.ToString("dd.MM.yyyy") ?? ""),
                                        new XElement("Время", request.ExecutePeriod),
                                        new XElement("Мастер", request.Master?.ShortName),
                                        new XElement("Исполнитель", request.Executer?.ShortName),
                                        new XElement("ВыполнениеС", request.FromTime?.ToString("HH:mm:ss") ?? ""),
                                        new XElement("ВыполнениеПо", request.ToTime?.ToString("HH:mm:ss") ?? ""),
                                        new XElement("ПотраченоВремени", request.SpendTime),
                                        new XElement("Гарантийная", request.GarantyTest),
                                        new XElement("Оценка", request.Rating),
                                        new XElement("Комментарий_К_Оценке", request.RatingDescription),
                                        new XElement("Повторная", request.IsRetry?"Да":""),
                                        new XElement("Последний_Комментарий_исполнителя", request.LastNote),
                                    }));
                        }
                        var saver = new FileStream(fileName, FileMode.Create);
                        root.Save(saver);
                        saver.Close();
                    }
                    if (fileName.EndsWith(".xlsx"))
                    {
                        File.Copy("templates\\requests.xlsx",fileName,true);
                        CreateExcelDocByTemplate(fileName);
                    }
                    MessageBox.Show("Данные сохранены в файл\r\n" + fileName);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Произошла ошибка:\r\n" + exc.Message);
            }

        }

        public void CreateExcelDoc(string fileName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet();

                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                
                Sheet sheet = new Sheet() {Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Export"};

                sheets.Append(sheet);

                workbookPart.Workbook.Save();

                SheetData sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                // Constructing header
                Row row = new Row();

                row.Append(
                    ConstructCell("Заявка", CellValues.String),
                    ConstructCell("Статус", CellValues.String),
                    ConstructCell("Дата Создания", CellValues.String),
                    ConstructCell("Создатель", CellValues.String),
                    ConstructCell("Улица", CellValues.String),
                    ConstructCell("Дом", CellValues.String),
                    ConstructCell("Корпус", CellValues.String),
                    ConstructCell("Квартира", CellValues.String),
                    ConstructCell("Телефоны", CellValues.String),
                    ConstructCell("Услуга", CellValues.String),
                    ConstructCell("Причина", CellValues.String),
                    ConstructCell("Примечание", CellValues.String),
                    ConstructCell("Дата", CellValues.String),
                    ConstructCell("Время", CellValues.String),
                    ConstructCell("Мастер", CellValues.String),
                    ConstructCell("Исполнитель", CellValues.String),
                    ConstructCell("Выполнение С", CellValues.String),
                    ConstructCell("Выполнение По", CellValues.String),
                    ConstructCell("Потрачено Времени", CellValues.String),
                    ConstructCell("Гарантийная", CellValues.String),
                    ConstructCell("Оценка", CellValues.String),
                    ConstructCell("Комментарий К Оценке", CellValues.String),
                    ConstructCell("Повторная", CellValues.String)
                );
                // Insert the header row to the Sheet Data
                sheetData.AppendChild(row);
                // Inserting each employee
                foreach (var request in RequestList)
                {
                    {
                        row = new Row();

                        row.Append(
                            ConstructCell(request.Id.ToString(), CellValues.Number),
                            ConstructCell(request.Status, CellValues.String),
                            ConstructCell(request.CreateTime.ToString("dd.MM.yyyy HH:mm"), CellValues.String),
                            ConstructCell(request.CreateUser.ShortName, CellValues.String),
                            ConstructCell(request.StreetName, CellValues.String),
                            ConstructCell(request.Building, CellValues.String),
                            ConstructCell(request.Corpus, CellValues.String),
                            ConstructCell(request.Flat, CellValues.String),
                            ConstructCell(request.ContactPhones, CellValues.String),
                            ConstructCell(request.ParentService, CellValues.String),
                            ConstructCell(request.Service, CellValues.String),
                            ConstructCell(request.Description, CellValues.String),
                            ConstructCell(request.ExecuteTime?.Date.ToString("dd.MM.yyyy") ?? "", CellValues.String),
                            ConstructCell(request.ExecutePeriod, CellValues.String),
                            ConstructCell(request.Master?.ShortName, CellValues.String),
                            ConstructCell(request.Executer?.ShortName, CellValues.String),
                            ConstructCell(request.FromTime?.ToString("HH:mm:ss") ?? "", CellValues.String),
                            ConstructCell(request.ToTime?.ToString("HH:mm:ss") ?? "", CellValues.String),
                            ConstructCell(request.SpendTime, CellValues.String),
                            ConstructCell(request.GarantyTest, CellValues.String),
                            ConstructCell(request.Rating, CellValues.String),
                            ConstructCell(request.RatingDescription, CellValues.String),
                            ConstructCell(request.IsRetry ? "Да" : "", CellValues.String));

                        sheetData.AppendChild(row);
                    }
                    worksheetPart.Worksheet.Save();
                }
            }
        }
        public void CreateExcelDocByTemplate(string fileName)
        {
            //using (SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook)
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, true)
            )
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
                //Sheet sheet = document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().SingleOrDefault(s => s.Name == "Export");
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Inserting each rows
                foreach (var request in RequestList)
                {
                    {
                        var row = new Row();
                        row.Append(
                            ConstructCell(request.Id.ToString(), CellValues.Number),
                            ConstructCell(request.Status, CellValues.String),
                            ConstructCell(request.CreateTime.ToString("dd.MM.yyyy HH:mm"), CellValues.String),
                            ConstructCell(request.CreateUser.ShortName, CellValues.String),
                            ConstructCell(request.StreetName, CellValues.String),
                            ConstructCell(request.Building, CellValues.String),
                            ConstructCell(request.Corpus, CellValues.String),
                            ConstructCell(request.Flat, CellValues.String),
                            ConstructCell(request.ContactPhones, CellValues.String),
                            ConstructCell(request.MainFio, CellValues.String),
                            ConstructCell(request.ParentService, CellValues.String),
                            ConstructCell(request.Service, CellValues.String),
                            ConstructCell(request.Description, CellValues.String),
                            ConstructCell(request.ExecuteTime?.Date.ToString("dd.MM.yyyy") ?? "", CellValues.String),
                            ConstructCell(request.ExecutePeriod, CellValues.String),
                            ConstructCell(request.Master?.ShortName, CellValues.String),
                            ConstructCell(request.Executer?.ShortName, CellValues.String),
                            ConstructCell(request.FromTime?.ToString("HH:mm:ss") ?? "", CellValues.String),
                            ConstructCell(request.ToTime?.ToString("HH:mm:ss") ?? "", CellValues.String),
                            ConstructCell(request.SpendTime, CellValues.String),
                            ConstructCell(request.GarantyTest, CellValues.String),
                            ConstructCell(request.Rating, CellValues.String),
                            ConstructCell(request.RatingDescription, CellValues.String),
                            ConstructCell(request.IsRetry ? "Да" : "", CellValues.String),
                            ConstructCell(request.LastNote, CellValues.String));

                        sheetData.AppendChild(row);
                    }
                    worksheetPart.Worksheet.Save();
                }
            }
        }

        private Cell ConstructCell(string value, CellValues dataType)
        {
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType),
            };
        }

        private ICommand _openRequestCommand;
        private DateTime _fromDate;
        private DateTime _toDate;
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;
        private ObservableCollection<FlatDto> _flatList;
        private FlatDto _selectedFlat;
        private ObservableCollection<ServiceDto> _serviceList;
        private ServiceDto _selectedService;
        private string _requestNum;
        private int _requestCount;
        private DateTime _executeFromDate;
        private DateTime _executeToDate;
        private bool _filterByCreateDate;
        private ObservableCollection<FieldForFilterDto> _filterMasterList;
        private ObservableCollection<PaymentDto> _paymentList;
        private PaymentDto _selectedPayment;
        private bool _serviceCompanyBadWork;
        private bool _onlyRetry;
        private string _clientPhone;
        private ObservableCollection<FieldForFilterDto> _filterStatusList;
        private string _statusText;
        private string _masterText;
        private ObservableCollection<FieldForFilterDto> _filterServiceCompanyList;
        private string _serviceCompanyText;
        private string _executerText;
        private ObservableCollection<FieldForFilterDto> _filterExecuterList;
        private ObservableCollection<FieldForFilterDto> _filterUserList;
        private string _userText;
        private ObservableCollection<FieldForFilterDto> _filterRatingList;
        private string _ratingText;
        private ObservableCollection<FieldForFilterDto> _filterStreetList;
        private string _streetText;
        private ObservableCollection<FieldForFilterDto> _filterParentServiceList;
        private string _parentServiceText;
        private bool _onlyGaranty;

        public ICommand OpenRequestCommand { get { return _openRequestCommand ?? (_openRequestCommand = new RelayCommand(OpenRequest));} }

        public string RequestNum
        {
            get { return _requestNum; }
            set { _requestNum = value; OnPropertyChanged(nameof(RequestNum));}
        }

        public ObservableCollection<FieldForFilterDto> FilterStreetList
        {
            get { return _filterStreetList; }
            set { _filterStreetList = value; OnPropertyChanged(nameof(FilterStreetList));}
        }

        public ObservableCollection<HouseDto> HouseList
        {
            get { return _houseList; }
            set { _houseList = value; OnPropertyChanged(nameof(HouseList)); }
        }

        public HouseDto SelectedHouse
        {
            get { return _selectedHouse; }
            set
            {
                _selectedHouse = value;
                ChangeHouse(value?.Id);
                OnPropertyChanged(nameof(SelectedHouse));
            }
        }

        public ObservableCollection<FlatDto> FlatList
        {
            get { return _flatList; }
            set { _flatList = value; OnPropertyChanged(nameof(FlatList)); }
        }

        public FlatDto SelectedFlat
        {
            get { return _selectedFlat; }
            set { _selectedFlat = value; OnPropertyChanged(nameof(SelectedFlat)); }
        }

        private void ChangeStreet(int? streetId)
        {
            HouseList.Clear();
            if (!streetId.HasValue)
                return;
            foreach (var house in _requestService.GetHouses(streetId.Value).OrderBy(s => s.Building?.PadLeft(6, '0')).ThenBy(s => s.Corpus?.PadLeft(6, '0')))
            {
                HouseList.Add(house);
            }
            OnPropertyChanged(nameof(HouseList));
        }

        public ObservableCollection<FieldForFilterDto> FilterParentServiceList
        {
            get { return _filterParentServiceList; }
            set { _filterParentServiceList = value; OnPropertyChanged(nameof(FilterParentServiceList));}
        }

        public ObservableCollection<ServiceDto> ServiceList
        {
            get { return _serviceList; }
            set { _serviceList = value; OnPropertyChanged(nameof(ServiceList)); }
        }

        public ServiceDto SelectedService
        {
            get { return _selectedService; }
            set { _selectedService = value; OnPropertyChanged(nameof(SelectedService)); }
        }

        public ObservableCollection<FieldForFilterDto> FilterExecuterList
        {
            get { return _filterExecuterList; }
            set { _filterExecuterList = value; OnPropertyChanged(nameof(FilterExecuterList));}
        }

        public ObservableCollection<FieldForFilterDto> FilterMasterList
        {
            get { return _filterMasterList; }
            set { _filterMasterList = value; OnPropertyChanged(nameof(FilterMasterList));}
        }

        public ObservableCollection<FieldForFilterDto> FilterStatusList
        {
            get { return _filterStatusList; }
            set { _filterStatusList = value; OnPropertyChanged(nameof(FilterStatusList)); }
        }

        public ObservableCollection<FieldForFilterDto> FilterRatingList
        {
            get { return _filterRatingList; }
            set { _filterRatingList = value; OnPropertyChanged(nameof(FilterRatingList));}
        }

        public ObservableCollection<FieldForFilterDto> FilterUserList
        {
            get { return _filterUserList; }
            set { _filterUserList = value; OnPropertyChanged(nameof(FilterUserList)); }
        }

        public ObservableCollection<FieldForFilterDto> FilterServiceCompanyList
        {
            get { return _filterServiceCompanyList; }
            set { _filterServiceCompanyList = value; OnPropertyChanged(nameof(FilterServiceCompanyList));}
        }

        public ObservableCollection<PaymentDto> PaymentList
        {
            get { return _paymentList; }
            set { _paymentList = value; OnPropertyChanged(nameof(PaymentList));}
        }

        public bool OnlyRetry
        {
            get { return _onlyRetry; }
            set { _onlyRetry = value; OnPropertyChanged(nameof(OnlyRetry));}
        }

        public bool OnlyGaranty
        {
            get { return _onlyGaranty; }
            set { _onlyGaranty = value; OnPropertyChanged(nameof(OnlyGaranty)); }
        }

        public string ClientPhone
        {
            get { return _clientPhone; }
            set { _clientPhone = value; OnPropertyChanged(nameof(ClientPhone)); }
        }

        public bool ServiceCompanyBadWork
        {
            get { return _serviceCompanyBadWork; }
            set { _serviceCompanyBadWork = value; OnPropertyChanged(nameof(ServiceCompanyBadWork));}
        }

        public PaymentDto SelectedPayment
        {
            get { return _selectedPayment; }
            set { _selectedPayment = value; OnPropertyChanged(nameof(SelectedPayment));}
        }

        private void ChangeParentService(int? parentServiceId)
        {
            ServiceList.Clear();
            if (!parentServiceId.HasValue)
                return;
            foreach (var source in _requestService.GetServices(parentServiceId.Value).OrderBy(s => s.Name))
            {
                ServiceList.Add(source);
            }
            OnPropertyChanged(nameof(ServiceList));
        }

        private void ChangeHouse(int? houseId)
        {
            FlatList.Clear();
            if (!houseId.HasValue)
                return;
            foreach (var flat in _requestService.GetFlats(houseId.Value).OrderBy(s => s.TypeId).ThenBy(s => s.Flat?.PadLeft(6, '0')))
            {
                FlatList.Add(flat);
            }
            OnPropertyChanged(nameof(FlatList));
        }

        private void ChangeCity(int? cityId)
        {
            foreach (var street in FilterStreetList)
            {
                street.PropertyChanged -= StreetOnPropertyChanged;
            }
            FilterStreetList.Clear();
            if (!cityId.HasValue)
                return;
            foreach (var street in _requestService.GetStreets(cityId.Value).OrderBy(s => s.Name).Select(w => new FieldForFilterDto()
            {
                Id = w.Id,
                Name = w.NameWithPrefix,
                Selected = false
            }))
            {
                FilterStreetList.Add(street);
            }
            foreach (var street in FilterStreetList)
            {
                street.PropertyChanged += StreetOnPropertyChanged;
            }
            OnPropertyChanged(nameof(FilterStreetList));
        }

        private void StreetOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var item = sender as FieldForFilterDto;
            if (item != null)
            {
                if (FilterStreetList.Count(f => f.Selected) == 1)
                {
                    ChangeStreet(FilterStreetList.FirstOrDefault(f => f.Selected)?.Id);
                }
                else
                {
                    ChangeStreet(null);
                }
            }

        }

        private void OpenRequest(object sender)
        {
            var selectedItem = sender as RequestForListDto;
            if (selectedItem == null)
                return;
            if (_requestService == null)
                _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);

            var request = _requestService.GetRequest(selectedItem.Id);
            if (request == null)
            {
                MessageBox.Show("Произошла непредвиденная ошибка!");
                return;
            }

            var viewModel = new RequestDialogViewModel();
            var view = new RequestDialog(viewModel);
            viewModel.SetView(view);
            viewModel.RequestId = request.Id;
            viewModel.SelectedCity = viewModel.CityList.SingleOrDefault(i=>i.Id == request.Address.CityId);
            viewModel.SelectedStreet = viewModel.StreetList.SingleOrDefault(i => i.Id == request.Address.StreetId);
            viewModel.SelectedHouse = viewModel.HouseList.SingleOrDefault(i=>i.Id == request.Address.HouseId);
            viewModel.SelectedFlat =  viewModel.FlatList.SingleOrDefault(i=>i.Id == request.Address.Id);
            viewModel.Floor = request.Floor;
            viewModel.Entrance = request.Entrance;
            viewModel.FromTime = request.FromTime;
            viewModel.ToTime = request.ToTime;
            var requestModel = viewModel.RequestList.FirstOrDefault();
            requestModel.SelectedParentService = requestModel.ParentServiceList.SingleOrDefault(i => i.Id == request.Type.ParentId);
            if ( requestModel.SelectedParentService == null)
            {
                var parrentServiceType = _requestService.GetServiceById(request.Type.ParentId ?? 0);
                requestModel.ParentServiceList.Add(parrentServiceType);
                requestModel.SelectedParentService = requestModel.ParentServiceList.SingleOrDefault(i => i.Id == request.Type.ParentId);
            }
            requestModel.SelectedService = requestModel.ServiceList.SingleOrDefault(i => i.Id == request.Type.Id);
            if (requestModel.SelectedService == null)
            {
                var serviceType = _requestService.GetServiceById(request.Type.Id);
                requestModel.ParentServiceList.Add(serviceType);
                requestModel.SelectedService = requestModel.ServiceList.SingleOrDefault(i => i.Id == request.Type.Id);
            }

            requestModel.Description = request.Description;
            requestModel.IsChargeable = request.IsChargeable;
            requestModel.IsImmediate = request.IsImmediate;
            requestModel.IsBadWork = request.IsBadWork;
            requestModel.IsRetry = request.IsRetry;
            requestModel.Gatanty = request.Garanty;
            requestModel.RequestCreator = request.CreateUser.ShortName;
            requestModel.RequestDate = request.CreateTime;
            requestModel.RequestState = request.State.Description;
            requestModel.SelectedMaster = request.MasterId.HasValue? _requestService.GetWorkerById(request.MasterId.Value):null;
            requestModel.SelectedExecuter = requestModel.ExecuterList.SingleOrDefault(w => w.Id == request.ExecuterId);
            requestModel.SelectedEquipment = requestModel.EquipmentList.SingleOrDefault(e => e.Id == request.Equipment?.Id);
            requestModel.RequestId = request.Id;
            requestModel.Rating = request.Rating;
            requestModel.AlertTime = request.AlertTime;
            if (request.ServiceCompanyId.HasValue)
            {
                requestModel.SelectedCompany = requestModel.CompanyList.FirstOrDefault(c => c.Id == request.ServiceCompanyId.Value);
            }
            if (request.ExecuteDate.HasValue && request.ExecuteDate.Value.Date > DateTime.MinValue)
            {
                requestModel.SelectedDateTime = request.ExecuteDate.Value.Date;
                requestModel.SelectedPeriod = requestModel.PeriodList.SingleOrDefault(i => i.Id == request.PeriodId);
            }
            requestModel.TermOfExecution = request.TermOfExecution;
            viewModel.ContactList = new ObservableCollection<ContactDto>(request.Contacts);
            view.Show();

        }

        private void RefreshRequest()
        {
            if(_requestService == null)
                _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);
            RequestList.Clear();
            var requests = _requestService.GetRequestList(RequestNum, FilterByCreateDate, FromDate, ToDate,
                ExecuteFromDate, ExecuteToDate,
                FilterStreetList.Where(w => w.Selected).Select(x => x.Id).ToArray(),
                _selectedHouse?.Id, SelectedFlat?.Id,
                FilterParentServiceList.Where(w => w.Selected).Select(x => x.Id).ToArray(),
                SelectedService?.Id,
                FilterStatusList.Where(w => w.Selected).Select(x => x.Id).ToArray(),
                FilterMasterList.Where(w => w.Selected).Select(x => x.Id).ToArray(),
                FilterExecuterList.Where(w => w.Selected).Select(x => x.Id).ToArray(),
                FilterServiceCompanyList.Where(w => w.Selected).Select(x => x.Id).ToArray(),
                FilterUserList.Where(w => w.Selected).Select(x => x.Id).ToArray(),
                FilterRatingList.Where(w => w.Selected).Select(x => x.Id).ToArray(),
                SelectedPayment?.Id, ServiceCompanyBadWork, OnlyRetry, ClientPhone, OnlyGaranty);
            foreach (var request in requests)
            {
                RequestList.Add(request);
            }
            RequestCount = RequestList.Count;
            OnPropertyChanged(nameof(RequestList));
        }

        public int RequestCount
        {
            get { return _requestCount; }
            set { _requestCount = value; OnPropertyChanged(nameof(RequestCount));}
        }

        public RequestControlContext()
        {
            RequestList = new ObservableCollection<RequestForListDto>();
            FilterByCreateDate = true;
            FromDate = DateTime.Today;
            ToDate = DateTime.Today;
            ExecuteFromDate = FromDate;
            ExecuteToDate = ToDate;
        }

        public void InitCollections()
        {
            _requestService = new RequestServiceImpl.RequestService(AppSettings.DbConnection);
            FilterStreetList = new ObservableCollection<FieldForFilterDto>();
            HouseList = new ObservableCollection<HouseDto>();
            FlatList = new ObservableCollection<FlatDto>();
            ServiceList = new ObservableCollection<ServiceDto>();
            FilterMasterList = new ObservableCollection<FieldForFilterDto>(_requestService.GetMasters(null).Select(
                w => new FieldForFilterDto()
                {
                    Id = w.Id,
                    Name = $"{w.SurName} {w.FirstName} {w.PatrName}",
                    Selected = false
                }).OrderBy(s=>s.Name));

            FilterExecuterList = new ObservableCollection<FieldForFilterDto>(_requestService.GetExecuters(null).Select(
                w => new FieldForFilterDto()
                {
                    Id = w.Id,
                    Name = $"{w.SurName} {w.FirstName} {w.PatrName}",
                    Selected = false
                }).OrderBy(s=>s.Name));

            FilterServiceCompanyList = new ObservableCollection<FieldForFilterDto>(_requestService.GetServiceCompanies().Select(
                w => new FieldForFilterDto()
                {
                    Id = w.Id,
                    Name = w.Name,
                    Selected = false
                }).OrderBy(s=>s.Name));

            FilterStatusList = new ObservableCollection<FieldForFilterDto>(_requestService.GetRequestStatuses().Select(
                w => new FieldForFilterDto()
                {
                    Id = w.Id,
                    Name = w.Description,
                    Selected = false
                }).OrderBy(s => s.Name));

            FilterUserList = new ObservableCollection<FieldForFilterDto>(_requestService.GetUsers().Select(
                w => new FieldForFilterDto()
                {
                    Id = w.Id,
                    Name = w.FullName,
                    Selected = false
                }).OrderBy(s => s.Name));
            FilterRatingList = new ObservableCollection<FieldForFilterDto>(new []{1,2,3,4,5}.Select(
                w => new FieldForFilterDto()
                {
                    Id = w,
                    Name = w.ToString(),
                    Selected = false
                }).OrderBy(s => s.Name));
            FilterParentServiceList = new ObservableCollection<FieldForFilterDto>(_requestService.GetServices(null).Select(
                w => new FieldForFilterDto()
                {
                    Id = w.Id,
                    Name = w.Name,
                    Selected = false
                }).OrderBy(s => s.Name));
            foreach (var service in FilterParentServiceList)
            {
                service.PropertyChanged += ServiceOnPropertyChanged;
            }
            //foreach (var status in FilterStatusList)
            //{
            //    status.PropertyChanged += OnPropertyChanged;
            //}

            PaymentList = new ObservableCollection<PaymentDto>(new [] {new PaymentDto{Id=0,Name="Бесплатные"}, new PaymentDto{Id = 1, Name = "Платные"}});
            ChangeCity(_requestService.GetCities().FirstOrDefault().Id);
        }

        private void ServiceOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var item = sender as FieldForFilterDto;
            if (item != null)
            {
                if (FilterParentServiceList.Count(f => f.Selected) == 1)
                {
                    ChangeParentService(FilterParentServiceList.FirstOrDefault(f => f.Selected)?.Id);
                }
                else
                {
                    ChangeParentService(null);
                }
            }
        }

        //private void OnPropertyChanged(object sender, PropertyChangedEventArgs arg)
        //{
        //    var t = arg;
        //}

        public ObservableCollection<RequestForListDto> RequestList
        {
            get { return _requestList; }
            set { _requestList = value; OnPropertyChanged(nameof(RequestList));}
        }

        public DateTime ExecuteFromDate
        {
            get { return _executeFromDate; }
            set { _executeFromDate = value; OnPropertyChanged(nameof(ExecuteFromDate));}
        }

        public DateTime ExecuteToDate
        {
            get { return _executeToDate; }
            set { _executeToDate = value; OnPropertyChanged(nameof(ExecuteToDate));}
        }

        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; OnPropertyChanged(nameof(FromDate));}
        }

        public DateTime ToDate
        {
            get { return _toDate; }
            set { _toDate = value; OnPropertyChanged(nameof(ToDate));}
        }
        public bool FilterByCreateDate
        {
            get { return _filterByCreateDate; }
            set { _filterByCreateDate = value; OnPropertyChanged(nameof(FilterByCreateDate)); }
        }

        private void AddRequest()
        {
            var viewModel = new RequestDialogViewModel();
            var view = new RequestDialog(viewModel);
            viewModel.SetView(view);
            view.Show();

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
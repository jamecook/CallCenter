using System;
using System.Configuration;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
using Stimulsoft.Report;


namespace RequestWcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RequestWebService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select RequestWebService.svc or RequestWebService.svc.cs at the Solution Explorer and start debugging.
    public class RequestWebService : IRequestWebService, IDisposable
    {
        private RequestService _requestService;
        private MySqlConnection _connection;
        public RequestWebService()
        {
            var connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", ConfigurationManager.AppSettings["ConnectionServer"], "asterisk", "mysqlasterisk", "asterisk");
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
            _requestService = new RequestService(_connection);
        }

        public DateTime GetCurrentDate()
        {
            return _requestService.GetCurrentDate();
        }
        public StatusDto[] GetStatusesAllowedInWeb()
        {
            return _requestService.GetStatusesAllowedInWeb();
        }

        public AttachmentDto[] GetAttachmentList(int requestId)
        {
            return _requestService.GetAttachmentsWeb(requestId);
        }

        public string GetRedirectPhone()
        {
            return _requestService.GetRedirectPhone();
        }

        public void SaveRedirectPhone(string secret, string phoneNumber)
        {
            if (secret == "savenewphone")
                _requestService.SaveRedirectPhone(phoneNumber);
        }
        public byte[] DownloadFile(int requestId, string fileName)
        {
            var rootDir = ConfigurationManager.AppSettings["rootFolder"].TrimEnd('\\');
            if (string.IsNullOrEmpty(rootDir))
                throw new ConfigurationErrorsException("rootFolder is not set!");
            if (Directory.Exists($"{rootDir}\\{requestId}"))
            {
                return File.ReadAllBytes($"{rootDir}\\{requestId}\\{fileName}");
            }
            return null;
        }

        public FileUploadResponse UploadFile(FileUploadRequest input)
        {
            var rootDir = ConfigurationManager.AppSettings["rootFolder"].TrimEnd('\\');
            if (string.IsNullOrEmpty(rootDir))
                throw new ConfigurationErrorsException("rootFolder is not set!");
            if (!Directory.Exists($"{rootDir}\\{input.RequestId}"))
            {
                Directory.CreateDirectory($"{rootDir}\\{input.RequestId}");
            }
            var fileExtension = Path.GetExtension(input.FileName);
            var fileName = Guid.NewGuid() + fileExtension;
            using (var writer = new FileStream($"{rootDir}\\{input.RequestId}\\{fileName}", FileMode.Create))
            {
                int readCount;
                var buffer = new byte[8192];
                while ((readCount = input.FileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    writer.Write(buffer, 0, readCount);
                }
            }
            _requestService.AttachFileToRequest(input.UserId,input.RequestId,input.FileName, fileName);

            return new FileUploadResponse() { RetFileName = fileName };
        }
        public void ChangeState(int requestId, int stateId, int userId)
        {
            _requestService.AddNewState(requestId, stateId, userId);
        }

        public byte[] GetRequestActs(int workerId, DateTime fromDate, DateTime toDate, int? FirlerWorkerId, int? FilterStreetId, int? FilterHouseId, int? FilterAddressId, int? FilterStatusId, int? FilterParrentServiceId, int? FilterServiceId)
        {
            var requests = _requestService.WebRequestList2(workerId, null, false, DateTime.Now, DateTime.Now, fromDate, toDate, FilterStreetId, FilterHouseId, FilterAddressId, FilterParrentServiceId, FilterServiceId, FilterStatusId, FirlerWorkerId);
            var stiReport = new StiReport();
            stiReport.Load("templates\\act.mrt");
            StiOptions.Engine.HideRenderingProgress = true;
            StiOptions.Engine.HideExceptions = true;
            StiOptions.Engine.HideMessages = true;

            var acts = requests.Select(r=>new {Address=r.FullAddress, Workers = r.Master.FullName, ClientPhones = r.ContactPhones, Service = r.ParentService + ": "+ r.Service, Description = r.Description}).ToArray();

            stiReport.RegBusinessObject("", "Acts", acts);
            stiReport.Render();
            var reportStream = new MemoryStream();
            stiReport.ExportDocument(StiExportFormat.Pdf, reportStream);
            reportStream.Position = 0;
            //File.WriteAllBytes("\\111.pdf",reportStream.GetBuffer());
            return reportStream.GetBuffer();
        }

        public byte[] ExportToExcel(int workerId, int? requestId, DateTime fromDate, DateTime toDate, int[] filterWorkerIds, int[] filterExecuterIds, int[] filterStreetIds, int[] filterHouseIds, int[] filterAddressIds, int[] filterStatusIds, int[] filterParrentServiceIds, int[] filterServiceIds, bool badWork, bool garanty, string clientPhone, int[] ratingIds, bool filterByCreateDate)
        {
            var requests = _requestService.WebRequestListArrayParam(workerId, requestId, filterByCreateDate, fromDate, toDate, fromDate, toDate, filterStreetIds, filterHouseIds, filterAddressIds, filterParrentServiceIds, filterServiceIds, filterStatusIds, filterWorkerIds, filterExecuterIds, ratingIds, badWork, garanty, clientPhone);
            var requestsDto = requests.Select(r => new { r.Id,r.CreateTime, r.StreetName, r.Building, r.Corpus, r.Flat, MasterShortName = r.Master?.ShortName, ExecuterShortName = r.Executer?.ShortName, r.ContactPhones, r.ParentService, r.Service, r.Description,r.Rating,r.Status,r.ExecuteTime,r.ExecutePeriod }).ToArray();

            var stiReport = new StiReport();
            stiReport.Load("templates\\exportToExcel.mrt");
            StiOptions.Engine.HideRenderingProgress = true;
            //StiOptions.Engine.HideExceptions = true;
            StiOptions.Engine.HideMessages = true;


            stiReport.RegBusinessObject("", "Requests", requestsDto);
            stiReport.Render();
            var reportStream = new MemoryStream();
            stiReport.ExportDocument(StiExportFormat.Excel2007, reportStream);
            reportStream.Position = 0;
            return reportStream.GetBuffer();
        }
        public CityDto[] GetData()
        {
            return _requestService.GetCities().ToArray();
        }

        public WebUserDto Login(string login, string password)
        {
            return _requestService.WebLogin(login, password);
        }

        public RequestForListDto[] RequestList(int workerId, DateTime fromDate, DateTime toDate,int? filterWorkerId, int? filterStreetId, int? filterHouseId, int? filterAddressId, int? filterStatusId,int? filterParrentServiceId, int? filterServiceId,bool badWork, bool garanty, string clientPhone, int? rating,bool filterByCreateDate)
        {
            return _requestService.WebRequestList2(workerId,null, filterByCreateDate, fromDate, toDate, fromDate, toDate, filterStreetId, filterHouseId, filterAddressId, filterParrentServiceId, filterServiceId, filterStatusId, filterWorkerId, badWork, garanty, clientPhone, rating);
        }
        public RequestForListDto[] RequestListArrayParams(int workerId, int? requestId, DateTime fromDate, DateTime toDate, int[] filterWorkerIds, int[] filterExecuterIds, int[] filterStreetIds, int[] filterHouseIds, int[] filterAddressIds, int[] filterStatusIds, int[] filterParrentServiceIds, int[] filterServiceIds, bool badWork, bool garanty, string clientPhone, int[] ratingIds, bool filterByCreateDate)
        {
            return _requestService.WebRequestListArrayParam(workerId, requestId, filterByCreateDate, fromDate, toDate, fromDate, toDate, filterStreetIds, filterHouseIds, filterAddressIds, filterParrentServiceIds, filterServiceIds, filterStatusIds, filterWorkerIds, filterExecuterIds, ratingIds, badWork, garanty, clientPhone);
        }

        public RequestForListDto GetRequestById(int requestId)
        {
            var request =_requestService.WebRequestList2(0, requestId, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, null, null, null, null, null, null, null).FirstOrDefault();
            return request;
        }
        public RequestForListDto GetRequestByWorkerAndId(int workerId, int requestId)
        {
            var request =_requestService.WebRequestList2(workerId, requestId, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, null, null, null, null, null, null, null).FirstOrDefault();
            return request;
        }

        public WorkerDto[] GetWorkers(int workerId)
        {
            return _requestService.GetWorkersByWorkerId(workerId);
        }
        public WorkerDto[] GetWorkersByPeriod(bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int workerId)
        {
            return _requestService.GetWorkersByPeriod(filterByCreateDate, fromDate, toDate, executeFromDate, executeToDate, workerId);
        }
        public WorkerDto[] GetExecutersByPeriod(bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int workerId)
        {
            return _requestService.GetExecutersByPeriod(filterByCreateDate, fromDate, toDate, executeFromDate, executeToDate, workerId);
        }
        public StreetDto[] GetStreetListByWorker(int workerId)
        {
            return _requestService.GetStreetsByWorkerId(workerId);
        }

        public WebStatusDto[] GetWebStatuses()
        {
            return _requestService.GetWebStatuses();
        }

        public WebHouseDto[] GetHousesByStreetAndWorkerId(int streetId, int workerId)
        {
            return _requestService.GetHousesByStreetAndWorkerId(streetId, workerId);
        }
        public ServiceDto[] GetServices(int? parentId)
        {
            return _requestService.GetServices(parentId).ToArray();
        }
        public WebFlatDto[] GetFlatByHouseId(int houseId)
        {
            var flats = _requestService.GetFlats(houseId).OrderBy(s => s.TypeId).ThenBy(s => s.Flat?.PadLeft(6, '0'));
            return flats.Select(f => new WebFlatDto { Id = f.Id, Name = f.Name }).ToArray();
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
        }

        public byte[] GetMediaByRequestId(int requestId)
        {
            return _requestService.GetMediaByRequestId(requestId);
        }

        public byte[] GetRecordById(int recordId)
        {
            return _requestService.GetRecordById(recordId);
        }

        public StatInfoDto[] GetRequestByUsersInfo()
        {
            return _requestService.GetRequestByUsersInfo();
        }
        public StatInfoDto[] GetRequestByWorkersInfo()
        {
            return _requestService.GetRequestByWorkersInfo();
        }

        public WebCallsDto[] GetWebCallsByRequestId(int requestId)
        {
            return _requestService.GetWebCallsByRequestId(requestId);
        }

        public void AddNote(int requestId, string note, int userId)
        {
            _requestService.AddNewNote(requestId,note,userId);
        }

        public NoteDto[] GetNotes(int requestId)
        {
            return _requestService.GetNotesWeb(requestId).ToArray();
        }
        public AppAddressDto[] GetAddressesByPhone(string phone,string code)
        {
            return _requestService.GetAddressesByPhone(phone, code).ToArray();
        }
        public AppRequestDto[] GetRequestsByPhone(string phone,string code)
        {
            return _requestService.GetRequestsByPhone(phone, code).ToArray();
        }
        public AppTypeDto[] GetTypesByPhone(string phone, string code)
        {
            return _requestService.GetTypesByPhone(phone, code).ToArray();
        }
        public void CreateRequestFromPhone(string phone,string code, int addressId, int typeId, string description)
        {
            _requestService.CreateRequestFromPhone(phone, code, addressId, typeId, description);
        }
        public string CreateRequest(int workerId, string phone, string fio, int addressId, int typeId, int? masterId, int? executerId, string description)
        {
            return _requestService.CreateRequestFromWeb(workerId, phone, fio, addressId, typeId, masterId, executerId,
                description);
        }

        public StatInfoDto[] GetWorkerStat(int currentWorkerId, DateTime fromDate, DateTime toDate)
        {
            return _requestService.GetWorkerStat(currentWorkerId, fromDate, toDate);
        }
    }
}

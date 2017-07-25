﻿using System;
using System.Configuration;
using System.Linq;
using MySql.Data.MySqlClient;
using RequestServiceImpl;
using RequestServiceImpl.Dto;


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
        public CityDto[] GetData()
        {
            return _requestService.GetCities().ToArray();
        }

        public WebUserDto Login(string login, string password)
        {
            return _requestService.WebLogin(login, password);
        }

        public RequestForListDto[] RequestList(int workerId, DateTime fromDate, DateTime toDate,int? FirlerWorkerId, int? FilterStreetId, int? FilterHouseId, int? FilterAddressId, int? FilterStatusId,int? FilterParrentServiceId, int? FilterServiceId )
        {
            return _requestService.WebRequestList(workerId,null,false,DateTime.Now,DateTime.Now, fromDate, toDate, FilterStreetId, FilterHouseId, FilterAddressId, FilterParrentServiceId, FilterServiceId, FilterStatusId, FirlerWorkerId);
        }

        public RequestInfoDto GetRequestById(int requestId)
        {
            return _requestService.GetRequest(requestId);
        }

        public WorkerDto[] GetWorkers(int workerId)
        {
            return _requestService.GetWorkersByWorkerId(workerId);
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
            return _requestService.GetMeiaByRequestId(requestId);
        }
    }
}
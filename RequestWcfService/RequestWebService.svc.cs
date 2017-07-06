using System;
using System.Configuration;
using System.Linq;
using MySql.Data.MySqlClient;
using RequestServiceImpl;
using RequestServiceImpl.Dto;


namespace RequestWcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RequestWebService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select RequestWebService.svc or RequestWebService.svc.cs at the Solution Explorer and start debugging.
    public class RequestWebService : IRequestWebService
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

        public RequestForListDto[] RequestList(int workerId, DateTime fromDate, DateTime toDate)
        {
            return _requestService.WebRequestList(workerId,null,false,DateTime.Now,DateTime.Now, fromDate, toDate, null,null,null,null,null,null,null);
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

        public WebHouseDto[] GetHousesByStrteetAndWorkerId(int streetId, int workerId)
        {
            return _requestService.GetHousesByStrteetAndWorkerId(streetId, workerId);
        }
    }
}

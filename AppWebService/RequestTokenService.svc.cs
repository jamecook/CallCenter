using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MySql.Data.MySqlClient;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace AppWebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RequestService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select RequestService.svc or RequestService.svc.cs at the Solution Explorer and start debugging.
    public class RequestTokenService : IRequestTokenService
    {
        private RequestService _requestService;
        private MySqlConnection _connection;
        public RequestTokenService()
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
        public LoginDto Login(string login, string password)
        {
            var userInfo = _requestService.WebLogin(login, password);
            if (userInfo == null)
                return null;
            return new LoginDto(){Token = Guid.NewGuid(),UserInfo = userInfo};
        }

        public ServiceDto[] GetServices(Guid token, int? parentId)
        {
            return _requestService.GetServices(parentId).ToArray();
        }
    }
}

using System.Diagnostics;
using CRMPhone.Dto;
using MySql.Data.MySqlClient;

namespace CRMPhone.ViewModel
{
    public static class AppSettings
    {
        private static MySqlConnection _dbConnection;
        private static UserDto _currentUser;
        private static SipDto _sipInfo;

        public static MySqlConnection DbConnection
        {
            get { return _dbConnection; }
        }

        public static void SetDbConnection(MySqlConnection connection)
        {
            _dbConnection = connection;
        }

        public static SipDto SipInfo
        {
            get { return _sipInfo; }
        }

        public static void SetSipInfo(SipDto info)
        {
            _sipInfo = info;
        }
        public static UserDto CurrentUser
        {
            get { return _currentUser; }
        }

        public static string LastIncomingCall { get; set; }

        public static void SetUser(UserDto user)
        {
            _currentUser = user;
        }

    }
}
using System.Configuration;
using MySql.Data.MySqlClient;

namespace ChangeStateToInWork
{
    class Program
    {
        static void Main(string[] args)
        {
            var serverIP = ConfigurationManager.AppSettings["CallCenterIP"];
            var connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", serverIP,
                "asterisk", "mysqlasterisk", "asterisk");
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            using (var cmd = new MySqlCommand("call CallCenter.RequestChangeStateToInWork()", conn))
            {
                cmd.ExecuteNonQuery();
            }

        }
    }
}

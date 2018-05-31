using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace StopAllAutoRing
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
                using (var cmd = new MySqlCommand("update asterisk.RingUpList set state = 2 where state in (0,1)", conn))
                {
                    cmd.ExecuteNonQuery();
                }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using RequestWebService.Dto;

namespace RequestWebService.Services
{
    public static class RequestService
    {
        private static string _connectionString;
        static RequestService()
        {
            _connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", "192.168.1.130", "dispex", "mysqldispex", "Dispex");
        }

        public static void LogOperation(string ipAddress, string function, string jsonOperation)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("insert into Logs(IpAddress,Json,Function) values(@IpStr,@Json,@Function);", conn))
                {
                    cmd.Parameters.AddWithValue("@IpStr", ipAddress);
                    cmd.Parameters.AddWithValue("@Json", jsonOperation);
                    cmd.Parameters.AddWithValue("@Function", function);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public static void AddUser(string bitrixId,string surName,string firstName,string patrName, string phone, string email, string login, string password, string defaultServiceCompany, bool isMaster)
        {
            
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("call Dispex.add_user(@bitrixIdStr,@surNameStr,@firstNameStr,@patrNameStr,@phoneStr,@emailStr,@loginStr,@passwordStr,@defServiceCompanyStr,@isMaster);", conn))
                {
                    cmd.Parameters.AddWithValue("@bitrixIdStr", bitrixId);
                    cmd.Parameters.AddWithValue("@surNameStr", surName);
                    cmd.Parameters.AddWithValue("@firstNameStr", firstName);
                    cmd.Parameters.AddWithValue("@patrNameStr", patrName);
                    cmd.Parameters.AddWithValue("@phoneStr", phone);
                    cmd.Parameters.AddWithValue("@emailStr", email);
                    cmd.Parameters.AddWithValue("@loginStr", login);
                    cmd.Parameters.AddWithValue("@passwordStr", password);
                    cmd.Parameters.AddWithValue("@defServiceCompanyStr", defaultServiceCompany);
                    cmd.Parameters.AddWithValue("@isMaster", isMaster);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public static void UpdateUser(string bitrixId, string surName, string firstName, string patrName, string phone, string email, string login, string password, string defaultServiceCompany, bool isMaster)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(
                            "call Dispex.update_user(@bitrixIdStr,@surNameStr,@firstNameStr,@patrNameStr,@phoneStr,@emailStr,@loginStr,@passwordStr,@defServiceCompanyStr,@isMaster);",
                            conn))
                {
                    cmd.Parameters.AddWithValue("@bitrixIdStr", bitrixId);
                    cmd.Parameters.AddWithValue("@surNameStr", surName);
                    cmd.Parameters.AddWithValue("@firstNameStr", firstName);
                    cmd.Parameters.AddWithValue("@patrNameStr", patrName);
                    cmd.Parameters.AddWithValue("@phoneStr", phone);
                    cmd.Parameters.AddWithValue("@emailStr", email);
                    cmd.Parameters.AddWithValue("@loginStr", login);
                    cmd.Parameters.AddWithValue("@passwordStr", password);
                    cmd.Parameters.AddWithValue("@defServiceCompanyStr", defaultServiceCompany);
                    cmd.Parameters.AddWithValue("@isMaster", isMaster);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }
        public static UserDto GetUser(string bitrixId)
        {
            UserDto result;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("call Dispex.get_user(@bitrixIdStr);", conn))
                {
                    cmd.Parameters.AddWithValue("@bitrixIdStr", bitrixId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        result = new UserDto()
                        {
                            BitrixId = dataReader.GetNullableString("corps"),
                            SurName = dataReader.GetNullableString("sur_name"),
                            FirstName = dataReader.GetNullableString("first_name"),
                            PatrName = dataReader.GetNullableString("patr_name"),
                            Phone = dataReader.GetNullableString("phone"),
                            IsMaster = dataReader.GetBoolean("is_master"),
                            Email = dataReader.GetNullableString("email"),
                            Login = dataReader.GetNullableString("login"),
                            DefaultServiceCompany = dataReader.GetNullableString("default_service_company"),
                        };

                    }
                }
                conn.Close();
            }
            return result;
        }
    }
}

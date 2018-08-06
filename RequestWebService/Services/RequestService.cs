using System;
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
                using (var cmd = new MySqlCommand("insert into logs(ip_address,json,function) values(@IpStr,@Json,@Function);", conn))
                {
                    cmd.Parameters.AddWithValue("@IpStr", ipAddress);
                    cmd.Parameters.AddWithValue("@Json", jsonOperation);
                    cmd.Parameters.AddWithValue("@Function", function);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public static string AddUser(string bitrixId,string surName,string firstName,string patrName, string phone, string email, string login, string password, string defaultServiceCompany, bool isMaster)
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
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        return dataReader.GetNullableString("userId");
                    }
                }
            }
        }

        internal static void UpdateRequest(string bitrixId, string phone, DateTime createDateTime, string streetName, string building, string corpus,
            string flat, string serviceId, string serviceName, string descript, string status, string executer, DateTime? executeDateTime, double? cost)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("call Dispex.update_request(@bitrixIdStr,@phoneStr,@createDateTime,@streetNameStr,@buildingStr,@corpusStr,@flatStr,@serviceIdStr,@serviceNameStr," +
                                                  "@descriptStr,@statusStr,@executerStr,@executeDateTime,@costDouble);", conn))
                {
                    cmd.Parameters.AddWithValue("@bitrixIdStr", bitrixId);
                    cmd.Parameters.AddWithValue("@phoneStr", phone);
                    cmd.Parameters.AddWithValue("@createDateTime", createDateTime);
                    cmd.Parameters.AddWithValue("@streetNameStr", streetName);
                    cmd.Parameters.AddWithValue("@buildingStr", building);
                    cmd.Parameters.AddWithValue("@corpusStr", corpus);
                    cmd.Parameters.AddWithValue("@flatStr", flat);
                    cmd.Parameters.AddWithValue("@serviceIdStr", serviceId);
                    cmd.Parameters.AddWithValue("@serviceNameStr", serviceName);
                    cmd.Parameters.AddWithValue("@descriptStr", descript);
                    cmd.Parameters.AddWithValue("@statusStr", status);
                    cmd.Parameters.AddWithValue("@executerStr", executer);
                    cmd.Parameters.AddWithValue("@executeDateTime", executeDateTime);
                    cmd.Parameters.AddWithValue("@costDouble", cost);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public static string AddRequest(string bitrixId,string phone, DateTime createDateTime, string streetName, string building, string corpus,
            string flat, string serviceId, string serviceName, string descript, string status, string executer, DateTime? executeDateTime, double? cost)
        {
            
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("call Dispex.add_request(@bitrixIdStr,@phoneStr,@createDateTime,@streetNameStr,@buildingStr,@corpusStr,@flatStr,@serviceIdStr,@serviceNameStr," +
                                                  "@descriptStr,@statusStr,@executerStr,@executeDateTime,@costDouble);", conn))
                {
                    cmd.Parameters.AddWithValue("@bitrixIdStr", bitrixId);
                    cmd.Parameters.AddWithValue("@phoneStr", phone);
                    cmd.Parameters.AddWithValue("@createDateTime", createDateTime);
                    cmd.Parameters.AddWithValue("@streetNameStr", streetName);
                    cmd.Parameters.AddWithValue("@buildingStr", building);
                    cmd.Parameters.AddWithValue("@corpusStr", corpus);
                    cmd.Parameters.AddWithValue("@flatStr", flat);
                    cmd.Parameters.AddWithValue("@serviceIdStr", serviceId);
                    cmd.Parameters.AddWithValue("@serviceNameStr", serviceName);
                    cmd.Parameters.AddWithValue("@descriptStr", descript);
                    cmd.Parameters.AddWithValue("@statusStr", status);
                    cmd.Parameters.AddWithValue("@executerStr", executer);
                    cmd.Parameters.AddWithValue("@executeDateTime", executeDateTime);
                    cmd.Parameters.AddWithValue("@costDouble", cost);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        return dataReader.GetNullableString("requestId");
                    }
                }
            }
        }
        public static RequestDto GetRequest(string bitrixId)
        {
            RequestDto result;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("call Dispex.get_request(@bitrixIdStr);", conn))
                {
                    cmd.Parameters.AddWithValue("@bitrixIdStr", bitrixId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if(dataReader.Read())

                        { result = new RequestDto()
                        {
                            BitrixId = dataReader.GetNullableString("bitrix_id"),
                            Id = dataReader.GetInt32("id"),
                            CreaterPhone = dataReader.GetNullableString("from_phone"),
                            CreateTime = dataReader.GetDateTime("create_date"),
                            StreetName = dataReader.GetNullableString("street_name"),
                            Building = dataReader.GetNullableString("building"),
                            Corpus = dataReader.GetNullableString("corpus"),
                            Flat = dataReader.GetNullableString("flat"),
                            ServiceId = dataReader.GetNullableString("bitrix_service_id"),
                            ServiceFullName = dataReader.GetNullableString("bitrix_service_name"),
                            Description = dataReader.GetNullableString("descript"),
                            Status = dataReader.GetNullableString("status"),
                            ExecuterName = dataReader.GetNullableString("executer_name"),
                            ExecuteTime = dataReader.GetNullableDateTime("execute_date"),
                            Cost = dataReader.GetNullableDouble("cost"),
                        };
                        }
                        else
                        {
                            result = null;
                        }
                    }
                }
                conn.Close();
            }
            return result;
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
                        if(dataReader.Read())
                        { result = new UserDto()
                        {
                            //BitrixId = dataReader.GetNullableString("bitrix_id"),
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
                        else
                        {
                            result = null;
                        }

                    }
                }
                conn.Close();
            }
            return result;
        }
    }
}

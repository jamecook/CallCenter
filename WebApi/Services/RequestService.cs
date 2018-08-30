using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using WebApi.Models;

namespace WebApi.Services
{
    public static class RequestService
    {
        private static string _connectionString;

        static RequestService()
        {
            _connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", "192.168.1.130",
                "asterisk", "mysqlasterisk", "CallCenter");
        }

        public static WebUserDto WebLogin(string userName, string password)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand($"Call CallCenter.WebLogin('{userName}','{password}')", conn)
                )
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return new WebUserDto
                            {
                                UserId = dataReader.GetInt32("UserId"),
                                Login = dataReader.GetNullableString("Login"),
                                SurName = dataReader.GetNullableString("SurName"),
                                FirstName = dataReader.GetNullableString("FirstName"),
                                PatrName = dataReader.GetNullableString("PatrName"),
                                WorkerId = dataReader.GetInt32("worker_id"),
                                ServiceCompanyId = dataReader.GetInt32("service_company_id"),
                                SpecialityId = dataReader.GetInt32("speciality_id"),
                                CanCreateRequestInWeb = dataReader.GetBoolean("can_create_in_web")
                            };
                        }
                        dataReader.Close();
                    }
                }
                return null;
            }
        }

        public static RequestForListDto[] WebRequestListArrayParam(int currentWorkerId, int? requestId, bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int[] streetIds, int[] houseIds, int[] addressIds, int[] parentServiceIds, int[] serviceIds, int[] statusIds, int[] workerIds, int[] executerIds, int[] ratingIds, bool badWork = false, bool garanty = false, string clientPhone = null)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.WebGetRequestsArrayParam(@CurWorker,@RequestId,@ByCreateDate,@FromDate,@ToDate,@ExecuteFromDate,@ExecuteToDate,@StreetIds,@HouseIds,@AddressIds,@ParentServiceIds,@ServiceIds,@StatusIds,@WorkerIds,@ExecuterIds,@BadWork,@Garanty,@ClientPhone,@RatingIds)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@ByCreateDate", filterByCreateDate);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@ExecuteFromDate", executeFromDate);
                    cmd.Parameters.AddWithValue("@ExecuteToDate", executeToDate);
                    cmd.Parameters.AddWithValue("@StreetIds",
                        streetIds != null && streetIds.Length > 0
                            ? streetIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@HouseIds",
                        houseIds != null && houseIds.Length > 0
                            ? houseIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@AddressIds",
                        addressIds != null && addressIds.Length > 0
                            ? addressIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@ParentServiceIds",
                        parentServiceIds != null && parentServiceIds.Length > 0
                            ? parentServiceIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@ServiceIds",
                        serviceIds != null && serviceIds.Length > 0
                            ? serviceIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@StatusIds",
                        statusIds != null && statusIds.Length > 0
                            ? statusIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@WorkerIds",
                        workerIds != null && workerIds.Length > 0
                            ? workerIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@ExecuterIds",
                        executerIds != null && executerIds.Length > 0
                            ? executerIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@BadWork", badWork);
                    cmd.Parameters.AddWithValue("@Garanty", garanty);
                    cmd.Parameters.AddWithValue("@ClientPhone", clientPhone);
                    cmd.Parameters.AddWithValue("@RatingIds",
                        ratingIds != null && ratingIds.Length > 0
                            ? ratingIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);

                    var requests = new List<RequestForListDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            requests.Add(new RequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                StreetPrefix = dataReader.GetString("prefix_name"),
                                StreetName = dataReader.GetString("street_name"),
                                AddressType = dataReader.GetString("address_type"),
                                Flat = dataReader.GetString("flat"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                Entrance = dataReader.GetNullableString("entrance"),
                                Floor = dataReader.GetNullableString("floor"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                Master = dataReader.GetNullableInt("worker_id") != null
                                    ? new RequestUserDto
                                    {
                                        Id = dataReader.GetInt32("worker_id"),
                                        SurName = dataReader.GetNullableString("sur_name"),
                                        FirstName = dataReader.GetNullableString("first_name"),
                                        PatrName = dataReader.GetNullableString("patr_name"),
                                    }
                                    : null,
                                Executer = dataReader.GetNullableInt("executer_id") != null
                                    ? new RequestUserDto
                                    {
                                        Id = dataReader.GetInt32("executer_id"),
                                        SurName = dataReader.GetNullableString("exec_sur_name"),
                                        FirstName = dataReader.GetNullableString("exec_first_name"),
                                        PatrName = dataReader.GetNullableString("exec_patr_name"),
                                    }
                                    : null,
                                CreateUser = new RequestUserDto
                                {
                                    Id = dataReader.GetInt32("create_user_id"),
                                    SurName = dataReader.GetNullableString("surname"),
                                    FirstName = dataReader.GetNullableString("firstname"),
                                    PatrName = dataReader.GetNullableString("patrname"),
                                },
                                ExecuteTime = dataReader.GetNullableDateTime("execute_date"),
                                ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                                Rating = dataReader.GetNullableString("Rating"),
                                BadWork = dataReader.GetBoolean("bad_work"),
                                IsRetry = dataReader.GetBoolean("retry"),
                                Garanty = dataReader.GetBoolean("garanty"),
                                StatusId = dataReader.GetInt32("req_status_id"),
                                Status = dataReader.GetNullableString("Req_Status"),
                                TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                            });
                        }
                        dataReader.Close();
                    }
                    return requests.ToArray();
                }
            }
        }
    }
}

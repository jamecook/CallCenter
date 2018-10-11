using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using Stimulsoft.Report;
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
                using (var cmd = new MySqlCommand($"Call CallCenter.DispexLogin('{userName}','{password}')", conn)
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
                                CanCreateRequestInWeb = dataReader.GetBoolean("can_create_in_web"),
                                AllowStatistics = dataReader.GetBoolean("allow_statistics")
                            };
                        }
                        dataReader.Close();
                    }
                }
                return null;
            }
        }

        public static byte[] GetRequestActs(int workerId, int[] requestIds)
        {
            var requests = WebRequestsByIds(workerId, requestIds);
            var stiReport = new StiReport();
            stiReport.Load("templates\\act.mrt");
            StiOptions.Engine.HideRenderingProgress = true;
            StiOptions.Engine.HideExceptions = true;
            StiOptions.Engine.HideMessages = true;

            var acts = requests.Select(r => new { Address = r.FullAddress, Workers = r.Master?.FullName, ClientPhones = r.ContactPhones, Service = r.ParentService + ": " + r.Service, Description = r.Description }).ToArray();

            stiReport.RegBusinessObject("", "Acts", acts);
            stiReport.Render();
            var reportStream = new MemoryStream();
            stiReport.ExportDocument(StiExportFormat.Pdf, reportStream);
            reportStream.Position = 0;
            //File.WriteAllBytes("\\111.pdf",reportStream.GetBuffer());
            return reportStream.GetBuffer();

        }

        public static WorkerDto[] GetWorkersByWorkerId(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.DispexGetWorkers(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var workers = new List<WorkerDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            workers.Add(new WorkerDto
                            {
                                Id = dataReader.GetInt32("id"),
                                SurName = dataReader.GetString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                SpecialityId = dataReader.GetNullableInt("speciality_id"),
                            });
                        }
                        dataReader.Close();
                    }
                    return workers.ToArray();
                }
            }
        }
        public static int[] GetAddressesId(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.DispexGetAddresses(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<int>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                                list.Add(dataReader.GetInt32("id"));
                        }
                        dataReader.Close();
                    }
                    return list.ToArray();
                }
            }
        }
        public static int[] GetHousesId(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.DispexGetAllHouses(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<int>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                                list.Add(dataReader.GetInt32("id"));
                        }
                        dataReader.Close();
                    }
                    return list.ToArray();
                }
            }
        }
        public static StatusDto[] GetStatusesAll(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = "CALL CallCenter.DispexGetStatuses(@CurWorker)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", workerId);
                    var types = new List<StatusDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            types.Add(new StatusDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Description = dataReader.GetString("Description")
                            });
                        }
                        dataReader.Close();
                    }
                    return types.ToArray();
                }
            }
        }
        public static StatusDto[] GetStatusesAllowedInWeb(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = "CALL CallCenter.DispexGetStatusesForSet(@CurWorker)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", workerId);
                    var types = new List<StatusDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            types.Add(new StatusDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Description = dataReader.GetString("Description")
                            });
                        }
                        dataReader.Close();
                    }
                    return types.ToArray();
                }
            }
        }

        public static WebCallsDto[] GetWebCallsByRequestId(int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = @"SELECT rc.id,c.CallerIdNum,c.CreateTime,c.MonitorFile FROM CallCenter.Requests r
join CallCenter.RequestCalls rc on rc.request_id = r.id
join asterisk.ChannelHistory c on c.UniqueID = rc.uniqueID where r.id = @reqId order by c.CreateTime";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    var states = new List<WebCallsDto>();
                    cmd.Parameters.AddWithValue("@reqId", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            states.Add(new WebCallsDto
                            {
                                Id = dataReader.GetInt32("id"),
                                PhoneNumber = dataReader.GetNullableString("CallerIdNum"),
                                CreateTime = dataReader.GetDateTime("CreateTime"),
                            });
                        }
                        dataReader.Close();
                    }
                    return states.ToArray();
                }
            }
        }
        public static byte[] GetRecordById(int recordId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand(@"SELECT MonitorFile FROM CallCenter.RequestCalls r
    join asterisk.ChannelHistory c on c.UniqueID = r.uniqueID
    where r.id = @reqId", conn))
                {
                    cmd.Parameters.AddWithValue("@reqId", recordId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            var fileName = dataReader.GetNullableString("MonitorFile");
                            var serverIpAddress = conn.DataSource;
                            var localFileName =
                                fileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\")
                                    .Replace("/", "\\");
                            if (File.Exists(localFileName))
                            {
                                return File.ReadAllBytes(localFileName);
                            }
                            var localFileNameMp3 = localFileName.Replace(".wav", ".mp3");
                            if (File.Exists(localFileNameMp3))
                            {
                                return File.ReadAllBytes(localFileNameMp3);
                            }
                            return null;
                        }
                    }
                }
            }
            return new byte[0];
        }
        public static byte[] DownloadFile(int requestId, string fileName,string rootDir)
        {
            if (!string.IsNullOrEmpty(rootDir) && Directory.Exists($"{rootDir}\\{requestId}"))
            {
                return File.ReadAllBytes($"{rootDir}\\{requestId}\\{fileName}");
            }
            return null;
        }

        public static StreetDto[] GetStreetsByWorkerId(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = "CALL CallCenter.DispexGetStreets(@CurWorker)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", workerId);
                    var streets = new List<StreetDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            streets.Add(new StreetDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Prefix = new StreetPrefixDto
                                {
                                    Id = dataReader.GetInt32("Prefix_id"),
                                    Name = dataReader.GetString("Prefix_Name"),
                                    ShortName = dataReader.GetString("ShortName")
                                },
                                CityId = dataReader.GetInt32("city_id")
                            });
                        }
                        dataReader.Close();
                    }
                    return streets.ToArray();
                }
            }
        }
        public static WebHouseDto[] GetHousesByStreetAndWorkerId(int streetId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = @"CALL CallCenter.DispexGetHouses(@StreetId,@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@StreetId", streetId);
                    var houses = new List<HouseDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            houses.Add(new HouseDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                StreetId = streetId
                            });
                        }
                        dataReader.Close();
                    }
                    return houses.Select(h => new WebHouseDto {Id = h.Id, Name = h.FullName}).ToArray();
                }
            }
        }
        public static IList<ServiceDto> GetParentServices()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = @"SELECT id,name,can_send_sms, null parent_id, null parent_name FROM CallCenter.RequestTypes R where parrent_id is null and enabled = 1 order by name";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var services = new List<ServiceDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            services.Add(new ServiceDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                CanSendSms = dataReader.GetBoolean("can_send_sms"),
                                ParentId = dataReader.GetNullableInt("parent_id"),
                                ParentName = dataReader.GetNullableString("parent_name")
                            });
                        }
                        dataReader.Close();
                    }
                    return services;
                }
            }
        }
        public static List<ServiceCompanyDto> GetServicesCompanies()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var query = "SELECT id,name FROM CallCenter.ServiceCompanies S where Enabled = 1 order by S.Name";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var companies = new List<ServiceCompanyDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            companies.Add(new ServiceCompanyDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name")
                            });
                        }
                        dataReader.Close();
                    }
                    return companies.OrderBy(i => i.Name).ToList();
                }
            }
        }

        public static IList<ServiceDto> GetServices(int[] parentIds)
        {
            if (parentIds == null || parentIds.Length == 0)
                return new List<ServiceDto>(0);
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var ids = parentIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j);
                var query =
                    $@"SELECT t1.id,t1.name,t1.can_send_sms,t2.id parent_id, t2.name parent_name FROM CallCenter.RequestTypes t1
                        left join CallCenter.RequestTypes t2 on t2.id = t1.parrent_id
                        where t1.parrent_id in ({ids}) and t1.enabled = 1 order by t2.name,t1.name";
                    
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var services = new List<ServiceDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            services.Add(new ServiceDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                CanSendSms = dataReader.GetBoolean("can_send_sms"),
                                ParentId = dataReader.GetNullableInt("parent_id"),
                                ParentName = dataReader.GetNullableString("parent_name")
                            });
                        }
                        dataReader.Close();
                    }
                    return services;
                }
            }
        }
        public static RequestForListDto[] WebRequestListArrayParam(int currentWorkerId, int? requestId, bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int[] streetIds, int[] houseIds, int[] addressIds, int[] parentServiceIds, int[] serviceIds, int[] statusIds, int[] workerIds, int[] executerIds, int[] ratingIds,int[] companies, bool badWork = false, bool garanty = false, string clientPhone = null)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.DispexGetRequests2(@CurWorker,@RequestId,@ByCreateDate,@FromDate,@ToDate,@ExecuteFromDate,@ExecuteToDate,@StreetIds,@HouseIds,@AddressIds,@ParentServiceIds,@ServiceIds,@StatusIds,@WorkerIds,@ExecuterIds,@BadWork,@Garanty,@ClientPhone,@RatingIds,@CompaniesIds)";
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
                    cmd.Parameters.AddWithValue("@CompaniesIds",
                        companies != null && companies.Length > 0
                            ? companies.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    
                    var requests = new List<RequestForListDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var recordId = dataReader.GetNullableInt("recordId");
                            requests.Add(new RequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                StreetPrefix = dataReader.GetString("prefix_name"),
                                StreetName = dataReader.GetString("street_name"),
                                AddressType = dataReader.GetString("address_type"),
                                CompanyId = dataReader.GetNullableInt("service_company_id"),
                                CompanyName = dataReader.GetNullableString("company_name"),
                                Flat = dataReader.GetString("flat"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                Entrance = dataReader.GetNullableString("entrance"),
                                FirstRecordId = recordId,
                                HasRecord = recordId.HasValue,
                                HasAttachment = dataReader.GetBoolean("has_attach"),
                                IsBadWork = dataReader.GetBoolean("bad_work"),
                                IsImmediate = dataReader.GetBoolean("is_immediate"),
                                Floor = dataReader.GetNullableString("floor"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                Master = dataReader.GetNullableInt("worker_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("worker_id"),
                                        SurName = dataReader.GetNullableString("sur_name"),
                                        FirstName = dataReader.GetNullableString("first_name"),
                                        PatrName = dataReader.GetNullableString("patr_name"),
                                    }
                                    : null,
                                Executer = dataReader.GetNullableInt("executer_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("executer_id"),
                                        SurName = dataReader.GetNullableString("exec_sur_name"),
                                        FirstName = dataReader.GetNullableString("exec_first_name"),
                                        PatrName = dataReader.GetNullableString("exec_patr_name"),
                                    }
                                    : null,
                                CreateUser = new UserDto
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
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                LastNote = dataReader.GetNullableString("last_note")
                            });
                        }
                        dataReader.Close();
                    }
                    return requests.ToArray();
                }
            }
        }
        public static RequestForListDto[] WebRequestsByIds(int currentWorkerId, int[] requestIds)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.DispexGetRequestsByIds(@CurWorker,@RequestIds)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@RequestIds",
                        requestIds != null && requestIds.Length > 0
                            ? requestIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);

                    var requests = new List<RequestForListDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var recordId = dataReader.GetNullableInt("recordId");
                            requests.Add(new RequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                StreetPrefix = dataReader.GetString("prefix_name"),
                                StreetName = dataReader.GetString("street_name"),
                                AddressType = dataReader.GetString("address_type"),
                                CompanyId = dataReader.GetNullableInt("service_company_id"),
                                CompanyName = dataReader.GetNullableString("company_name"),
                                Flat = dataReader.GetString("flat"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                Entrance = dataReader.GetNullableString("entrance"),
                                FirstRecordId = recordId,
                                HasRecord = recordId.HasValue,
                                HasAttachment = dataReader.GetBoolean("has_attach"),
                                IsBadWork = dataReader.GetBoolean("bad_work"),
                                IsImmediate = dataReader.GetBoolean("is_immediate"),
                                Floor = dataReader.GetNullableString("floor"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                Master = dataReader.GetNullableInt("worker_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("worker_id"),
                                        SurName = dataReader.GetNullableString("sur_name"),
                                        FirstName = dataReader.GetNullableString("first_name"),
                                        PatrName = dataReader.GetNullableString("patr_name"),
                                    }
                                    : null,
                                Executer = dataReader.GetNullableInt("executer_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("executer_id"),
                                        SurName = dataReader.GetNullableString("exec_sur_name"),
                                        FirstName = dataReader.GetNullableString("exec_first_name"),
                                        PatrName = dataReader.GetNullableString("exec_patr_name"),
                                    }
                                    : null,
                                CreateUser = new UserDto
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
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                LastNote = dataReader.GetNullableString("last_note")
                            });
                        }
                        dataReader.Close();
                    }
                    return requests.ToArray();
                }
            }
        }

        public static void AddNewNote(int requestId, string note, int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestNoteHistory (request_id,operation_date,user_id,note)
 values(@RequestId,sysdate(),@UserId,@Note);", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Note", note);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }


        }

        public static FlatDto[] GetFlats(int houseId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"SELECT A.id,A.type_id,A.flat,T.Name FROM CallCenter.Addresses A
    join CallCenter.AddressesTypes T on T.id = A.type_id
    where A.enabled = true and A.house_id = @HouseId", conn))
                {
                    cmd.Parameters.AddWithValue("@HouseId", houseId);

                    var flats = new List<FlatDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            flats.Add(new FlatDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Flat = dataReader.GetString("flat"),
                                TypeId = dataReader.GetInt32("type_id"),
                                TypeName = dataReader.GetString("Name"),
                            });
                        }
                        dataReader.Close();
                    }
                    return flats.OrderBy(s => s.TypeId).ThenBy(s => s.Flat?.PadLeft(6, '0')).ToArray();
                }
            }
        }

        public static string CreateRequest(int workerId, string phone, string fio, int addressId, int typeId, int? masterId, int? executerId, string description)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    "call CallCenter.WebCreateRequest(@WorkerId,@Phone,@Fio,@AddressId,@TypeId,@MasterId,@ExecuterId,@Desc);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Fio", fio);
                    cmd.Parameters.AddWithValue("@AddressId", addressId);
                    cmd.Parameters.AddWithValue("@TypeId", typeId);
                    cmd.Parameters.AddWithValue("@MasterId", masterId);
                    cmd.Parameters.AddWithValue("@ExecuterId", executerId);
                    cmd.Parameters.AddWithValue("@Desc", description);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        return dataReader.GetNullableString("requestId");
                    }
                }
            }
        }

        public static void AttachFileToRequest(int userId, int requestId, string fileName, string generatedFileName)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                        new MySqlCommand(@"insert into CallCenter.RequestAttachments(request_id,name,file_name,create_date,user_id)
 values(@RequestId,@Name,@FileName,sysdate(),@userId);", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@Name", fileName);
                    cmd.Parameters.AddWithValue("@FileName", generatedFileName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<AttachmentDto> GetAttachments(int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"SELECT a.id,a.request_id,a.name,a.file_name,a.create_date,u.id user_id,u.SurName,u.FirstName,u.PatrName FROM CallCenter.RequestAttachments a
 join CallCenter.Users u on u.id = a.user_id where a.deleted = 0 and a.request_id = @requestId", conn))
                {
                    cmd.Parameters.AddWithValue("@requestId", requestId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var attachments = new List<AttachmentDto>();
                        while (dataReader.Read())
                        {
                            attachments.Add(new AttachmentDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                FileName = dataReader.GetString("file_name"),
                                CreateDate = dataReader.GetDateTime("create_date"),
                                RequestId = dataReader.GetInt32("request_id"),
                                User = new UserDto()
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("SurName"),
                                    FirstName = dataReader.GetNullableString("FirstName"),
                                    PatrName = dataReader.GetNullableString("PatrName"),
                                }
                            });
                        }
                        dataReader.Close();
                        return attachments;
                    }
                }
            }
        }

        public static List<NoteDto> GetNotes(int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    @"SELECT n.id,n.operation_date,n.request_id,n.user_id,n.note,u.SurName,u.FirstName,u.PatrName from CallCenter.RequestNoteHistory n
join CallCenter.Users u on u.id = n.user_id where request_id = @RequestId order by operation_date";
                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var noteList = new List<NoteDto>();
                        while (dataReader.Read())
                        {
                            noteList.Add(new NoteDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Date = dataReader.GetDateTime("operation_date"),
                                Note = dataReader.GetNullableString("note"),
                                User = new UserDto
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("SurName"),
                                    FirstName = dataReader.GetNullableString("FirstName"),
                                    PatrName = dataReader.GetNullableString("PatrName"),
                                },
                            });
                        }
                        dataReader.Close();
                        return noteList;
                    }
                }

            }
        }

        public static void AddNewState(int requestId, int stateId, int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestStateHistory (request_id,operation_date,user_id,state_id) 
    values(@RequestId,sysdate(),@UserId,@StatusId);", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@StatusId", stateId);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd =
                        new MySqlCommand(
                            @"update CallCenter.Requests set state_id = @StatusId where id = @RequestId", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@StatusId", stateId);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

        }
    }
}

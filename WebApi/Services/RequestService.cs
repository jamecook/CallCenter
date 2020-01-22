using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NLog;
using NLog.Filters;
using RestSharp;
using Stimulsoft.Report;
using WebApi.Models;

namespace WebApi.Services
{
    public static class RequestService
    {
        private static string _connectionString;
        private static string _connectionStringAts;
        private static string _sipServer;
        private static readonly Logger _logger;

        static RequestService()
        {
            _connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", "192.168.1.130",
                "asterisk", "mysqlasterisk", "CallCenter");
            _connectionStringAts = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8",
                "151.248.121.220",
                "zerg", "Dispex1411Zerg", "asterisk");
            _sipServer = "@151.248.121.220:6050";
            _logger = LogManager.GetCurrentClassLogger();

        }

        public static WebUserDto WebLogin(string userName, string password)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand($"Call CallCenter.DispexLogin2('{userName}','{password}')", conn)
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
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                WorkerId = dataReader.GetInt32("id"),
                                ServiceCompanyId = dataReader.GetInt32("service_company_id"),
                                SpecialityId = dataReader.GetInt32("speciality_id"),
                                CanCreateRequestInWeb = dataReader.GetBoolean("can_create_in_web"),
                                CanCloseRequest = dataReader.GetBoolean("can_close_request"),
                                CanChangeStatus = dataReader.GetBoolean("can_change_status"),
                                CanChangeImmediate = dataReader.GetNullableInt("change_immediate") == 1,
                                CanChangeChargeable = dataReader.GetNullableInt("change_chargeable") == 1,
                                CanChangeDescription = dataReader.GetNullableInt("change_description") == 1,
                                CanChangeAddress = dataReader.GetNullableInt("change_address") == 1,
                                CanChangeServiceType = dataReader.GetNullableInt("change_service_type") == 1,
                                CanChangeExecuteDate = dataReader.GetBoolean("can_change_execute_date"),
                                CanSetRating = dataReader.GetBoolean("can_set_rating"),
                                AllowStatistics = dataReader.GetBoolean("allow_statistics"),
                                AllowCalendar = dataReader.GetBoolean("allow_calendar"),
                                AllowDocs = dataReader.GetBoolean("allow_docs"),
                                OnlyImmediate = dataReader.GetBoolean("only_immediate"),
                                CanChangeExecutors = dataReader.GetBoolean("can_change_executors"),
                                ServiceCompanyFilter = dataReader.GetBoolean("show_all_request"),
                                EnableAdminPage = dataReader.GetBoolean("enable_admin_page"),
                                PushId = dataReader.GetString("guid"),
                            };
                        }

                        dataReader.Close();
                    }
                }

                return null;
            }
        }

        internal static string VoIpPush(string account)
        {
            if (account?.Length != 11)
            {
                return null;
            }

            var houseStr = account.Substring(0, 7);
            var flatStr = account.Substring(7, 4);
            long.TryParse(houseStr, out long houseId);
            long.TryParse(flatStr, out long flat);
            var addresses = new List<PushIdAndAddressDto>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand($"Call CallCenter.DoopPhoneGetClients(@houseId,@flat);", conn))
                {
                    cmd.Parameters.AddWithValue("@houseId", houseId);
                    cmd.Parameters.AddWithValue("@flat", flat);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            addresses.Add(new PushIdAndAddressDto
                            {
                                PushId = dataReader.GetNullableString("push_id"),
                                DeviceId = dataReader.GetNullableString("device_uid"),
                                SipPhone = dataReader.GetNullableString("sip_account"),
                                Secret = dataReader.GetNullableString("sip_secret"),
                                SipId = dataReader.GetNullableString("sip_id"),
                                Address = new AddressDto()
                                {
                                    Id = dataReader.GetInt32("id"),
                                    HouseId = dataReader.GetInt32("house_id"),
                                    StreetPrefix = dataReader.GetNullableString("prefix_name"),
                                    StreetName = dataReader.GetNullableString("street_name"),
                                    Building = dataReader.GetNullableString("building"),
                                    Corpus = dataReader.GetNullableString("corps"),
                                    Flat = dataReader.GetNullableString("flat"),
                                    AddressType = dataReader.GetNullableString("address_type"),
                                    IntercomId = dataReader.GetNullableString("intercomId"),
                                }
                            });
                        }

                        dataReader.Close();
                    }
                }

                addresses = addresses.Where(a => !string.IsNullOrEmpty(a.SipPhone)).ToList();

                List<Task<string>> tasks = new List<Task<string>>();
                foreach (var dto in addresses)
                {
                    tasks.Add(Task.Factory.StartNew((b) => Action(b), dto));
                }

                _logger.Debug("----------------Start");
                while (tasks.Exists(t => t.Status == TaskStatus.Running))
                {
                    _logger.Debug(
                        $"----------------WaitAny Start task running count={tasks.Count(t => t.Status == TaskStatus.Running)}");
                    var results = Task.WaitAny(tasks.Where(t => t.Status == TaskStatus.Running).ToArray(), 40000);
                    _logger.Debug(
                        $"----------------WaitAny Stop task running count={tasks.Count(t => t.Status == TaskStatus.Running)}");
                    var loopSer = JsonConvert.SerializeObject(tasks.Where(t => t.Status == TaskStatus.RanToCompletion));
                    _logger.Debug($"----------------\r\n\r\n{loopSer}\r\n\r\n");

                    if (tasks.Exists(
                        t => t.Status == TaskStatus.RanToCompletion && (t.Result == "" || t.Result == "OK")))
                        break;
                }

                _logger.Debug("----------------Stop!!!!!!!");
                //var serialize = JsonConvert.SerializeObject(tasks);
                //_logger.Debug($"----------------\r\n\r\n\r\n\r\n{serialize}\r\n\r\n\r\n\r\n");
                return addresses.Count > 0
                    ? addresses.Select(a => a.SipPhone).Aggregate((i, j) => i + "&" + j)
                    : "SIP/127001";

            }
        }

        private static string Action(object o)
        {
            var dto = o as PushIdAndAddressDto;
            return CallVoIpPush(dto);
        }

        /*
         Запрос, который должен выполнить Астериск перед тем как послать вызов:

POST https://dispex.org:5000/sip/call_request 

headers = { 
   "Content-Type": "application/json", 
   "Authorization": "c621f6c2-8462-4712-a8b5-ab36b4dbrf020" 
}

body = {
   "addr" : "Демонстрационная ...."
   "addrId" : 123456
   "pushId"    : "push id" 
}
             */
        public static string CallVoIpPush(PushIdAndAddressDto clientDto)
        {
            var saveSampleUrl = "https://dispex.org:5000/v2/sip/call_request";
            //var saveSampleUrl = "https://dispex.org:5000/sip/call_request";

            var client = new RestClient(saveSampleUrl);
            var request = new RestRequest(Method.POST) {RequestFormat = RestSharp.DataFormat.Json};
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", "c621f6c2-8462-4712-a8b5-ab36b4dbrf020");
            var discar = new VoIpPushDto
            {
                PushId = clientDto.PushId,
                DeviceId = clientDto.DeviceId,
                AddrId = clientDto.Address.Id,
                Addr = clientDto.Address.FullAddress,
                SipId = clientDto.SipId

            };
            var dataRequest = request.AddJsonBody(discar);
            _logger.Debug($"----------------Start CallVoIpPush({clientDto.PushId},{clientDto.DeviceId})");
            var responce = client.Execute(dataRequest);
            var result = responce.Content;
            _logger.Debug($"----------------Stop CallVoIpPush({clientDto.PushId},{clientDto.DeviceId})");
            return result;
        }

        internal static PushIdsAndAddressDto[] GetBindDoorPushIds(string flat, string doorUid)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"CALL CallCenter.DoorPhoneGetAddressesAndPushId(@flat,@doorUid)",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@doorUid", doorUid);
                    cmd.Parameters.AddWithValue("@flat", flat);
                    var addresses = new List<PushIdAndAddressDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            addresses.Add(new PushIdAndAddressDto
                            {
                                PushId = dataReader.GetNullableString("push_id"),
                                Address = new AddressDto()
                                {
                                    Id = dataReader.GetInt32("id"),
                                    HouseId = dataReader.GetInt32("house_id"),
                                    StreetPrefix = dataReader.GetNullableString("prefix_name"),
                                    StreetName = dataReader.GetNullableString("street_name"),
                                    Building = dataReader.GetNullableString("building"),
                                    Corpus = dataReader.GetNullableString("corps"),
                                    Flat = dataReader.GetNullableString("flat"),
                                    AddressType = dataReader.GetNullableString("address_type"),
                                    IntercomId = dataReader.GetNullableString("intercomId"),
                                }
                            });
                        }

                        dataReader.Close();
                    }

                    var result = addresses.GroupBy(r => r.Address.Id).Select(k => new PushIdsAndAddressDto()
                    {
                        Address = k.FirstOrDefault(i => i.Address.Id == k.Key).Address,
                        PushIds = k.Select(i => i.PushId).ToArray()
                    }).ToArray();
                    return result;
                }
            }
        }

        public static void BindDoorPhoneToHouse(int houseId, string doorUid, string doorNumber, string fromFlat,
            string toFlat)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"call CallCenter.AdminBindDoorPhoneToAddress(@houseId,@uid,@number,@from,@to);", conn))
                    {
                        cmd.Parameters.AddWithValue("@houseId", houseId);
                        cmd.Parameters.AddWithValue("@uid", doorUid);
                        cmd.Parameters.AddWithValue("@number", doorNumber);
                        cmd.Parameters.AddWithValue("@from", fromFlat);
                        cmd.Parameters.AddWithValue("@to", toFlat);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

        }

        internal static WebUserDto FindUserByToken(Guid refreshToken, DateTime expireDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {

                conn.Open();
                using (var cmd = new MySqlCommand($"Call CallCenter.DispexGetUserByToken(@Token,@ExpireDate)", conn)
                )
                {
                    cmd.Parameters.AddWithValue("@ExpireDate", expireDate);
                    cmd.Parameters.AddWithValue("@Token", refreshToken);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return new WebUserDto
                            {
                                UserId = dataReader.GetInt32("UserId"),
                                Login = dataReader.GetNullableString("Login"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                WorkerId = dataReader.GetInt32("id"),
                                ServiceCompanyId = dataReader.GetInt32("service_company_id"),
                                SpecialityId = dataReader.GetInt32("speciality_id"),
                                CanCreateRequestInWeb = dataReader.GetBoolean("can_create_in_web"),
                                CanCloseRequest = dataReader.GetBoolean("can_close_request"),
                                CanChangeStatus = dataReader.GetBoolean("can_change_status"),
                                CanChangeImmediate = dataReader.GetNullableInt("change_immediate") == 1,
                                CanChangeDescription = dataReader.GetNullableInt("change_description") == 1,
                                CanChangeChargeable = dataReader.GetNullableInt("change_chargeable") == 1,
                                CanChangeAddress = dataReader.GetNullableInt("change_address") == 1,
                                CanChangeServiceType = dataReader.GetNullableInt("change_service_type") == 1,
                                CanChangeExecuteDate = dataReader.GetBoolean("can_change_execute_date"),
                                CanSetRating = dataReader.GetBoolean("can_set_rating"),
                                AllowStatistics = dataReader.GetBoolean("allow_statistics"),
                                AllowCalendar = dataReader.GetBoolean("allow_calendar"),
                                AllowDocs = dataReader.GetBoolean("allow_docs"),
                                OnlyImmediate = dataReader.GetBoolean("only_immediate"),
                                CanChangeExecutors = dataReader.GetBoolean("can_change_executors"),
                                ServiceCompanyFilter = dataReader.GetBoolean("show_all_request"),
                                EnableAdminPage = dataReader.GetBoolean("enable_admin_page"),
                                PushId = dataReader.GetString("guid"),
                            };
                        }

                        dataReader.Close();
                    }
                }

                return null;
            }
        }


        internal static void AddRefreshToken(int workerId, Guid refreshToken, DateTime expireDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var cmd =
                        new MySqlCommand(@"CALL CallCenter.DispexAddToken(@WorkerId,@Token,@ExpireDate);", conn))
                    {
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.Parameters.AddWithValue("@Token", refreshToken);
                        cmd.Parameters.AddWithValue("@ExpireDate", expireDate);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
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

            var acts =
                requests.Select(
                    r =>
                        new
                        {
                            Id = r.Id,
                            CreateTime = r.CreateTime,
                            ContactPhones = r.ContactPhones,
                            Address = r.FullAddress,
                            Workers = r.Master?.FullName,
                            ClientPhones = r.ContactPhones,
                            Service = r.ParentService + ": " + r.Service,
                            Description = r.Description
                        }).ToArray();

            stiReport.RegBusinessObject("", "Acts", acts);
            stiReport.Render();
            var reportStream = new MemoryStream();
            stiReport.ExportDocument(StiExportFormat.Pdf, reportStream);
            reportStream.Position = 0;
            //File.WriteAllBytes("C:\\1\\act.pdf",reportStream.ToArray());
            return reportStream.ToArray();

        }

        public static byte[] GetRequestExcel(int workerId, int[] requestIds)
        {
            //Переделать на шаблон
            var requests = WebRequestsByIds(workerId, requestIds);
            var stiReport = new StiReport();
            stiReport.Load("templates\\requests.mrt");
            StiOptions.Engine.HideRenderingProgress = true;
            StiOptions.Engine.HideExceptions = true;
            StiOptions.Engine.HideMessages = true;

            var requestObj =
                requests.Select(
                    r =>
                        new
                        {
                            Id = r.Id,
                            ExecuteDate = r.ExecuteTime,
                            ContactPhones = r.ContactPhones,
                            Address = r.FullAddress,
                            Workers = r.Master?.FullName,
                            ClientPhones = r.ContactPhones,
                            Service = r.ParentService + ": " + r.Service,
                            Description = r.Description
                        }).ToArray();

            stiReport.RegBusinessObject("", "Requests", requestObj);
            stiReport.Render();
            var reportStream = new MemoryStream();
            stiReport.ExportDocument(StiExportFormat.Excel2007, reportStream);
            reportStream.Position = 0;
            //File.WriteAllBytes("C:\\1\\excel.xlsx",reportStream.ToArray());
            return reportStream.ToArray();

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

        public static WorkerDto[] GetWorkersByHouseAndService(int workerId, int houseId, int serviceId, int isMaster)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.DispexGetWorkersByHouseAndType(@WorkerId,@HouseId,@TypeId,@IsMaster)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@HouseId", houseId);
                    cmd.Parameters.AddWithValue("@TypeId", serviceId);
                    cmd.Parameters.AddWithValue("@IsMaster", isMaster);
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

        public static WorkerDto[] GetExecutersByWorkerId(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.DispexGetExecuters(@WorkerId)";
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

        public static byte[] GenerateExcel(RequestForListDto[] requests)
        {
            var saver = new MemoryStream();
            var prefix = File.ReadAllText("templates\\prefix.xml");
            var sufix = File.ReadAllText("templates\\sufix.xml");
            prefix = prefix.Replace("RowForReplace", (requests.Length + 1).ToString());
            var stringBuilder = new StringBuilder();

            //var start = new Stopwatch();
            //start.Start();
            foreach (var request in requests)
            {
                var row = $@"
<Row ss:AutoFitHeight=""1"">
    <Cell ss:StyleID=""s63""><Data ss:Type=""Number"">{request.Id}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.StreetName}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.Building}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.Corpus}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.Flat}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.ContactPhones}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.ParentService}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.Description}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.CreateTime.ToString("dd.MM.yyyy hh:mm")}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.ExecuteTime?.ToShortDateString()}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.ExecutePeriod}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String"">{request.LastNote}</Data></Cell>
    <Cell ss:StyleID=""s63""><Data ss:Type=""String""> </Data></Cell>
</Row>";
                stringBuilder.Append(row);
            }

            //var rows = start.ElapsedMilliseconds;
            var writer = new StreamWriter(saver);
            writer.Write(prefix);
            writer.Write(stringBuilder.ToString());
            writer.Write(sufix);
            writer.Flush();
            //var writeTime = start.ElapsedMilliseconds - rows;
            saver.Position = 0;
            return saver.ToArray();
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
                                OrderNum = dataReader.GetInt32("order_num"),
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
                                OrderNum = dataReader.GetInt32("order_num"),
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

        public static WebCallsDto[] GetWebCallsByRequestId(int workerId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = "CALL CallCenter.DispexGetRequestCalls(@CurWorker,@RequestId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    var states = new List<WebCallsDto>();
                    cmd.Parameters.AddWithValue("@CurWorker", workerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            states.Add(new WebCallsDto
                            {
                                Id = dataReader.GetInt32("id"),
                                PhoneNumber = dataReader.GetNullableString("CallerIdNum"),
                                Direction = dataReader.GetNullableString("direction"),
                                CreateTime = dataReader.GetDateTime("CreateTime"),
                                Duration = dataReader.GetInt32("duration"),
                                Extension = dataReader.GetNullableString("extension")
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

        public static byte[] DownloadFile(int requestId, string fileName, string rootDir)
        {
            if (!string.IsNullOrEmpty(rootDir) && Directory.Exists($"{rootDir}\\{requestId}"))
            {
                return File.ReadAllBytes($"{rootDir}\\{requestId}\\{fileName}");
            }

            return null;
        }

        public static byte[] DownloadPreview(int requestId, string fileName, string rootDir)
        {
            if (!string.IsNullOrEmpty(rootDir) && Directory.Exists($"{rootDir}\\{requestId}"))
            {
                if (File.Exists($"{rootDir}\\{requestId}\\preview\\{fileName}"))
                    return File.ReadAllBytes($"{rootDir}\\{requestId}\\preview\\{fileName}");
                try
                {
                    var image = System.Drawing.Image.FromFile($"{rootDir}\\{requestId}\\{fileName}");
                    var sized = ResizeImage(image, 450);
                    var buffer = new MemoryStream();
                    sized.Save(buffer, ImageFormat.Jpeg);
                    buffer.Position = 0;
                    if (!Directory.Exists($"{rootDir}\\{requestId}\\preview"))
                    {
                        Directory.CreateDirectory($"{rootDir}\\{requestId}\\preview");
                    }

                    File.WriteAllBytes($"{rootDir}\\{requestId}\\preview\\{fileName}", buffer.ToArray());
                    return buffer.ToArray();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            return null;
        }

        public static Bitmap ResizeImage(System.Drawing.Image image, int width)
        {
            if (image == null || image.Width == 0)
                return null;
            var scale = (double) image.Width / width;
            var newHeight = (int) (image.Height / scale);
            var destRect = new Rectangle(0, 0, width, newHeight);
            var destImage = new Bitmap(width, newHeight);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
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

        public static IList<ServiceDto> GetParentServices(int currentWorkerId, int? houseId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = @"CALL CallCenter.DispexGetRequestParrentTypes(@WorkerId,@HouseId)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@HouseId", houseId);
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

        public static RequestForListDto[] WebRequestListArrayParam(int currentWorkerId, int? requestId,
            bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate,
            DateTime executeToDate, int[] streetIds, int[] houseIds, int[] addressIds, int[] parentServiceIds,
            int[] serviceIds, int[] statusIds, int[] workerIds, int[] executerIds, int[] ratingIds, int[] companies,
            int[] warrantyIds, int[] immediateIds, int[] regionIds, bool badWork = false, bool garanty = false,
            bool onlyRetry = false, bool chargeable = false, bool onlyExpired = false, bool onlyByClient = false,
            string clientPhone = null)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.DispexGetRequestsBase(@CurWorker,@RequestId,@ByCreateDate,@FromDate,@ToDate,@ExecuteFromDate,@ExecuteToDate,@StreetIds,@HouseIds,@AddressIds,@ParentServiceIds,@ServiceIds,@StatusIds,@WorkerIds,@ExecuterIds,@WarrantyIds,@BadWork,@Garanty,@ClientPhone,@RatingIds,@CompaniesIds,@OnlyRetry,@OnlyChargeable,@ImmediateIds,@RegionIds,@onlyExpired,@onlyByClient,false)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@ByCreateDate", filterByCreateDate);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@ExecuteFromDate", executeFromDate);
                    cmd.Parameters.AddWithValue("@ExecuteToDate", executeToDate);
                    cmd.Parameters.AddWithValue("@OnlyRetry", onlyRetry);
                    cmd.Parameters.AddWithValue("@OnlyChargeable", chargeable);
                    cmd.Parameters.AddWithValue("@onlyExpired", onlyExpired);
                    cmd.Parameters.AddWithValue("@onlyByClient", onlyByClient);
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
                    cmd.Parameters.AddWithValue("@WarrantyIds",
                        warrantyIds != null && warrantyIds.Length > 0
                            ? warrantyIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@ImmediateIds",
                        immediateIds != null && immediateIds.Length > 0
                            ? immediateIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@RegionIds",
                        regionIds != null && regionIds.Length > 0
                            ? regionIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);

                    var requests = new List<RequestForListDto>();
                    cmd.CommandTimeout = 180;
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var recordId = dataReader.GetNullableInt("recordId");
                            requests.Add(new RequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                StreetPrefix = dataReader.GetString("prefix_name"),
                                RegionId = dataReader.GetNullableInt("region_id"),
                                RegionName = dataReader.GetNullableString("region_name"),
                                StreetName = dataReader.GetString("street_name"),
                                AddressType = dataReader.GetString("address_type"),
                                CompanyId = dataReader.GetNullableInt("service_company_id"),
                                CompanyName = dataReader.GetNullableString("company_name"),
                                HouseId = dataReader.GetInt32("house_id"),
                                StreetId = dataReader.GetInt32("street_id"),
                                AddressId = dataReader.GetInt32("address_id"),
                                Flat = dataReader.GetString("flat"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                Entrance = dataReader.GetNullableString("entrance"),
                                FirstRecordId = recordId,
                                HasRecord = recordId.HasValue,
                                HasAttachment = dataReader.GetBoolean("has_attach"),
                                IsBadWork = dataReader.GetBoolean("bad_work"),
                                IsImmediate = dataReader.GetBoolean("is_immediate"),
                                ByClient = dataReader.GetBoolean("by_client"),
                                Floor = dataReader.GetNullableString("floor"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                ParentServiceId = dataReader.GetNullableInt("parent_id"),
                                ServiceId = dataReader.GetNullableInt("service_id"),
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
                                FirstViewDate = dataReader.GetNullableDateTime("first_view_date"),
                                ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                                Rating = dataReader.GetNullableString("Rating"),
                                BadWork = dataReader.GetBoolean("bad_work"),
                                IsRetry = dataReader.GetBoolean("retry"),
                                Garanty = dataReader.GetBoolean("garanty"),
                                StatusId = dataReader.GetInt32("req_status_id"),
                                StatusOrder = dataReader.GetInt32("status_order"),
                                Status = dataReader.GetNullableString("Req_Status"),
                                TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                LastNote = dataReader.GetNullableString("last_note"),
                                ExistNote = dataReader.GetBoolean("exist_note"),
                                IsChargeable = dataReader.GetBoolean("is_chargeable"),
                                ClientName = dataReader.GetNullableString("client_name"),
                                CloseDate = dataReader.GetNullableDateTime("close_date"),
                                DoneDate = dataReader.GetNullableDateTime("done_date"),
                                GarantyId = dataReader.GetInt32("garanty"),
                                TaskId = dataReader.GetNullableInt("task_id"),
                                TaskStart = dataReader.GetNullableDateTime("task_from"),
                                TaskEnd = dataReader.GetNullableDateTime("task_to"),
                                TaskWorker = dataReader.GetNullableInt("task_worker_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("task_worker_id"),
                                        SurName = dataReader.GetNullableString("task_sur_name"),
                                        FirstName = dataReader.GetNullableString("task_first_name"),
                                        PatrName = dataReader.GetNullableString("task_patr_name"),
                                    }
                                    : null,
                            });
                        }

                        dataReader.Close();
                    }

                    return requests.ToArray();
                }
            }
        }

        public static int WebRequestListCount(int currentWorkerId, int? requestId, bool filterByCreateDate,
            DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int[] streetIds,
            int[] houseIds, int[] addressIds, int[] parentServiceIds, int[] serviceIds, int[] statusIds,
            int[] workerIds,
            int[] executerIds, int[] ratingIds, int[] companies, int[] warrantyIds, int[] immediateIds, int[] regionIds,
            bool badWork = false, bool garanty = false, bool onlyRetry = false, bool chargeable = false,
            bool onlyExpired = false, bool onlyByClient = false, string clientPhone = null)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            var requestCount = 0;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.DispexGetRequestsBase(@CurWorker,@RequestId,@ByCreateDate,@FromDate,@ToDate,@ExecuteFromDate,@ExecuteToDate,@StreetIds,@HouseIds,@AddressIds,@ParentServiceIds,@ServiceIds,@StatusIds,@WorkerIds,@ExecuterIds,@WarrantyIds,@BadWork,@Garanty,@ClientPhone,@RatingIds,@CompaniesIds,@OnlyRetry,@OnlyChargeable,@ImmediateIds,@RegionIds,@onlyExpired,@onlyByClient,true)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@ByCreateDate", filterByCreateDate);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@ExecuteFromDate", executeFromDate);
                    cmd.Parameters.AddWithValue("@ExecuteToDate", executeToDate);
                    cmd.Parameters.AddWithValue("@OnlyRetry", onlyRetry);
                    cmd.Parameters.AddWithValue("@OnlyChargeable", chargeable);
                    cmd.Parameters.AddWithValue("@onlyExpired", onlyExpired);
                    cmd.Parameters.AddWithValue("@onlyByClient", onlyByClient);
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
                    cmd.Parameters.AddWithValue("@WarrantyIds",
                        warrantyIds != null && warrantyIds.Length > 0
                            ? warrantyIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@ImmediateIds",
                        immediateIds != null && immediateIds.Length > 0
                            ? immediateIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@RegionIds",
                        regionIds != null && regionIds.Length > 0
                            ? regionIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        requestCount = dataReader.GetInt32("items");
                    }

                    return requestCount;
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
                                RegionId = dataReader.GetNullableInt("region_id"),
                                RegionName = dataReader.GetNullableString("region_name"),
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
                                ByClient = dataReader.GetBoolean("by_client"),
                                Floor = dataReader.GetNullableString("floor"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                ParentServiceId = dataReader.GetNullableInt("parent_id"),
                                ServiceId = dataReader.GetNullableInt("service_id"),
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
                                FirstViewDate = dataReader.GetNullableDateTime("first_view_date"),
                                ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                                Rating = dataReader.GetNullableString("Rating"),
                                BadWork = dataReader.GetBoolean("bad_work"),
                                IsRetry = dataReader.GetBoolean("retry"),
                                Garanty = dataReader.GetBoolean("garanty"),
                                StatusId = dataReader.GetInt32("req_status_id"),
                                Status = dataReader.GetNullableString("Req_Status"),
                                TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                LastNote = dataReader.GetNullableString("last_note"),
                                IsChargeable = dataReader.GetBoolean("is_chargeable"),
                                ClientName = dataReader.GetNullableString("client_name"),
                                TaskId = dataReader.GetNullableInt("task_id"),
                                TaskStart = dataReader.GetNullableDateTime("task_from"),
                                TaskEnd = dataReader.GetNullableDateTime("task_to"),
                                TaskWorker = dataReader.GetNullableInt("task_worker_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("task_worker_id"),
                                        SurName = dataReader.GetNullableString("task_sur_name"),
                                        FirstName = dataReader.GetNullableString("task_first_name"),
                                        PatrName = dataReader.GetNullableString("task_patr_name"),
                                    }
                                    : null,
                            });
                        }

                        dataReader.Close();
                    }

                    return requests.ToArray();
                }
            }
        }

        public static void AddNewNote(int requestId, string note, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"insert into CallCenter.RequestNoteHistory (request_id,operation_date,user_id,note,worker_id)
 values(@RequestId,sysdate(),0,@Note,@WorkerId);", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.Parameters.AddWithValue("@Note", note);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }


        }

        public static void SetRating(int workerId, int requestId, int ratingId, string description)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"CALL CallCenter.DispexSetRating2(@WorkerId,@RequestId,@RatingId,@Desc);", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.Parameters.AddWithValue("@RatingId", ratingId);
                        cmd.Parameters.AddWithValue("@Desc", description);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public static void SetExecuteDate(int workerId, int requestId, DateTime executeDate, string note)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"CALL CallCenter.DispexSetExecuteDate2(@WorkerId,@RequestId,@ExecuteDate,@Note);",
                                conn)
                    )
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.Parameters.AddWithValue("@ExecuteDate", executeDate);
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

        public static string CreateRequest(int workerId, string phone, string fio, int addressId, int typeId,
            int? masterId, int? executerId, string description, bool isChargeable = false, DateTime? executeDate = null,
            int warrantyId = 0, bool isImmediate = false, string platform = null)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    "call CallCenter.DispexCreateRequest(@WorkerId,@Phone,@Fio,@AddressId,@TypeId,@MasterId,@ExecuterId,@Desc,@IsChargeable,@ExecuteDate,@IsWarranty,@IsImmediate,@Origin);";
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
                    cmd.Parameters.AddWithValue("@IsChargeable", isChargeable);
                    cmd.Parameters.AddWithValue("@ExecuteDate", executeDate);
                    cmd.Parameters.AddWithValue("@IsWarranty", warrantyId);
                    cmd.Parameters.AddWithValue("@IsImmediate", isImmediate);
                    cmd.Parameters.AddWithValue("@Origin", platform);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        return dataReader.GetNullableString("requestId");
                    }
                }
            }
        }

        public static void AttachFileToRequest(int workerId, int requestId, string fileName, string generatedFileName)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand(
                        @"insert into CallCenter.RequestAttachments(request_id,name,file_name,create_date,user_id,worker_id)
 values(@RequestId,@Name,@FileName,sysdate(),0,@WorkerId);", conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
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
                        new MySqlCommand(
                            @"SELECT a.id,a.request_id,a.name,a.file_name,a.create_date,u.id user_id,u.SurName,u.FirstName,u.PatrName FROM CallCenter.RequestAttachments a
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

        public static List<NoteDto> GetNotes(int workerId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = "call CallCenter.DispexGetNotes(@WorkerId,@RequestId);";
                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
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
                                    Id = dataReader.GetInt32("create_user_id"),
                                    SurName = dataReader.GetNullableString("surname"),
                                    FirstName = dataReader.GetNullableString("firstname"),
                                    PatrName = dataReader.GetNullableString("patrname"),
                                },
                            });
                        }

                        dataReader.Close();
                        return noteList;
                    }
                }

            }
        }

        public static List<CityRegionDto> GetRegions(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = "call CallCenter.DispexGetCityRegions(@WorkerId);";
                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var regions = new List<CityRegionDto>();
                        while (dataReader.Read())
                        {
                            regions.Add(new CityRegionDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name")
                            });
                        }

                        dataReader.Close();
                        return regions;
                    }
                }

            }
        }

        public static void AddNewState(int requestId, int stateId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"insert into CallCenter.RequestStateHistory (request_id,operation_date,user_id,state_id,worker_id) 
    values(@RequestId,sysdate(),0,@StatusId,@WorkerId);", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
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

        //public static void SetNewService(int requestId, int serviceId, int workerId)
        //{
        //    using (var conn = new MySqlConnection(_connectionString))
        //    {
        //        conn.Open();
        //        var query = "call CallCenter.DispexSetService(@WorkerId,@Id,@NewService);";
        //        using (var cmd = new MySqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@WorkerId", workerId);
        //            cmd.Parameters.AddWithValue("@Id", requestId);
        //            cmd.Parameters.AddWithValue("@NewService", serviceId);
        //            cmd.ExecuteNonQuery();
        //        }
        //    }

        //}
        public static void SetNewImmediate(int requestId, int immediate, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.DispexSetImmediate(@WorkerId,@Id,@NewValue);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@NewValue", immediate);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void SetNewChargeable(int requestId, int chargeable, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.DispexSetChargeable(@WorkerId,@Id,@NewValue);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@NewValue", chargeable);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void SetNewAddress(int requestId, int address, int serviceType, int? masterId, int? executerId,
            int currentWorkerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    "call CallCenter.DispexSetAddress(@WorkerId,@Id,@NewValue,@ServiceId,@MasterId,@ExecuterId);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@NewValue", address);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceType);
                    cmd.Parameters.AddWithValue("@MasterId", masterId);
                    cmd.Parameters.AddWithValue("@ExecuterId", executerId);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void SetNewServiceType(int requestId, int serviceType, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.DispexSetServiceType(@WorkerId,@Id,@NewValue);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@NewValue", serviceType);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void SetNewDescription(int requestId, string description, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.DispexSetDescription(@WorkerId,@Id,@NewValue);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@NewValue", description);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void SetNewMaster(int requestId, int masterId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.DispexSetMaster(@WorkerId,@Id,@NewMaster);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@NewMaster", masterId);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void SetNewExecuter(int requestId, int executerId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.DispexSetExecutor(@WorkerId,@Id,@NewExecuter);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@NewExecuter", executerId);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void SetViewRequest(int requestId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.DispexViewRequest(@WorkerId,@Id);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static WarrantyDocDto[] WarrantyGetDocs(int currentWorkerId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.WarrantyGetDocs(@CurWorker,@RequestId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);

                    var docs = new List<WarrantyDocDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            docs.Add(new WarrantyDocDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Extension = dataReader.GetString("extension"),
                                RequestId = dataReader.GetNullableInt("request_id"),
                                CreateDate = dataReader.GetDateTime("create_date"),
                                InsertDate = dataReader.GetDateTime("insert_date"),
                                Direction = dataReader.GetString("direction"),

                                CreateWorker = new WorkerDto()
                                {
                                    Id = dataReader.GetInt32("worker_id"),
                                    SurName = dataReader.GetNullableString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                    Phone = dataReader.GetNullableString("phone"),
                                },
                                Organization = dataReader.GetNullableInt("org_id") != null
                                    ? new WarrantyOrganizationDto()
                                    {
                                        Id = dataReader.GetInt32("org_id"),
                                        Name = dataReader.GetNullableString("org_name"),
                                        Inn = dataReader.GetNullableString("org_inn"),
                                        DirectorFio = dataReader.GetNullableString("director_fio"),
                                    }
                                    : null,
                                Type = new WarrantyTypeDto()
                                {
                                    Id = dataReader.GetInt32("type_id"),
                                    Name = dataReader.GetNullableString("type_name"),
                                    IsAct = dataReader.GetBoolean("is_act"),
                                }

                            });
                        }

                        dataReader.Close();
                    }

                    return docs.ToArray();
                }
            }
        }

        public static WarrantyInfoDto WarrantyGetInfo(int currentWorkerId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.WarrantyGetInfo(@CurWorker,@RequestId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return new WarrantyInfoDto
                            {
                                Id = dataReader.GetInt32("id"),
                                RequestId = dataReader.GetInt32("request_id"),
                                StartDate = dataReader.GetNullableDateTime("start_date"),
                                BeginDate = dataReader.GetNullableDateTime("begin_date"),
                                EndDate = dataReader.GetNullableDateTime("end_date"),
                                InsertDate = dataReader.GetDateTime("insert_date"),
                                ContactName = dataReader.GetNullableString("contact_name"),
                                ContactPhone = dataReader.GetNullableString("contact_phone"),
                                OrgId = dataReader.GetNullableInt("org_id"),
                                Organization = dataReader.GetNullableInt("org_id") != null
                                    ? new WarrantyOrganizationDto()
                                    {
                                        Id = dataReader.GetInt32("org_id"),
                                        Name = dataReader.GetNullableString("org_name"),
                                        Inn = dataReader.GetNullableString("org_inn"),
                                        DirectorFio = dataReader.GetNullableString("director_fio"),
                                    }
                                    : null,

                            };
                        }

                        dataReader.Close();
                    }

                    return null;
                }
            }
        }

        public static void WarrantySetInfo(int workerId, WarrantyInfoDto info)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    "call CallCenter.WarrantyAddInfo(@WorkerId,@RequestId,@OrgId,@ContactName,@ContactPhone,@StartDate,@BeginDate,@EndDate);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@RequestId", info.RequestId);
                    cmd.Parameters.AddWithValue("@OrgId", info.OrgId);
                    cmd.Parameters.AddWithValue("@ContactName", info.ContactName);
                    cmd.Parameters.AddWithValue("@ContactPhone", info.ContactPhone);
                    cmd.Parameters.AddWithValue("@StartDate", info.StartDate);
                    cmd.Parameters.AddWithValue("@BeginDate", info.BeginDate);
                    cmd.Parameters.AddWithValue("@EndDate", info.EndDate);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void WarrantyAddDoc(int id, int? orgId, int typeId, string name, DateTime docDate,
            string fileName,
            string direction, int workerId, string extension)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    "call CallCenter.WarrantyAddDoc(@WorkerId,@Id,@OrgId,@TypeId,@Name,@FileName,@DocDate,@Direction,@Extension);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@OrgId", orgId);
                    cmd.Parameters.AddWithValue("@TypeId", typeId);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@DocDate", docDate);
                    cmd.Parameters.AddWithValue("@FileName", fileName);
                    cmd.Parameters.AddWithValue("@Direction", direction);
                    cmd.Parameters.AddWithValue("@Extension", extension);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void WarrantyDeleteDoc(int id, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.WarrantyDeleteDoc(@WorkerId,@Id);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void WarrantySetState(int id, int stateId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.WarrantySetState(@WorkerId,@Id,@State);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@State", stateId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static IEnumerable<WarrantyTypeDto> WarrantyGetDocTypes(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.WarrantyGetDocTypes(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<WarrantyTypeDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var type = new WarrantyTypeDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                IsAct = dataReader.GetBoolean("is_act"),
                            };
                            list.Add(type);
                        }

                        dataReader.Close();
                    }

                    return list;
                }
            }
        }

        public static IEnumerable<WarrantyOrganizationDto> WarrantyGetOrganizations(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.WarrantyGetOrgs(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<WarrantyOrganizationDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var type = new WarrantyOrganizationDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Inn = dataReader.GetNullableString("inn"),
                                DirectorFio = dataReader.GetNullableString("director_fio"),
                            };
                            list.Add(type);
                        }

                        dataReader.Close();
                    }

                    return list;
                }
            }
        }

        public static void WarrantyAddOrg(int workerId, WarrantyOrganizationDto organization)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.WarrantyCreateOrgs(@WorkerId,@Name,@Inn,@Fio);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Name", organization.Name);
                    cmd.Parameters.AddWithValue("@Inn", organization.Inn);
                    cmd.Parameters.AddWithValue("@Fio", organization.DirectorFio);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void WarrantyEditOrg(int workerId, WarrantyOrganizationDto organization)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.WarrantySetOrgs(@WorkerId,@Id,@Inn,@Fio);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", organization.Id);
                    cmd.Parameters.AddWithValue("@Inn", organization.Inn);
                    cmd.Parameters.AddWithValue("@Fio", organization.DirectorFio);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static WarrantyFileInfoDto WarrantyGetDocFileName(int workerId, int id)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.WarrantyGetDocFileName(@CurWorker,@DocId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", workerId);
                    cmd.Parameters.AddWithValue("@DocId", id);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return new WarrantyFileInfoDto
                            {
                                Id = dataReader.GetInt32("id"),
                                RequestId = dataReader.GetInt32("request_id"),
                                Name = dataReader.GetNullableString("name"),
                                FileName = dataReader.GetString("filename")
                            };
                        }

                        dataReader.Close();
                    }

                    return null;
                }
            }
        }


        public static ScheduleTaskDto[] GetScheduleTask(int currentWorkerId, int? workerId, DateTime fromDate,
            DateTime toDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(
                            @"CALL CallCenter.DispexGetScheduleTask(@CurrentWorkerId,@WorkerId,@FromDate,@ToDate)",
                            conn)
                )
                {
                    cmd.Parameters.AddWithValue("@CurrentWorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));

                    var taskDtos = new List<ScheduleTaskDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            taskDtos.Add(new ScheduleTaskDto
                            {
                                Id = dataReader.GetInt32("id"),
                                RequestId = dataReader.GetInt32("request_id"),
                                FromDate = dataReader.GetDateTime("from_date"),
                                ToDate = dataReader.GetDateTime("to_date"),
                                Worker = new ScheduleWorkerDto
                                {
                                    Id = dataReader.GetInt32("worker_id"),
                                    SurName = dataReader.GetNullableString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                    Phone = dataReader.GetNullableString("phone"),
                                }
                            });
                        }

                        dataReader.Close();
                    }

                    return taskDtos.ToArray();
                }
            }
        }

        public static ScheduleTaskDto[] GetAllScheduleTask(int currentWorkerId, int? workerId, DateTime fromDate,
            DateTime toDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(
                            @"CALL CallCenter.DispexGetAllScheduleTask(@CurrentWorkerId,@WorkerId,@FromDate,@ToDate)",
                            conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentWorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));

                    var taskDtos = new List<ScheduleTaskDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            taskDtos.Add(new ScheduleTaskDto
                            {
                                Id = dataReader.GetInt32("id"),
                                RequestId = dataReader.GetInt32("request_id"),
                                FromDate = dataReader.GetDateTime("from_date"),
                                ToDate = dataReader.GetDateTime("to_date"),
                                Worker = new ScheduleWorkerDto
                                {
                                    Id = dataReader.GetInt32("worker_id"),
                                    SurName = dataReader.GetNullableString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                    Phone = dataReader.GetNullableString("phone"),
                                }
                            });
                        }

                        dataReader.Close();
                    }

                    return taskDtos.ToArray();
                }
            }
        }

//        public ScheduleTaskDto GetScheduleTaskByRequestId(int requestId)
//        {
//            ScheduleTaskDto result = null;
//            var query = @"SELECT s.id,w.id worker_id,w.sur_name,w.first_name,w.patr_name,s.request_id,s.from_date,s.to_date,s.event_description FROM CallCenter.ScheduleTasks s
//join CallCenter.Workers w on s.worker_id = w.id
//where s.request_id = @RequestId and deleted = 0;";
//            using (var cmd = new MySqlCommand(query, _dbConnection))
//            {
//                cmd.Parameters.AddWithValue("@RequestId", requestId);

//                using (var dataReader = cmd.ExecuteReader())
//                {
//                    if (dataReader.Read())
//                    {
//                        result = new ScheduleTaskDto
//                        {
//                            Id = dataReader.GetInt32("id"),
//                            RequestId = dataReader.GetNullableInt("request_id"),
//                            Worker = new WorkerDto()
//                            {
//                                Id = dataReader.GetInt32("worker_id"),
//                                SurName = dataReader.GetString("sur_name"),
//                                FirstName = dataReader.GetNullableString("first_name"),
//                                PatrName = dataReader.GetNullableString("patr_name"),
//                            },
//                            FromDate = dataReader.GetDateTime("from_date"),
//                            ToDate = dataReader.GetDateTime("to_date"),
//                            EventDescription = dataReader.GetNullableString("event_description")
//                        };
//                    }
//                    dataReader.Close();
//                }
//                return result;
//            }
//        }

        public static string AddScheduleTask(int currentWorkerId, int workerId, int? requestId, DateTime fromDate,
            DateTime toDate, string eventDescription)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(
                            @"CALL CallCenter.DispexScheduleTaskAdd(@CurrentWorkerId,@WorkerId,@RequestId,@FromDate,@ToDate)",
                            conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentWorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        return dataReader.GetNullableString("taskId");
                    }

                }
            }
        }

        public static void UpdateScheduleTask(int currentWorkerId, int taskId, int workerId, int? requestId,
            DateTime fromDate,
            DateTime toDate, string eventDescription)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(
                            @"CALL CallCenter.DispexScheduleTaskUpdate(@CurrentWorkerId,@TaskId,@WorkerId,@RequestId,@FromDate,@ToDate)",
                            conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentWorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@TaskId", taskId);
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteScheduleTask(int currentWorkerId, int sheduleId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(
                            @"CALL CallCenter.DispexScheduleTaskDrop(@CurrentWorkerId,@TaskId)",
                            conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentWorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@TaskId", sheduleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static IEnumerable<ReportDto> ReportsGetAwailable(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.ReportGetAwailable(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<ReportDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var type = new ReportDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Url = dataReader.GetString("url"),
                            };
                            list.Add(type);
                        }

                        dataReader.Close();
                    }

                    return list;
                }
            }
        }

        public static ClientUserDto ClientLogin(string phone, string code, string deviceId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand($"Call CallCenter.ClientLogin('{phone}','{code}')", conn)
                )
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return new ClientUserDto
                            {
                                Id = dataReader.GetInt32("Id"),
                                Phone = dataReader.GetString("Phone"),
                                Name = dataReader.GetNullableString("name"),
                                PushId = dataReader.GetString("guid"),
                                DeviceId = deviceId
                            };
                        }

                        dataReader.Close();
                    }
                }

                return null;
            }
        }

        public static void AddClientRefreshToken(int clientId, Guid refreshToken, DateTime expireDate, string deviceId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var cmd =
                        new MySqlCommand(@"CALL CallCenter.ClientAddTokenV2(@ClientId,@Token,@ExpireDate,@deviceId);",
                            conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientId", clientId);
                        cmd.Parameters.AddWithValue("@Token", refreshToken);
                        cmd.Parameters.AddWithValue("@ExpireDate", expireDate);
                        cmd.Parameters.AddWithValue("@deviceId", deviceId);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public static ClientUserDto ClientFindByToken(Guid refreshToken, DateTime expireDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {

                conn.Open();
                using (var cmd = new MySqlCommand($"Call CallCenter.ClientGetByTokenV2(@Token,@ExpireDate)", conn)
                )
                {
                    cmd.Parameters.AddWithValue("@ExpireDate", expireDate);
                    cmd.Parameters.AddWithValue("@Token", refreshToken);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return new ClientUserDto
                            {
                                Id = dataReader.GetInt32("Id"),
                                Phone = dataReader.GetString("Phone"),
                                Name = dataReader.GetNullableString("name"),
                                PushId = dataReader.GetString("guid"),
                                DeviceId = dataReader.GetNullableString("device_uid"),
                            };
                        }

                        dataReader.Close();
                    }
                }

                return null;
            }
        }

        public static void ClientValidatePhone(string phone)
        {
            Random random = new Random();
            int randomNumber = random.Next(100000, 999999);
            var code = randomNumber.ToString();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var cmd =
                        new MySqlCommand(@"CALL CallCenter.ClientValidatePhone(@Phone,@Code);", conn))
                    {
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Code", code);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public static string ClientValidTest(string phone)
        {
            Random random = new Random();
            int randomNumber = random.Next(100000, 999999);
            var code = randomNumber.ToString();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var cmd =
                        new MySqlCommand(@"CALL CallCenter.ClientValidTest(@Phone,@Code);", conn))
                    {
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Code", code);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }

                return code;
            }
        }

        public static StreetDto[] GetStreetsByClient(int clientId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = "CALL CallCenter.ClientGetStreets(@ClientId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
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

        public static WebHouseDto[] GetHousesByStreetAndClientId(int clientId, int streetId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = @"CALL CallCenter.ClientGetHouses(@ClientId,@StreetId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
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

        public static FlatDto[] GetFlatsForClient(int clientId, int houseId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"CALL CallCenter.ClientGetFlats(@ClientId,@HouseId)", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
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

        public static ServiceDto[] GetParentServicesForClient(int clientId, int? houseId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = @"call CallCenter.ClientGetRequestParrentTypes(@ClientId,@HouseId)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@HouseId", houseId);

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

                    return services.ToArray();
                }
            }
        }

        public static ServiceDto[] GetServicesForClient(int clientId, int[] parentIds)
        {
            if (parentIds == null || parentIds.Length == 0)
                return new ServiceDto[0];
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var ids = parentIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j);
                var query =
                    $@"SELECT t1.id,t1.name,t1.can_send_sms,t2.id parent_id, t2.name parent_name FROM CallCenter.RequestTypes t1
                        left join CallCenter.RequestTypes t2 on t2.id = t1.parrent_id
                        where t1.parrent_id in ({ids}) and t1.for_client = 1 and t2.for_client = 1 and t1.enabled = 1 order by t2.name,t1.name";

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

                    return services.ToArray();
                }
            }
        }

        public static int AddAddress(int clientId, int addressId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"CALL CallCenter.ClientAddAddress(@ClientId,@AddressId)", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@AddressId", addressId);
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public static void SetAnnulledRequest(int clientId, int rId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"CALL CallCenter.ClientDropRequest(@clientId,@requestId)", conn))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);
                    cmd.Parameters.AddWithValue("@requestId", rId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static int GetExpiredRequestCount(int currentWorkerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"CALL CallCenter.DispexGetExpiredRequestCount(@WorkerId)", conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", currentWorkerId);
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public static RequestForListDto[] GetExpiredRequests(int currentWorkerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.DispexGetExpiredRequests(@CurWorker)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);

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
                                RegionId = dataReader.GetNullableInt("region_id"),
                                RegionName = dataReader.GetNullableString("region_name"),
                                StreetName = dataReader.GetString("street_name"),
                                AddressType = dataReader.GetString("address_type"),
                                CompanyId = dataReader.GetNullableInt("service_company_id"),
                                CompanyName = dataReader.GetNullableString("company_name"),
                                HouseId = dataReader.GetInt32("house_id"),
                                StreetId = dataReader.GetInt32("street_id"),
                                AddressId = dataReader.GetInt32("address_id"),
                                Flat = dataReader.GetString("flat"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                Entrance = dataReader.GetNullableString("entrance"),
                                FirstRecordId = recordId,
                                HasRecord = recordId.HasValue,
                                HasAttachment = dataReader.GetBoolean("has_attach"),
                                IsBadWork = dataReader.GetBoolean("bad_work"),
                                IsImmediate = dataReader.GetBoolean("is_immediate"),
                                ByClient = dataReader.GetBoolean("by_client"),
                                Floor = dataReader.GetNullableString("floor"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                ParentServiceId = dataReader.GetNullableInt("parent_id"),
                                ServiceId = dataReader.GetNullableInt("service_id"),
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
                                FirstViewDate = dataReader.GetNullableDateTime("first_view_date"),
                                ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                                Rating = dataReader.GetNullableString("Rating"),
                                BadWork = dataReader.GetBoolean("bad_work"),
                                IsRetry = dataReader.GetBoolean("retry"),
                                Garanty = dataReader.GetBoolean("garanty"),
                                StatusId = dataReader.GetInt32("req_status_id"),
                                Status = dataReader.GetNullableString("Req_Status"),
                                TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                LastNote = dataReader.GetNullableString("last_note"),
                                IsChargeable = dataReader.GetBoolean("is_chargeable"),
                                ClientName = dataReader.GetNullableString("client_name"),
                                CloseDate = dataReader.GetNullableDateTime("close_date"),
                                DoneDate = dataReader.GetNullableDateTime("done_date"),
                                GarantyId = dataReader.GetInt32("garanty"),
                                TaskId = dataReader.GetNullableInt("task_id"),
                                TaskStart = dataReader.GetNullableDateTime("task_from"),
                                TaskEnd = dataReader.GetNullableDateTime("task_to"),
                                TaskWorker = dataReader.GetNullableInt("task_worker_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("task_worker_id"),
                                        SurName = dataReader.GetNullableString("task_sur_name"),
                                        FirstName = dataReader.GetNullableString("task_first_name"),
                                        PatrName = dataReader.GetNullableString("task_patr_name"),
                                    }
                                    : null,
                            });
                        }

                        dataReader.Close();
                    }

                    return requests.ToArray();
                }
            }
        }

        public static void DeleteAddress(int clientId, int addressId)
        {
            var devIds = new List<int>();
            using (var devConn = new MySqlConnection(_connectionString))
            {
                devConn.Open();
                using (var cmd = new MySqlCommand(@"CALL CallCenter.ClientGetDevicesForDeleteV2(@ClientId,@AddressId)",
                    devConn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@AddressId", addressId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            devIds.Add(dataReader.GetInt32("id"));
                        }

                        dataReader.Close();
                    }
                }
            }

            foreach (var dev in devIds)
            {
                DeleteSipAccount(dev);
            }

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"CALL CallCenter.ClientDeleteAddressV2(@ClientId,@AddressId)", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@AddressId", addressId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static string ClientCreateRequest(int clientId, int addressId, int typeId, string description,
            string origin)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    "call CallCenter.ClientCreateRequest20191030(@ClientId,@AddressId,@TypeId,@Desc, @Origin);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@AddressId", addressId);
                    cmd.Parameters.AddWithValue("@TypeId", typeId);
                    cmd.Parameters.AddWithValue("@Desc", description);
                    cmd.Parameters.AddWithValue("@Origin", origin);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        return dataReader.GetNullableString("requestId");
                    }
                }
            }
        }

        public static AddressDto[] GetAddresses(int clientId, string deviceId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"CALL CallCenter.ClientGetAddressesV2(@ClientId,@DeviceId)", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                    var addresses = new List<AddressDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            addresses.Add(new AddressDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                HouseId = dataReader.GetInt32("house_id"),
                                StreetPrefix = dataReader.GetNullableString("prefix_name"),
                                StreetName = dataReader.GetNullableString("street_name"),
                                Building = dataReader.GetNullableString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                Flat = dataReader.GetNullableString("flat"),
                                AddressType = dataReader.GetNullableString("address_type"),
                                IntercomId = dataReader.GetNullableString("intercomId"),
                                SipId = dataReader.GetNullableString("sip_id"),
                                CanBeCalled = dataReader.GetNullableBoolean("can_be_called"),
                            });
                        }

                        dataReader.Close();
                    }

                    return addresses.ToArray();
                }
            }
        }

        public static void CanBeCalled(int clientId, string deviceId, int addressId, bool value)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand(@"CALL CallCenter.ClientSetCanBeCalledV2(@ClientId,@DeviceId,@AddressId,@Value)",
                        conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                    cmd.Parameters.AddWithValue("@AddressId", addressId);
                    cmd.Parameters.AddWithValue("@Value", value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static ClientRequestForListDto[] ClientRequestListArrayParam(int clientId, int? requestId,
            DateTime fromDate, DateTime toDate, int[] addressIds)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.ClientGetRequests(@ClientId,@RequestId,@FromDate,@ToDate,@AddressIds)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@AddressIds",
                        addressIds != null && addressIds.Length > 0
                            ? addressIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);


                    var requests = new List<ClientRequestForListDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var recordId = dataReader.GetNullableInt("recordId");
                            requests.Add(new ClientRequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                StreetPrefix = dataReader.GetString("prefix_name"),
                                RegionId = dataReader.GetNullableInt("region_id"),
                                RegionName = dataReader.GetNullableString("region_name"),
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
                                CanBeDeleted = dataReader.GetBoolean("can_be_deleted"),
                                CanBeClosed = dataReader.GetBoolean("can_be_closed"),
                                Floor = dataReader.GetNullableString("floor"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                ParentServiceId = dataReader.GetNullableInt("parent_id"),
                                ServiceId = dataReader.GetNullableInt("service_id"),
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
                                ExecuteTime = dataReader.GetNullableDateTime("execute_date"),
                                ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                                Rating = dataReader.GetNullableString("Rating"),
                                Garanty = dataReader.GetBoolean("garanty"),
                                StatusId = dataReader.GetInt32("req_status_id"),
                                Status = dataReader.GetNullableString("Req_Status"),
                                TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                LastNote = dataReader.GetNullableString("last_note"),
                                IsChargeable = dataReader.GetBoolean("is_chargeable"),
                                CloseDate = dataReader.GetNullableDateTime("close_date"),
                                DoneDate = dataReader.GetNullableDateTime("done_date"),
                                GarantyId = dataReader.GetInt32("garanty"),
                                TaskId = dataReader.GetNullableInt("task_id"),
                                TaskStart = dataReader.GetNullableDateTime("task_from"),
                                TaskEnd = dataReader.GetNullableDateTime("task_to"),
                                TaskWorker = dataReader.GetNullableInt("task_worker_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("task_worker_id"),
                                        SurName = dataReader.GetNullableString("task_sur_name"),
                                        FirstName = dataReader.GetNullableString("task_first_name"),
                                        PatrName = dataReader.GetNullableString("task_patr_name"),
                                    }
                                    : null,
                            });
                        }

                        dataReader.Close();
                    }

                    return requests.ToArray();
                }
            }
        }

        public static AttachmentDto[] ClientGetAttachments(int clientId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"call CallCenter.ClientGetAttachments(@clientId,@requestId)", conn))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);
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
                                CanBeDeleted = dataReader.GetBoolean("can_be_deleted"),
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
                        return attachments.ToArray();
                    }
                }
            }
        }

        public static void ClientDropAttachment(int clientId, int requestId, int attachId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"call CallCenter.ClientDropAttachment(@clientId,@requestId, @attachId)",
                            conn))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    cmd.Parameters.AddWithValue("@attachId", attachId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void ClientAttachFileToRequest(int clientId, int requestId, string fileName,
            string generatedFileName)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand(
                        @"insert into CallCenter.RequestAttachments(request_id,name,file_name,create_date,user_id,client_id)
 values(@RequestId,@Name,@FileName,sysdate(),100,@ClientId);", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@Name", fileName);
                    cmd.Parameters.AddWithValue("@FileName", generatedFileName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static NoteDto[] ClientGetNotes(int clientId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = "call CallCenter.ClientGetNotes(@Client,@RequestId);";
                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Client", clientId);
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
                                    Id = dataReader.GetInt32("create_user_id"),
                                    SurName = dataReader.GetNullableString("surname"),
                                    FirstName = dataReader.GetNullableString("firstname"),
                                    PatrName = dataReader.GetNullableString("patrname"),
                                },
                            });
                        }

                        dataReader.Close();
                        return noteList.ToArray();
                    }
                }
            }
        }

        public static void ClientAddNewNote(int clientId, int requestId, string note)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"call CallCenter.ClientAddNote(@ClientId,@RequestId,@Note);", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@ClientId", clientId);
                        cmd.Parameters.AddWithValue("@Note", note);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public static void ClientCloseRequest(int clientId, int requestId, int ratingId, string description)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"CALL CallCenter.ClientCloseRequest(@ClientId,@RequestId,@RatingId,@Desc);", conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientId", clientId);
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@RatingId", ratingId);
                        cmd.Parameters.AddWithValue("@Desc", description);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public static void DismissRequest(int clientId, int requestId, string description)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"CALL CallCenter.ClientDismissRequest(@ClientId,@RequestId,@Desc);", conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientId", clientId);
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@Desc", description);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public static void BindDoorPhone(string phone, string doorUid, string deviceId, int addressId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                int devId = 0;
                string sipAccount = string.Empty;
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"call CallCenter.ClientBindDoorPhoneWithDevice(@clientPhone,@uid,@deviceId,@addressId);",
                                conn))
                    {
                        cmd.Parameters.AddWithValue("@clientPhone", phone);
                        cmd.Parameters.AddWithValue("@uid", doorUid);
                        cmd.Parameters.AddWithValue("@deviceId", deviceId);
                        cmd.Parameters.AddWithValue("@addressId", addressId);
                        using (var dataReader = cmd.ExecuteReader())
                        {
                            if (dataReader.Read())
                            {
                                devId = dataReader.GetInt32("id");
                                sipAccount = dataReader.GetNullableString("sip_account");
                            }

                            dataReader.Close();
                        }

                        if (string.IsNullOrEmpty(sipAccount))
                        {
                            var password = (addressId + devId).ToString().PadLeft(9, '0');
                            //Создаем аккаунт на удаленной базе астериск
                            var sip = CreateSipAccount(devId, password);
                            using (
                                var cmd2 =
                                    new MySqlCommand(
                                        @"call CallCenter.ClientBindDoorAddSip(@deviceId,@sipAccount,@secret);", conn))
                            {
                                cmd2.Parameters.AddWithValue("@deviceId", devId);
                                cmd2.Parameters.AddWithValue("@sipAccount", "SIP/" + sip);
                                cmd2.Parameters.AddWithValue("@secret", password);
                                cmd2.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }
            }
        }

        private static string CreateSipAccount(int devId, string password)
        {
            string sipAccount = devId.ToString().PadLeft(10, '0');
            string secret = password;
            using (var conn = new MySqlConnection(_connectionStringAts))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"INSERT INTO asterisk.sippeers
(name, defaultuser, host, type, context, secret,directmedia, nat, callgroup, language, disallow, allow, callerid, regexten, qualify, rtpkeepalive, `call-limit`)
VALUES
(@SipAccount, @SipAccount, 'dynamic', 'friend', 'outcalling', @Secret,'no', 'force_rport,comedia', '1', 'ru', 'all', 'alaw', @SipAccount, @SipAccount,'yes','10','1');",
                                conn))
                    {
                        cmd.Parameters.AddWithValue("@SipAccount", sipAccount);
                        cmd.Parameters.AddWithValue("@Secret", secret);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            return sipAccount;
        }

        private static void DeleteSipAccount(int devId)
        {
            string sipAccount = devId.ToString().PadLeft(10, '0');
            using (var conn = new MySqlConnection(_connectionStringAts))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"delete from asterisk.sippeers where name = @SipAccount;", conn))
                    {
                        cmd.Parameters.AddWithValue("@SipAccount", sipAccount);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public static bool GetDoorPhone(string phone, string doorUid, int addressId, string deviceId)
        {
            var result = false;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(
                            @"call CallCenter.ClientGetBindDoorPhoneV2(@clientPhone,@uid,@addressId,@deviceId);", conn))
                {
                    cmd.Parameters.AddWithValue("@clientPhone", phone);
                    cmd.Parameters.AddWithValue("@uid", doorUid);
                    cmd.Parameters.AddWithValue("@addressId", addressId);
                    cmd.Parameters.AddWithValue("@deviceId", deviceId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            result = dataReader.GetBoolean("result");
                        }

                        dataReader.Close();
                        return result;
                    }
                }
            }
        }

        public static bool ExistsSipPhone(int addressId)
        {
            var result = 0;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"call CallCenter.DoopPhoneExistsSip(@addressId);", conn))
                {
                    cmd.Parameters.AddWithValue("@addressId", addressId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            result = dataReader.GetInt32("sip_count");
                        }

                        dataReader.Close();
                        return result > 0;
                    }
                }
            }
        }

        public static string[] GetBindDoorPushIdsOld(string flat, string doorUid)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"call CallCenter.ClientGetBindDoorPushIds(@flat,@uid);", conn))
                {
                    cmd.Parameters.AddWithValue("@flat", flat);
                    cmd.Parameters.AddWithValue("@uid", doorUid);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var result = new List<string>();
                        if (dataReader.Read())
                        {
                            result.Add(dataReader.GetString("guid"));
                        }

                        dataReader.Close();
                        return result.ToArray();
                    }
                }
            }
        }


        public static IEnumerable<DocTypeDto> DocsGetTypes(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL docs_pack.get_types(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<DocTypeDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var type = new DocTypeDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                            };
                            list.Add(type);
                        }

                        dataReader.Close();
                    }

                    return list;
                }
            }
        }
        public static IEnumerable<DocTypeDto> DocsGetOrdTypes(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL docs_pack.get_organizational_types(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<DocTypeDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var type = new DocTypeDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                            };
                            list.Add(type);
                        }

                        dataReader.Close();
                    }

                    return list;
                }
            }
        }

        public static int DocsAddOrganisations(int workerId, DocOrgDto organisationDto)
        {
            int newId;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL docs_pack.add_org(@WorkerId,@Name,@Inn,@Fio)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Name", organisationDto.Name);
                    cmd.Parameters.AddWithValue("@Inn", organisationDto.Inn);
                    cmd.Parameters.AddWithValue("@Fio", organisationDto.Director);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        newId = dataReader.GetInt32("newId");
                    }
                }

                return newId;
            }
        }
        public static void DocsUpdateOrganisations(int workerId, int orgId, DocOrgDto organisationDto)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL docs_pack.update_org(@WorkerId,@OrgId, @Name,@Inn,@Fio)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@OrgId", orgId);
                    cmd.Parameters.AddWithValue("@Name", organisationDto.Name);
                    cmd.Parameters.AddWithValue("@Inn", organisationDto.Inn);
                    cmd.Parameters.AddWithValue("@Fio", organisationDto.Director);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void DocsDeleteOrganisations(int workerId, int orgId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL docs_pack.delete_org(@WorkerId,@OrgId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@OrgId", orgId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static IEnumerable<DocOrgDto> DocsGetOrganisations(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL docs_pack.get_organisation(@WorkerId,null)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<DocOrgDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var type = new DocOrgDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Inn = dataReader.GetString("inn"),
                                Director = dataReader.GetNullableString("director_fio"),
                            };
                            list.Add(type);
                        }

                        dataReader.Close();
                    }

                    return list;
                }
            }
        }

        public static IEnumerable<DocStatusDto> DocsGetStatuses(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL docs_pack.get_statuses(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<DocStatusDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var type = new DocStatusDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                            };
                            list.Add(type);
                        }

                        dataReader.Close();
                    }

                    return list;
                }
            }
        }

        public static IEnumerable<DocDto> DocsGetList(int workerId, DateTime fromDate, DateTime toDate, string inNumber,
            int[] orgs, int[] statuses, int[] types, int? documentId, int? appointedWorkerId, int[] streets, int[] houses, int[] addresses)
        {
            var findFromDate = documentId.HasValue ? DateTime.MinValue : fromDate.Date;
            var findToDate = documentId.HasValue ? DateTime.MaxValue : toDate.Date.AddDays(1).AddSeconds(-1);
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "call docs_pack.get_docs(@CurWorker,@FromDate,@ToDate, @InNumber, @Orgs, @Statuses, @Types, @documentId, @appointedWorkerId, @streets, @houses, @addresses);";

                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", workerId);
                    cmd.Parameters.AddWithValue("@FromDate", findFromDate);
                    cmd.Parameters.AddWithValue("@ToDate", findToDate);
                    cmd.Parameters.AddWithValue("@InNumber", inNumber);
                    cmd.Parameters.AddWithValue("@documentId", documentId);
                    cmd.Parameters.AddWithValue("@appointedWorkerId", appointedWorkerId);

                    cmd.Parameters.AddWithValue("@Orgs",
                        orgs != null && orgs.Length > 0
                            ? orgs.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@Statuses",
                        statuses != null && statuses.Length > 0
                            ? statuses.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@Types",
                        types != null && types.Length > 0
                            ? types.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);

                    cmd.Parameters.AddWithValue("@streets",
                        streets != null && streets.Length > 0
                            ? streets.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);

                    cmd.Parameters.AddWithValue("@houses",
                        houses != null && houses.Length > 0
                            ? houses.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);

                    cmd.Parameters.AddWithValue("@addresses",
                        addresses != null && addresses.Length > 0
                            ? addresses.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);

                    var requests = new List<DocDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            DocAttachOrgDto[] attachOrgs = null;
                            var attachOrgStr = dataReader.GetNullableString("orgs");
                            if (!string.IsNullOrEmpty(attachOrgStr))
                            {
                                attachOrgs = JsonConvert.DeserializeObject<DocAttachOrgDto[]>(attachOrgStr);
                            }

                            var docType = dataReader.GetNullableInt("type_id");
                            requests.Add(new DocDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                CreateDate = dataReader.GetDateTime("create_date"),
                                CreateUser = new UserDto
                                {
                                    Id = dataReader.GetInt32("create_worker_id"),
                                    SurName = dataReader.GetNullableString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                },
                                ClientAddress = dataReader.GetNullableInt("address_id") != null
                                    ? new ShortAddressDto
                                    {
                                        Id = dataReader.GetInt32("address_id"),
                                        StreetId = dataReader.GetInt32("street_id"),
                                        HouseId = dataReader.GetInt32("house_id"),
                                        StreetName = dataReader.GetNullableString("street_name"),
                                        StreetPrefix = dataReader.GetNullableString("street_prefix"),
                                        Building = dataReader.GetNullableString("building"),
                                        Corpus = dataReader.GetNullableString("corps"),
                                        Flat = dataReader.GetNullableString("flat"),
                                    }
                                    : null,
                                DocNumber = docType == 2 ? dataReader.GetNullableString("doc_number") 
                                          :  docType == 3 ? dataReader.GetNullableString("doc_inc_number")
                                          : (dataReader.GetNullableString("doc_number") ?? "") + "/" +
                                            (dataReader.GetNullableString("doc_inc_number") ?? ""),

                                DocDate = dataReader.GetDateTime("doc_date"),
                                DocYear = dataReader.GetInt32("doc_year"),
                                InNumber = docType == 2 ? dataReader.GetNullableString("doc_inc_number")
                                          : dataReader.GetNullableString("addit_number"),

                                InDate = docType == 2 ? dataReader.GetDateTime("create_date")
                                          : dataReader.GetNullableDateTime("addit_date"),

                                Topic = dataReader.GetNullableString("topic"),
                                Description = dataReader.GetNullableString("descript"),
                                DoneDate = dataReader.GetNullableDateTime("done_date"),
                                AttachCount = dataReader.GetInt32("attach_count"),
                                Org = dataReader.GetNullableInt("org_id") != null
                                    ? new DocOrgDto
                                    {
                                        Id = dataReader.GetInt32("org_id"),
                                        Name = dataReader.GetNullableString("org_name"),
                                        Inn = dataReader.GetNullableString("org_inn"),
                                    }
                                    : null,
                                Status = dataReader.GetNullableInt("status_id") != null
                                    ? new DocStatusDto
                                    {
                                        Id = dataReader.GetInt32("status_id"),
                                        Name = dataReader.GetNullableString("status_name"),
                                    }
                                    : null,
                                Type = docType != null
                                    ? new DocTypeDto
                                    {
                                        Id = dataReader.GetInt32("type_id"),
                                        Name = dataReader.GetNullableString("type_name"),
                                    }
                                    : null,
                                AppointedWorker = dataReader.GetNullableInt("appointed_worker_id") != null
                                    ? new UserDto()
                                    {
                                        Id = dataReader.GetInt32("appointed_worker_id"),
                                        SurName = dataReader.GetNullableString("asur_name"),
                                        FirstName = dataReader.GetNullableString("afirst_name"),
                                        PatrName = dataReader.GetNullableString("apatr_name"),
                                    }
                                    : null,
                                OrganizationalType = dataReader.GetNullableInt("organiz_type_id") != null
                                    ? new OrganizationalTypeDto()
                                    {
                                        Id = dataReader.GetInt32("organiz_type_id"),
                                        Name = dataReader.GetNullableString("organiz_type_name"),
                                    }
                                    : null,
                                AttachOrg = attachOrgs

                            });
                        }

                        dataReader.Close();
                    }

                    return requests.ToArray();
                }
            }
        }

        public static string CreateDoc(int workerId, int typeId, string topic, string docNumber, DateTime docDate,
            string inNumber,
            DateTime? inDate, int? orgId, OrgDocDto[] orgs, int? organizationalTypeId, string description,
            int? appoinedWorkerId, int? addressId)
        {
            var newDocId = string.Empty;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    "call docs_pack.create_doc(@WorkerId,@TypeId,@Topic, @DocNumber, @DocDate, @InNumber,@InDate,@OrgId,@OrganizTypeId,@Descript,@appoinedWorkerId,@addressId);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@TypeId", typeId);
                    cmd.Parameters.AddWithValue("@Topic", topic);
                    cmd.Parameters.AddWithValue("@DocNumber", docNumber);
                    cmd.Parameters.AddWithValue("@DocDate", docDate);
                    cmd.Parameters.AddWithValue("@InNumber", inNumber);
                    cmd.Parameters.AddWithValue("@InDate", inDate);
                    cmd.Parameters.AddWithValue("@OrgId", orgId);
                    cmd.Parameters.AddWithValue("@OrganizTypeId", organizationalTypeId);
                    cmd.Parameters.AddWithValue("@Descript", description);
                    cmd.Parameters.AddWithValue("@appoinedWorkerId", appoinedWorkerId);
                    cmd.Parameters.AddWithValue("@addressId", addressId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        newDocId = dataReader.GetNullableString("retDocId");
                    }

                    if (orgs != null)
                    {
                        foreach (var org in orgs)
                        {
                            var orgQuery =
                                "call docs_pack.attach_org_to_doc(@workerId, @docId, @orgId, @inNumber, @inDate);";
                            using (var orgCmd = new MySqlCommand(orgQuery, conn))
                            {
                                orgCmd.Parameters.AddWithValue("@workerId", workerId);
                                orgCmd.Parameters.AddWithValue("@docId", newDocId);
                                orgCmd.Parameters.AddWithValue("@orgId", org.OrgId);
                                orgCmd.Parameters.AddWithValue("@inNumber", org.InNumber);
                                orgCmd.Parameters.AddWithValue("@inDate", org.InDate);
                                orgCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    return newDocId;
                }
            }
        }

        public static int AddOrgToDoc(int workerId, int docId, int orgId, string inNumber, DateTime? inDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var orgQuery =
                    "call docs_pack.attach_org_to_doc(@workerId, @docId, @orgId, @inNumber, @inDate);";
                using (var cmd = new MySqlCommand(orgQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@workerId", workerId);
                    cmd.Parameters.AddWithValue("@docId", docId);
                    cmd.Parameters.AddWithValue("@orgId", orgId);
                    cmd.Parameters.AddWithValue("@inNumber", inNumber);
                    cmd.Parameters.AddWithValue("@inDate", inDate);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        return dataReader.GetInt32("newId");
                    }
                }
            }
        }
        public static void AddAddressToDoc(int workerId, int docId, int addressId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var orgQuery =
                    "call docs_pack.add_address(@workerId, @docId, @addressId);";
                using (var cmd = new MySqlCommand(orgQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@workerId", workerId);
                    cmd.Parameters.AddWithValue("@docId", docId);
                    cmd.Parameters.AddWithValue("@addressId", addressId);
                    cmd.ExecuteNonQuery();

                }
            }
        }
        public static void UpdateOrgInDoc(int workerId, int docId, int orgId, string inNumber, DateTime? inDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var orgQuery =
                    "call docs_pack.update_org_in_doc(@workerId, @docId, @orgId, @inNumber, @inDate);";
                using (var cmd = new MySqlCommand(orgQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@workerId", workerId);
                    cmd.Parameters.AddWithValue("@docId", docId);
                    cmd.Parameters.AddWithValue("@orgId", orgId);
                    cmd.Parameters.AddWithValue("@inNumber", inNumber);
                    cmd.Parameters.AddWithValue("@inDate", inDate);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void DeleteOrgFromDoc(int workerId, int docId, int orgId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var orgQuery =
                    "call docs_pack.delete_org_from_doc(@workerId, @docId, @orgId);";
                using (var cmd = new MySqlCommand(orgQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@workerId", workerId);
                    cmd.Parameters.AddWithValue("@docId", docId);
                    cmd.Parameters.AddWithValue("@orgId", orgId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void DeleteAttachFromDoc(int workerId, int docId, int attachId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var orgQuery =
                    "call docs_pack.delete_attach(@workerId, @docId, @attachId);";
                using (var cmd = new MySqlCommand(orgQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@workerId", workerId);
                    cmd.Parameters.AddWithValue("@docId", docId);
                    cmd.Parameters.AddWithValue("@attachId", attachId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void DeleteDoc(int workerId, int docId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var orgQuery =
                    "call docs_pack.delete_doc(@workerId, @docId);";
                using (var cmd = new MySqlCommand(orgQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@workerId", workerId);
                    cmd.Parameters.AddWithValue("@docId", docId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void UpdateDoc(int workerId,int docId, int typeId, string topic, string docNumber, DateTime docDate,
             string inNumber, DateTime? inDate, int? orgId, int? organizationalTypeId, string description,
             int? appoinedWorkerId, int? addressId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    "call docs_pack.update_doc(@WorkerId,@DocId,@TypeId,@Topic, @DocNumber, @DocDate, @InNumber,@InDate,@OrgId,@OrganizTypeId,@Descript,@appoinedWorkerId,@addressId);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@DocId", docId);
                    cmd.Parameters.AddWithValue("@TypeId", typeId);
                    cmd.Parameters.AddWithValue("@Topic", topic);
                    cmd.Parameters.AddWithValue("@DocNumber", docNumber);
                    cmd.Parameters.AddWithValue("@DocDate", docDate);
                    cmd.Parameters.AddWithValue("@InNumber", inNumber);
                    cmd.Parameters.AddWithValue("@InDate", inDate);
                    cmd.Parameters.AddWithValue("@OrgId", orgId);
                    cmd.Parameters.AddWithValue("@OrganizTypeId", organizationalTypeId);
                    cmd.Parameters.AddWithValue("@Descript", description);
                    cmd.Parameters.AddWithValue("@appoinedWorkerId", appoinedWorkerId);
                    cmd.Parameters.AddWithValue("@addressId", addressId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static WorkerDto[] DocGetWorkers(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL docs_pack.get_workers(@WorkerId)";
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

        public static void AttachFileToDoc(int workerId, int docId, string fileName, string generatedFileName,
            string extension)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand(@"call docs_pack.attach_file(@WorkerId,@Id,@Name,@generatedName,@Extension);",
                        conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", docId);
                    cmd.Parameters.AddWithValue("@Name", fileName);
                    cmd.Parameters.AddWithValue("@generatedName", generatedFileName);
                    cmd.Parameters.AddWithValue("@Extension", extension);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<AttachmentToDocDto> GetAttachmentsToDocs(int workerId, int docId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"call docs_pack.get_attachments(@WorkerId,@DocId)", conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@DocId", docId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var attachments = new List<AttachmentToDocDto>();
                        while (dataReader.Read())
                        {
                            attachments.Add(new AttachmentToDocDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                FileName = dataReader.GetString("filename"),
                                Extension = dataReader.GetString("extension"),
                                CreateDate = dataReader.GetDateTime("insert_date"),
                                DocId = dataReader.GetInt32("doc_id"),
                                User = new UserDto()
                                {
                                    Id = dataReader.GetInt32("worker_id"),
                                    SurName = dataReader.GetNullableString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                }
                            });
                        }

                        dataReader.Close();
                        return attachments;
                    }
                }
            }
        }

        public static byte[] GenerateMasterStatisticsReport(int currentWorkerId, DateTime fromDate, DateTime toDate)
        {
            var items = GetMasterStatistics(currentWorkerId, fromDate, toDate);
            XElement root = new XElement("Records");
            foreach (var item in items)
            {
                root.AddFirst(
                    new XElement("Record",
                        new[]
                        {
                                        new XElement("Дата", item.Date),
                                        new XElement("Поступило", item.Incoming),
                                        new XElement("Выполнено_Всего", item.Done),
                                        new XElement("Выполнено_созданых_в_этот_день", item.DoneInThisDay),
                                        new XElement("Выполнено_созданых_в_другой_день", item.DoneInOtherDay),
                                        
                        }));
            }
            var saver = new MemoryStream();
            root.Save(saver);
            var buffer = saver.ToArray();
            saver.Close();
            return buffer;
        }

        public static MasterStatDto[] GetMasterStatistics(int currentWorkerId, DateTime fromDate, DateTime toDate)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            var result = new List<MasterStatDto>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.ReportGetNewRequest(@CurWorker,@FromDate,@ToDate)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            result.Add(new MasterStatDto
                            {
                                Date = dataReader.GetDateTime("rep_date"),
                                Incoming = dataReader.GetInt32("item_count"),
                            });
                        }

                        dataReader.Close();
                    }

                }
                sqlQuery = "CALL CallCenter.ReportGetDoneRequest(@CurWorker,@FromDate,@ToDate,false)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var date = dataReader.GetDateTime("rep_date");
                            var doneCount = dataReader.GetInt32("done_count");
                            var doneInThisDayCount = dataReader.GetInt32("in_this_day_done");
                            var item = result.FirstOrDefault(r => r.Date.Equals(date));
                            if(item == null)
                            { result.Add(new MasterStatDto
                            {
                                Date = date,
                                Done = doneCount,
                                DoneInOtherDay = doneCount - doneInThisDayCount,
                                DoneInThisDay = doneInThisDayCount,
                                Incoming = 0
                            });
                            }
                            else
                            {
                                item.Done = doneCount;
                                item.DoneInOtherDay = doneCount - doneInThisDayCount;
                                item.DoneInThisDay = doneInThisDayCount;
                            }
                        }

                        dataReader.Close();
                    }

                }


                return result.ToArray();
            }
        }
    }
}

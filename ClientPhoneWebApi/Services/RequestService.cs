using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClientPhoneWebApi.Dto;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace ClientPhoneWebApi.Services
{
    public class RequestService
    {
        private string _connectionString;
        private ILogger Logger { get; }

        public RequestService(ILogger<RequestService> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", "192.168.1.130",
                "asterisk", "mysqlasterisk", "CallCenter");
       }


        public WebUserDto WebLogin(string userName, string password)
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

        public ActiveChannelsDto[] GetActiveChannels(int userId)
        {
            var readedChannels = new List<ActiveChannelsDto>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    @"SELECT UniqueID,Channel,CallerIDNum,ChannelState,AnswerTime,CreateTime,TIMESTAMPDIFF(SECOND,CreateTime,sysdate()) waitSec,ivr_dtmf,
null as request_id,s.short_name, w.id,w.sur_name,w.first_name,w.patr_name, w.id worker_id,w.sur_name,w.first_name,w.patr_name
FROM asterisk.ActiveChannels a
left join CallCenter.ServiceCompanies s on a.ServiceComp = s.trunk_name
left join CallCenter.Workers w on w.phone = a.PhoneNum and not exists (select 1 from CallCenter.Workers w2 where w2.phone = w.phone and w2.id> w.id)
where Application = 'queue' and AppData like 'dispetchers%' and BridgeId is null order by UniqueID", conn))
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        readedChannels.Add(new ActiveChannelsDto
                        {
                            UniqueId = dataReader.GetNullableString("UniqueID"),
                            Channel = dataReader.GetNullableString("Channel"),
                            CallerIdNum = dataReader.GetNullableString("CallerIDNum"),
                            ChannelState = dataReader.GetNullableString("ChannelState"),
                            AnswerTime = dataReader.GetNullableDateTime("AnswerTime"),
                            ServiceCompany = dataReader.GetNullableString("short_name"),
                            WaitSecond = dataReader.GetInt32("waitSec"),
                            IvrDtmf = dataReader.GetNullableInt("ivr_dtmf"),
                            RequestId = dataReader.GetNullableInt("request_id"),
                            CreateTime = dataReader.GetNullableDateTime("CreateTime"),
                            Master = dataReader.GetNullableInt("worker_id") != null
                                ? new RequestUserDto()
                                {
                                    Id = dataReader.GetInt32("worker_id"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    SurName = dataReader.GetNullableString("sur_name"),
                                    PatrName = dataReader.GetNullableString("patr_name")
                                }
                                : null
                        });
                    }
                    dataReader.Close();
                }
            }

            return readedChannels.ToArray();
        }

        public NotAnsweredDto[] GetNotAnsweredCalls(int userId)
        {
            var callList = new List<NotAnsweredDto>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("CALL phone_client.get_not_answered(@userId)", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            callList.Add(new NotAnsweredDto
                            {
                                UniqueId = dataReader.GetNullableString("UniqueID"),
                                CallerId = dataReader.GetNullableString("CallerIDNum"),
                                CreateTime = dataReader.GetNullableDateTime("CreateTime"),
                                ServiceCompany = dataReader.GetNullableString("short_name"),
                                Prefix = dataReader.GetNullableString("prefix"),
                                IvrDtmf = dataReader.GetNullableInt("ivr_dtmf"),

                            });
                        }

                        dataReader.Close();
                    }
                }
            }

            return callList.ToArray();
        }

        public CallsListDto[] GetCallList(DateTime fromDate, DateTime toDate, string requestId, int? operatorId, int? serviceCompanyId, string phoneNumber)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                //           var sqlQuery = @"SELECT UniqueId,CallDirection,CallerIDNum,CreateTime,AnswerTime,EndTime,BridgedTime, 
                //MonitorFile,TalkTime,WaitingTime, userId, SurName, FirstName, PatrName, RequestId FROM asterisk.CallsHistory C";

                var sqlQuery = @"select C.UniqueID AS UniqueId,C.Direction AS CallDirection,
            (case when C.PhoneNum is not null then C.PhoneNum when(C.CallerIDNum in ('scvip500415','594555')) then C.Exten else C.CallerIDNum end) AS CallerIDNum,
C.CreateTime AS CreateTime,
C.AnswerTime AS AnswerTime,
C.EndTime AS EndTime,
C.BridgedTime AS BridgedTime,
C.MonitorFile AS MonitorFile,
timestampdiff(SECOND, C.BridgedTime, C.EndTime) AS TalkTime,
  (timestampdiff(SECOND, C.CreateTime, C.EndTime) - ifnull(timestampdiff(SECOND, C.BridgedTime, C.EndTime), 0)) AS WaitingTime,
       u.id AS userId,
u.SurName AS SurName,
u.FirstName AS FirstName,
u.PatrName AS PatrName,
group_concat(r.request_id order by r.request_id separator ', ') AS RequestId, sc.Name ServiceCompanyName,
null as redirect_phone,
null as ivr_menu,
null as ivr_dial
from
(((asterisk.ChannelHistory C left join asterisk.ChannelHistory C2 on(((C2.BridgeId = C.BridgeId) and(C.UniqueID <> C2.UniqueID))))
left join CallCenter.Users u on((u.id = ifnull(C.UserId, C2.UserId))))
left join CallCenter.RequestCalls r on((r.uniqueID = C.UniqueID)))
left join CallCenter.ServiceCompanies sc on sc.trunk_name = C.ServiceComp
where C.Direction is not null and C.UniqueId < '1552128123.322928'";
                //where(((C.Context = 'from-trunk') and(C.Exten = 's')) or((C.Context = 'localphones') and(C.CallerIDNum = 'scvip500415')))";

                if (!string.IsNullOrEmpty(requestId))
                {
                    sqlQuery += " and r.id = @RequestNum";
                }
                else
                {
                    sqlQuery += " and C.CreateTime between @fromdate and @todate";
                    if (operatorId.HasValue)
                    {
                        sqlQuery += " and u.id = @UserNum";

                    }
                    //if (serviceCompanyId.HasValue)
                    //{
                    //    sqlQuery += " and sc.id = @ServiceCompanyId";
                    //}
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        sqlQuery +=
                            " and (case when C.PhoneNum is not null then C.PhoneNum when(C.CallerIDNum in ('scvip500415','594555')) then C.Exten else C.CallerIDNum end) like @PhoneNumber";
                    }
                }
                sqlQuery += " and sc.id in (17,88)";

                sqlQuery += " group by C.UniqueID";

                sqlQuery += @"
union
select UniqueId,CallDirection,CallerIDNum,CreateTime,AnswerTime,
EndTime,BridgedTime,MonitorFile,TalkTime,WaitingTime,u.id AS userId,
u.SurName AS SurName,u.FirstName AS FirstName,u.PatrName AS PatrName,
RequestId, ServiceCompanyName,redirect_phone,ivr_menu,ivr_dial from
(
select C.UniqueID AS UniqueId, C.Direction AS CallDirection,
(case when C.PhoneNum is not null then C.PhoneNum when(C.CallerIDNum in ('scvip500415','594555')) then C.Exten else C.CallerIDNum end) AS CallerIDNum,
C.CreateTime AS CreateTime,
C.AnswerTime AS AnswerTime,
C.EndTime AS EndTime,
C.BridgedTime AS BridgedTime,
C.MonitorFile AS MonitorFile,
timestampdiff(SECOND, C.BridgedTime, C.EndTime) AS TalkTime,
  (timestampdiff(SECOND, C.CreateTime, C.EndTime) - ifnull(timestampdiff(SECOND, C.BridgedTime, C.EndTime), 0)) AS WaitingTime,
ifnull(C.UserId, max(C2.UserId)) userId,
(select group_concat(r.request_id order by r.request_id separator ', ') from CallCenter.RequestCalls r where r.uniqueID = C.UniqueID) AS RequestId,
sc.Name ServiceCompanyName,
group_concat(concat(C2.peer_number, ':', C2.ChannelState) order by C2.UniqueId desc separator ',') as redirect_phone,
C.ivr_menu,C.ivr_dial
FROM asterisk.ChannelHistory C
left join asterisk.ChannelBridges B on B.UniqueId = C.UniqueId
left join asterisk.ChannelHistory C2 on C2.BridgeId = B.BridgeId and C2.UniqueId <> C.UniqueId
left join CallCenter.ServiceCompanies sc on sc.trunk_name = C.ServiceComp
left join CallCenter.RequestCalls r on r.uniqueID = C.UniqueID
where C.UniqueId >= '1552128123.322928' and C.UniqueId = C.LinkedId and C.Direction is not null
and C.Context not in ('autoring','ringupcalls')
";
                if (!string.IsNullOrEmpty(requestId))
                {
                    sqlQuery += " and r.id = @RequestNum";
                }
                else
                {
                    sqlQuery += " and C.CreateTime between @fromdate and @todate";
                    if (operatorId.HasValue)
                    {
                        sqlQuery += " and (C.UserId = @UserNum or C2.UserId = @UserNum)";

                    }
                    //if (serviceCompanyId.HasValue)
                    //{
                    //    sqlQuery += " and sc.id = @ServiceCompanyId";

                    //}
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        sqlQuery +=
                            " and (case when C.PhoneNum is not null then C.PhoneNum when(C.CallerIDNum in ('scvip500415','594555')) then C.Exten else C.CallerIDNum end) like @PhoneNumber";
                    }
                }
                sqlQuery += " and sc.id in (17,88)";
                sqlQuery += @" group by C.UniqueId
) a
left join CallCenter.Users u on u.id = a.userId";


                using (
                var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    if (!string.IsNullOrEmpty(requestId))
                    {
                        cmd.Parameters.AddWithValue("@RequestNum", requestId.Trim());

                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@fromdate", fromDate);
                        cmd.Parameters.AddWithValue("@todate", toDate);
                        if (operatorId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@UserNum", operatorId);
                        }
                        //if (serviceCompanyId.HasValue)
                        //{
                        //    cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);
                        //}
                        if (!string.IsNullOrEmpty(phoneNumber))
                        {
                            cmd.Parameters.AddWithValue("@PhoneNumber", "%" + phoneNumber + "%");
                        }

                    }
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var callList = new List<CallsListDto>();
                        while (dataReader.Read())
                        {
                            var redirectPhone = dataReader.GetNullableString("redirect_phone");
                            if (!string.IsNullOrEmpty(redirectPhone))
                            {
                                var position = redirectPhone.IndexOf("/");
                                redirectPhone = redirectPhone.Substring(position + 1);
                                if (!string.IsNullOrEmpty(redirectPhone))
                                {
                                    var items = redirectPhone.Split(':');
                                    if (items[0].Length > 4)
                                    {
                                        redirectPhone = "";
                                    }
                                }
                            }
                            var ivrMenu = dataReader.GetNullableString("ivr_menu");
                            var ivrDial = dataReader.GetNullableString("ivr_dial");
                            var ivrUser = string.IsNullOrEmpty(ivrMenu) || ivrDial == "dispetcher"
                                ? (RequestUserDto)null
                                : new RequestUserDto
                                {
                                    Id = -1,
                                    SurName = "IVR Переадресация"
                                };
                            callList.Add(new CallsListDto
                            {
                                UniqueId = dataReader.GetNullableString("UniqueID"),
                                CallerId = dataReader.GetNullableString("CallerIDNum"),
                                Direction = dataReader.GetNullableString("CallDirection"),
                                AnswerTime = dataReader.GetNullableDateTime("AnswerTime"),
                                CreateTime = dataReader.GetNullableDateTime("CreateTime"),
                                BridgedTime = dataReader.GetNullableDateTime("BridgedTime"),
                                EndTime = dataReader.GetNullableDateTime("EndTime"),
                                TalkTime = dataReader.GetNullableInt("TalkTime"),
                                WaitingTime = dataReader.GetNullableInt("WaitingTime"),
                                MonitorFileName = dataReader.GetNullableString("MonitorFile"),
                                Requests = dataReader.GetNullableString("RequestId"),
                                RedirectPhone = redirectPhone,
                                ServiceCompany = dataReader.GetNullableString("ServiceCompanyName"),
                                User = dataReader.GetNullableInt("userId").HasValue
                                    ? new RequestUserDto
                                    {
                                        Id = dataReader.GetInt32("userId"),
                                        SurName = dataReader.GetNullableString("SurName"),
                                        FirstName = dataReader.GetNullableString("FirstName"),
                                        PatrName = dataReader.GetNullableString("PatrName")
                                    }
                                    : ivrUser
                            });
                        }
                        dataReader.Close();
                        return callList.ToArray();
                    }
                }
            }
        }

        public TransferIntoDto[] GetTransferList(int userId)
        {
            var query = "call phone_client.get_transfer_list(@userId);";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    var transferList = new List<TransferIntoDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            transferList.Add(new TransferIntoDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                SipNumber = dataReader.GetString("sip_number")
                            });
                        }

                        dataReader.Close();
                    }

                    return transferList.ToArray();
                }
            }
        }

        public DispatcherStatDto[] GetDispatcherStatistics(int userId)
        {
            var query = @"CALL phone_client.get_dispatcher_stat(@userId)";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    var dispatcherStatistics = new List<DispatcherStatDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            dispatcherStatistics.Add(new DispatcherStatDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                SurName = dataReader.GetString("SurName"),
                                IpAddress = dataReader.GetString("ip_addr"),
                                FirstName = dataReader.GetNullableString("FirstName"),
                                PatrName = dataReader.GetNullableString("PatrName"),
                                Version = dataReader.GetNullableString("version"),
                                OnLine = dataReader.GetNullableBoolean("on_line"),
                                //SpecialityId = dataReader.GetNullableInt("speciality_id"),
                                //SpecialityName = dataReader.GetNullableString("speciality_name"),
                                SipNumber = dataReader.GetNullableString("sip"),
                                PhoneNumber = dataReader.GetNullableString("PhoneNum"),
                                Direction = dataReader.GetNullableString("Direction"),
                                UniqueId = dataReader.GetNullableString("UniqueId"),
                                TalkTime = dataReader.GetNullableInt("TalkTime"),
                                WaitingTime = dataReader.GetNullableInt("WaitingTime"),
                                AliveTime = dataReader.GetDateTime("alive_time")
                            });
                        }

                        dataReader.Close();
                    }
                    return dispatcherStatistics.ToArray();
                }
            }
        }

        public RequestForListDto[] GetRequestByPhone(int userId, string phoneNumber)
        {
            var query = "call phone_client.get_requests_by_client_phone(@userId,@clientPhone);";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@clientPhone", phoneNumber);
                    var requests = new List<RequestForListDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            //var recordUniqueId = dataReader.GetNullableString("recordId");
                            requests.Add(new RequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                //HasAttachment = dataReader.GetBoolean("has_attach"),
                                //IsBadWork = dataReader.GetBoolean("bad_work"),
                                //IsRetry = dataReader.GetBoolean("retry"),
                                //Warranty = dataReader.GetInt32("garanty"),
                                //Immediate = dataReader.GetBoolean("is_immediate"),
                                //HasRecord = !string.IsNullOrEmpty(recordUniqueId),
                                //RecordUniqueId = recordUniqueId,
                                StreetPrefix = dataReader.GetString("prefix_name"),
                                //RegionName = dataReader.GetNullableString("region_name"),
                                StreetName = dataReader.GetString("street_name"),
                                AddressType = dataReader.GetString("address_type"),
                                Flat = dataReader.GetString("flat"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                ServiceCompany = dataReader.GetNullableString("service_company_name"),

                                //Master = dataReader.GetNullableInt("worker_id") != null ? new RequestUserDto
                                //{
                                //    Id = dataReader.GetInt32("worker_id"),
                                //    SurName = dataReader.GetNullableString("sur_name"),
                                //    FirstName = dataReader.GetNullableString("first_name"),
                                //    PatrName = dataReader.GetNullableString("patr_name"),
                                //} : null,
                                //Executer = dataReader.GetNullableInt("executer_id") != null ? new RequestUserDto
                                //{
                                //    Id = dataReader.GetInt32("executer_id"),
                                //    SurName = dataReader.GetNullableString("exec_sur_name"),
                                //    FirstName = dataReader.GetNullableString("exec_first_name"),
                                //    PatrName = dataReader.GetNullableString("exec_patr_name"),
                                //} : null,
                                //CreateUser = new RequestUserDto
                                //{
                                //    Id = dataReader.GetInt32("create_user_id"),
                                //    SurName = dataReader.GetNullableString("surname"),
                                //    FirstName = dataReader.GetNullableString("firstname"),
                                //    PatrName = dataReader.GetNullableString("patrname"),
                                //},
                                //ExecuteTime = dataReader.GetNullableDateTime("execute_date"),
                                //TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                                //ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                                //Rating = dataReader.GetNullableString("Rating"),
                                //RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                //Status = dataReader.GetNullableString("Req_Status"),
                                //SpendTime = dataReader.GetNullableString("spend_time"),
                                //FromTime = dataReader.GetNullableDateTime("from_time"),
                                //ToTime = dataReader.GetNullableDateTime("to_time"),
                                //AlertTime = dataReader.GetNullableDateTime("alert_time"),
                                //MainFio = dataReader.GetNullableString("clinet_fio"),
                                //LastNote = dataReader.GetNullableString("last_note"),
                            });
                        }
                        dataReader.Close();
                    }
                    return requests.ToArray();
                }
            }
        }
        public void SendAlive(int userId, string sipUser)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"call CallCenter.SendAliveAndSip(@UserId,@Sip)", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Sip", sipUser);
                    cmd.ExecuteNonQuery();
                }
            }
        }

       public void IncreaseRingCount(int userId, string callerId)
       {
           using (var conn = new MySqlConnection(_connectionString))
           {
               conn.Open();
                var query = "update asterisk.NotAnsweredQueue set call_count = call_count + 1 where CallerIDNum  = @CallerId;";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CallerId", callerId);
                    cmd.ExecuteNonQuery();
                }
           }

       }
       public void AddCallToRequest(int userId, int requestId, string callUniqueId)
       {
           if (requestId <= 0 || string.IsNullOrEmpty(callUniqueId))
               return;
           using (var conn = new MySqlConnection(_connectionString))
           {
               conn.Open();
               using (var cmd =
                   new MySqlCommand(
                       "insert into CallCenter.RequestCalls(request_id,uniqueID) values(@Request, @UniqueId) ON DUPLICATE KEY UPDATE uniqueID = @UniqueId",
                       conn))
               {
                   cmd.Parameters.AddWithValue("@Request", requestId);
                   cmd.Parameters.AddWithValue("@UniqueId", callUniqueId);
                   cmd.ExecuteNonQuery();
               }
           }
       }
       public byte[] GetRecordById(int userId, string path)
       {
           using (var conn = new MySqlConnection(_connectionString))
           {
                           var fileName = path;
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

        public void DeleteCallFromNotAnsweredListByTryCount(int userId, string callerId)
       {
           using (var conn = new MySqlConnection(_connectionString))
           {
               conn.Open();
               var query = "delete from asterisk.NotAnsweredQueue where CallerIDNum  = @CallerId and call_count >= 2;";
               using (var cmd = new MySqlCommand(query, conn))
               {
                   cmd.Parameters.AddWithValue("@CallerId", callerId);
                   cmd.ExecuteNonQuery();
               }
           }
       }
        public void Logout(int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"call CallCenter.LogoutUser(@UserId)", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public SipDto GetSipInfoByIp(string localIp)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand($"CALL CallCenter.GetSIPInfoByIp('{localIp}')", conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            return new SipDto
                            {
                                SipUser = dataReader.GetNullableString("SIPName"),
                                SipSecret = dataReader.GetNullableString("Secret")
                            };
                        }

                        dataReader.Close();
                    }
                }
            }
            return null;
        }

        public UserDto[] GetDispatchers(int companyId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"call phone_client.get_dispatchers()", conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var users = new List<UserDto>();
                        while (dataReader.Read())
                        {
                            users.Add(new UserDto()
                            {
                                Id = dataReader.GetInt32("Id"),
                                Login = dataReader.GetNullableString("Login")
                            });
                        }
                        dataReader.Close();
                        return users.ToArray();
                    }
                }
            }
        }
        public RequestUserDto[] GetFilterDispatchers(int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("call phone_client.get_filtered_dispatcher(@userId)", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var usersList = new List<RequestUserDto>();
                        while (dataReader.Read())
                        {
                            usersList.Add(new RequestUserDto
                            {
                                Id = dataReader.GetInt32("id"),
                                SurName = dataReader.GetNullableString("SurName"),
                                FirstName = dataReader.GetNullableString("FirstName"),
                                PatrName = dataReader.GetNullableString("PatrName")
                            });
                        }
                        dataReader.Close();
                        return usersList.ToArray();
                    }
                }
            }
        }

        public ServiceCompanyDto[] GetFilterServiceCompanies(int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("call phone_client.get_filtered_companies(@userId)", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
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
                        return companies.OrderBy(i => i.Name).ToArray();
                }
            }
        }

        public ServiceCompanyDto[] GetServiceCompaniesForCall(int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("call phone_client.get_companies_for_call(@userId)", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                        var companies = new List<ServiceCompanyDto>();
                        using (var dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                companies.Add(new ServiceCompanyDto
                                {
                                    Id = dataReader.GetInt32("id"),
                                    Name = dataReader.GetNullableString("name"),
                                    Prefix = dataReader.GetNullableString("prefix"),
                                    Phone = dataReader.GetNullableString("phone"),
                                    ShortName = dataReader.GetNullableString("short_name")
                                });
                            }
                            dataReader.Close();
                        }
                        return companies.OrderBy(i => i.Name).ToArray();
                }
            }
        }
        public DateTime GetCurrentDate()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("select sysdate() curdate", conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        return dataReader.GetDateTime("curdate");
                    }
                }
            }
        }
        public string GetUniqueIdByCallId(int userId,string callId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("call phone_client.get_uniqueid_by_callid(@userId,@callId)", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@callId", callId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if(dataReader.Read())
                            return dataReader.GetNullableString("uniqueIdStr");
                    }

                }

                return null;
            }
        }
        public UserDto Login(string login, string password, string sipUser)
        {
            UserDto currentUser = null;
            var userId = 0;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(
                            $"Call CallCenter.LoginUser('{login}','{password}','{sipUser}')",
                            conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            userId = dataReader.GetInt32("Id");
                            currentUser = new UserDto()
                            {
                                Id = dataReader.GetInt32("Id"),
                                Login = dataReader.GetNullableString("Login"),
                                FirstName = dataReader.GetNullableString("FirstName"),
                                SurName = dataReader.GetNullableString("SurName"),
                                PatrName = dataReader.GetNullableString("PatrName")
                            };

                        }

                        dataReader.Close();
                    }
                }

                if (userId > 0)
                {
                    using (var cmd = new MySqlCommand($@"SELECT r.* FROM CallCenter.UserRoles u
                            join CallCenter.Roles r on r.id = u.role_id
                            where u.user_id = {
                                userId}", conn)) 
                    {
                        using (var dataReader = cmd.ExecuteReader())
                        {
                            var userRoles = new List<RoleDto>();
                            while (dataReader.Read())
                            {
                                userRoles.Add(new RoleDto
                                {
                                    Id = dataReader.GetInt32("Id"),
                                    Name = dataReader.GetNullableString("Name")
                                });
                            }

                            dataReader.Close();
                            currentUser.Roles = userRoles.ToArray();
                        }
                    }
                }
            }
            return currentUser;
        }

        private void RefreshNotAnsweredCalls()
        {
            var callList = new List<NotAnsweredDto>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("CALL CallCenter.GetNotAnswered()", conn))
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        callList.Add(new NotAnsweredDto
                        {
                            UniqueId = dataReader.GetNullableString("UniqueID"),
                            CallerId = dataReader.GetNullableString("CallerIDNum"),
                            CreateTime = dataReader.GetNullableDateTime("CreateTime"),
                            ServiceCompany = dataReader.GetNullableString("short_name"),
                            Prefix = dataReader.GetNullableString("prefix"),
                            IvrDtmf = dataReader.GetNullableInt("ivr_dtmf"),

                        });
                    }
                    dataReader.Close();
                }
            }
        }



    }
}
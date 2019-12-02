using System;
using System.Collections.Generic;
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

        public ActiveChannelsDto[] GetActiveChannels()
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
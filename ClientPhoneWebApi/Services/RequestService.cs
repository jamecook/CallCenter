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
                using (var cmd = new MySqlCommand(@"SELECT u.id,u.Login FROM CallCenter.Users u
 join Workers w on w.id = u.worker_id and w.enabled = 1
 where u.Enabled = 1 and u.ShowInForm = 1 and w.service_company_id = @CompanyId order by u.Login", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
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
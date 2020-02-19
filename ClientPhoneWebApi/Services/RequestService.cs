using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClientPhoneWebApi.Dto;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace ClientPhoneWebApi.Services
{
    public class RequestService
    {
        private string _connectionString;
        private ILogger Logger { get; }

        public RequestService(ILogger<RequestService> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", "192.168.0.130",
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

        public RequestForListDto[] GetRequestList(int userId, string requestId, bool filterByCreateDate,
            DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int[] streetsId,
            int? houseId, int? addressId, int[] parentServicesId, int? serviceId, int[] statusesId, int[] mastersId,
            int[] executersId, int[] serviceCompaniesId, int[] usersId, int[] ratingsId, int? payment, bool onlyBadWork,
            bool onlyRetry, string clientPhone, bool onlyGaranty, bool onlyImmediate, bool onlyByClient)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            var sqlQuery =
                @"SELECT R.id,case when count(ra.id)=0 then false else true end has_attach,R.create_time,sp.name as prefix_name,s.name as street_name,h.building,h.corps,at.Name address_type, a.flat,
    R.worker_id, w.sur_name,w.first_name,w.patr_name,case when create_user_id = 0 then cw.id else create_user_id end create_user_id,
    case when create_user_id = 0 then cw.sur_name else u.surname end surname,
    case when create_user_id = 0 then cw.first_name else u.firstname end firstname,
    case when create_user_id = 0 then cw.patr_name else u.patrname end patrname, R.is_immediate,
    R.execute_date,p.Name Period_Name, R.description,rt.name service_name, rt2.name parent_name, group_concat(distinct cp.Number order by rc.IsMain desc separator ', ') client_phones,
    (SELECT name from CallCenter.RequestContacts rc2
    join CallCenter.ClientPhones cp2 on cp2.id = rc2.ClientPhone_id
    where rc2.request_id = R.id
    order by IsMain desc limit 1) clinet_fio,    
    rating.Name Rating,
    rtype.Description RatingDesc,
    RS.Description Req_Status,R.to_time, R.from_time, TIMEDIFF(R.to_time,R.from_time) spend_time,R.bad_work,R.garanty,R.retry,
    min(rcalls.uniqueID) recordId, R.alert_time,
    (SELECT note FROM CallCenter.RequestNoteHistory rnh where rnh.request_id = R.id
    order by operation_date desc limit 1) last_note,
    R.executer_id,execw.sur_name exec_sur_name, execw.first_name exec_first_name, execw.patr_name exec_patr_name,R.term_of_execution,
    sc.name service_company_name,
    reg.Name region_name,
    group_concat(distinct concat(vw.sur_name,' ',substr(ifnull(vw.first_name,''),1,1),'.',substr(ifnull(vw.patr_name,''),1,1)) order by vr.id desc separator '; ') viewed_by
    FROM CallCenter.Requests R
    join CallCenter.RequestState RS on RS.id = R.state_id
    join CallCenter.Addresses a on a.id = R.address_id
    join CallCenter.AddressesTypes at on at.id = a.type_id
    join CallCenter.Houses h on h.id = house_id
    left join CallCenter.CityRegions reg on reg.id = h.region_id
    join CallCenter.Streets s on s.id = street_id
    join CallCenter.StreetPrefixes sp on sp.id = s.prefix_id
    join CallCenter.RequestTypes rt on rt.id = R.type_id
    join CallCenter.RequestTypes rt2 on rt2.id = rt.parrent_id
    left join (select request_id,max(id) max_id from CallCenter.RequestRating rr group by request_id) rr_max on rr_max.request_id = R.Id
    left join CallCenter.RequestRating rtype on rtype.id = rr_max.max_id
    left join CallCenter.RatingTypes rating on rtype.rating_id = rating.id
    left join CallCenter.RequestAttachments ra on ra.request_id = R.id
    left join CallCenter.Workers w on w.id = R.worker_id
    left join CallCenter.Workers execw on execw.id = R.executer_id
    left join CallCenter.RequestContacts rc on rc.request_id = R.id
    left join CallCenter.ClientPhones cp on cp.id = rc.clientPhone_id
    left join CallCenter.ServiceCompanies sc on sc.id= R.service_company_id
    join CallCenter.Users u on u.id = create_user_id
    left join CallCenter.PeriodTimes p on p.id = R.period_time_id
    left join CallCenter.RequestCalls rcalls on rcalls.request_id = R.id
    left join CallCenter.Workers cw on cw.id = R.create_worker_id
    left join CallCenter.ViewRequests vr on vr.request_id = R.id
    left join CallCenter.Workers vw on vw.id = vr.worker_id";
            if (string.IsNullOrEmpty(requestId))
            {
                if (filterByCreateDate)
                {
                    sqlQuery += " where R.create_time between @FromDate and @ToDate";
                }
                else
                {
                    findFromDate = executeFromDate.Date;
                    findToDate = executeToDate.Date.AddDays(1).AddSeconds(-1);

                    sqlQuery += " where R.execute_date between @FromDate and @ToDate";
                }

                if (streetsId != null && streetsId.Length > 0)
                    sqlQuery +=
                        $" and s.id in ({streetsId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";
                if (houseId.HasValue)
                    sqlQuery += $" and h.id = {houseId.Value}";
                if (addressId.HasValue)
                    sqlQuery += $" and a.id = {addressId.Value}";
                if (serviceId.HasValue)
                    sqlQuery += $" and rt.id = {serviceId.Value}";

                if (parentServicesId != null && parentServicesId.Length > 0)
                    sqlQuery +=
                        $" and rt2.id in ({parentServicesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";

                if (statusesId != null && statusesId.Length > 0)
                    sqlQuery +=
                        $" and R.state_id in ({statusesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";

                if (mastersId != null && mastersId.Length > 0)
                    sqlQuery +=
                        $" and w.id in ({mastersId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";

                if (executersId != null && executersId.Length > 0)
                    sqlQuery +=
                        $" and R.executer_id in ({executersId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";

                if (serviceCompaniesId != null && serviceCompaniesId.Length > 0)
                    sqlQuery +=
                        $" and R.service_company_id in ({serviceCompaniesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";

                if (usersId != null && usersId.Length > 0)
                    sqlQuery +=
                        $" and R.create_user_id in ({usersId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";

                if (ratingsId != null && ratingsId.Length > 0)
                    sqlQuery +=
                        $" and rtype.rating_id in ({ratingsId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";

                if (payment.HasValue)
                    sqlQuery += $" and R.is_chargeable = {payment.Value}";
                if (onlyBadWork)
                    sqlQuery += " and R.bad_work = 1";
                if (onlyRetry)
                    sqlQuery += " and R.retry = 1";
                if (onlyGaranty)
                    sqlQuery += " and R.garanty = 1";
                if (onlyImmediate)
                    sqlQuery += " and R.is_immediate = 1";
                if (onlyByClient)
                    sqlQuery += " and R.create_client_id is not null";
                if (!string.IsNullOrEmpty(clientPhone))
                    sqlQuery += $" and cp.Number like '%{clientPhone}'";
            }
            else
            {
                sqlQuery += " where R.id = @RequestId";
            }

            sqlQuery += " and R.service_company_id in (17,88,48,142)";
            sqlQuery += " group by R.id order by id desc";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand(sqlQuery, conn))
                {
                    if (string.IsNullOrEmpty(requestId))
                    {

                        cmd.Parameters.AddWithValue("@FromDate", findFromDate);
                        cmd.Parameters.AddWithValue("@ToDate", findToDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId.Trim());
                    }

                    var requests = new List<RequestForListDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var recordUniqueId = dataReader.GetNullableString("recordId");
                            requests.Add(new RequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                HasAttachment = dataReader.GetBoolean("has_attach"),
                                IsBadWork = dataReader.GetBoolean("bad_work"),
                                IsRetry = dataReader.GetBoolean("retry"),
                                Warranty = dataReader.GetInt32("garanty"),
                                Immediate = dataReader.GetBoolean("is_immediate"),
                                HasRecord = !string.IsNullOrEmpty(recordUniqueId),
                                RecordUniqueId = recordUniqueId,
                                StreetPrefix = dataReader.GetString("prefix_name"),
                                RegionName = dataReader.GetNullableString("region_name"),
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
                                ViewedBy = dataReader.GetNullableString("viewed_by"),
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
                                TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                                ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                                Rating = dataReader.GetNullableString("Rating"),
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                Status = dataReader.GetNullableString("Req_Status"),
                                SpendTime = dataReader.GetNullableString("spend_time"),
                                FromTime = dataReader.GetNullableDateTime("from_time"),
                                ToTime = dataReader.GetNullableDateTime("to_time"),
                                AlertTime = dataReader.GetNullableDateTime("alert_time"),
                                MainFio = dataReader.GetNullableString("clinet_fio"),
                                LastNote = dataReader.GetNullableString("last_note"),
                                ServiceCompany = dataReader.GetNullableString("service_company_name"),
                            });
                        }

                        dataReader.Close();
                    }

                    return requests.ToArray();
                }
            }
        }

        public WorkerDto[] GetMasters(int userId, int? serviceCompanyId, bool showOnlyExecutors = true)
        {
            var sqlQuery =
                @"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,sp.name speciality_name,w.can_assign,w.parent_worker_id,send_sms,is_master,is_executer,is_dispetcher,w.send_notification FROM CallCenter.Workers w
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id
    left join CallCenter.Speciality sp on sp.id = w.speciality_id
    where w.enabled = 1 and w.is_master = 1 and service_company_id in (17,88,48,142) ";
            if (showOnlyExecutors)
                sqlQuery += " and can_assign = true";
            sqlQuery += serviceCompanyId.HasValue ? " and service_company_id = " + serviceCompanyId : "";
            sqlQuery += " order by sur_name,first_name,patr_name";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    var workers = new List<WorkerDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            workers.Add(new WorkerDto
                            {
                                Id = dataReader.GetInt32("id"),
                                ServiceCompanyId = dataReader.GetNullableInt("service_id"),
                                ServiceCompanyName = dataReader.GetNullableString("service_name"),
                                SurName = dataReader.GetString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                SpecialityId = dataReader.GetNullableInt("speciality_id"),
                                SpecialityName = dataReader.GetNullableString("speciality_name"),
                                Phone = dataReader.GetNullableString("phone"),
                                CanAssign = dataReader.GetBoolean("can_assign"),
                                SendSms = dataReader.GetBoolean("send_sms"),
                                AppNotification = dataReader.GetBoolean("send_notification"),
                                IsMaster = dataReader.GetBoolean("is_master"),
                                IsExecuter = dataReader.GetBoolean("is_executer"),
                                IsDispetcher = dataReader.GetBoolean("is_dispetcher"),
                                ParentWorkerId = dataReader.GetNullableInt("parent_worker_id"),
                            });
                        }

                        dataReader.Close();
                    }

                    return workers.ToArray();
                }
            }
        }

        public WorkerDto[] GetExecutors(int userId, int? serviceCompanyId, bool showOnlyExecutors = true)
        {
            var sqlQuery =
                @"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,sp.name speciality_name,w.can_assign,w.parent_worker_id,w.send_sms,is_master,is_executer,is_dispetcher,send_notification FROM CallCenter.Workers w
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id
    left join CallCenter.Speciality sp on sp.id = w.speciality_id
    where w.enabled = 1 and w.is_executer = true and service_company_id in (17,88,48,142) ";
            if (showOnlyExecutors)
                sqlQuery += " and can_assign = true";
            sqlQuery += serviceCompanyId.HasValue ? " and service_company_id = " + serviceCompanyId : "";
            sqlQuery += " order by sur_name,first_name,patr_name";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    var workers = new List<WorkerDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            workers.Add(new WorkerDto
                            {
                                Id = dataReader.GetInt32("id"),
                                ServiceCompanyId = dataReader.GetNullableInt("service_id"),
                                ServiceCompanyName = dataReader.GetNullableString("service_name"),
                                SurName = dataReader.GetString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                SpecialityId = dataReader.GetNullableInt("speciality_id"),
                                SpecialityName = dataReader.GetNullableString("speciality_name"),
                                Phone = dataReader.GetNullableString("phone"),
                                CanAssign = dataReader.GetBoolean("can_assign"),
                                SendSms = dataReader.GetBoolean("send_sms"),
                                AppNotification = dataReader.GetBoolean("send_notification"),
                                IsMaster = dataReader.GetBoolean("is_master"),
                                IsExecuter = dataReader.GetBoolean("is_executer"),
                                IsDispetcher = dataReader.GetBoolean("is_dispetcher"),
                                ParentWorkerId = dataReader.GetNullableInt("parent_worker_id"),
                            });
                        }

                        dataReader.Close();
                    }

                    return workers.ToArray();
                }
            }
        }

        public ServiceDto GetServiceById(int userId, int serviceId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                ServiceDto service = null;
                var query = "SELECT id,name,can_send_sms,immediate FROM CallCenter.RequestTypes where id = @ID";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", serviceId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            service = new ServiceDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                CanSendSms = dataReader.GetBoolean("can_send_sms"),
                                Immediate = dataReader.GetBoolean("immediate")
                            };
                        }

                        dataReader.Close();
                    }
                }

                return service;
            }
        }

        public AddressTypeDto[] GetAddressTypes(int userId)
        {
            var query = "SELECT id,Name FROM CallCenter.AddressesTypes A order by OrderNum";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var types = new List<AddressTypeDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            types.Add(new AddressTypeDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name")
                            });
                        }

                        dataReader.Close();
                    }

                    return types.ToArray();
                }
            }
        }

        public EquipmentDto[] GetEquipments(int userId)
        {
            var query =
                "SELECT e.id,t.name type_name,e.name FROM CallCenter.Equipments e join CallCenter.EquipmentTypes t on t.id = e.type_id order by t.name,e.name";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var equipment = new List<EquipmentDto>();
                    equipment.Add(new EquipmentDto() {Id = null, Name = "Нет"});
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            equipment.Add(new EquipmentDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = $"{dataReader.GetString("type_name")} - {dataReader.GetString("name")}",
                            });
                        }

                        dataReader.Close();
                    }

                    return equipment.ToArray();
                }
            }
        }

        public PeriodDto[] GetPeriods(int userId)
        {
            var query = "SELECT id,Name,SetTime,OrderNum FROM CallCenter.PeriodTimes P";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var periods = new List<PeriodDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            periods.Add(new PeriodDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                SetTime = dataReader.GetDateTime("SetTime"),
                                OrderNum = dataReader.GetInt32("OrderNum")
                            });
                        }

                        dataReader.Close();
                    }

                    return periods.OrderBy(i => i.OrderNum).ToArray();
                }
            }
        }

        public WorkerDto[] GetWorkersByHouseAndService(int userId, int houseId, int parentServiceTypeId,
            bool showMasters = true)
        {
            var query =
                $@"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,sp.name speciality_name,w.can_assign,w.parent_worker_id,w.send_sms, w.send_notification
    FROM CallCenter.WorkerHouseAndType wh
    join CallCenter.Workers w on wh.worker_id = w.id
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id
    left join CallCenter.Speciality sp on sp.id = w.speciality_id
    where w.enabled = 1 and wh.master_weigth is not null and wh.house_id = {houseId}
    and (wh.type_id is null or wh.type_id = {parentServiceTypeId})";
            if (showMasters)
                query += "and w.is_master = 1";
            else
                query += "and w.is_executer = 1";
            query +=
                @" group by s.id,s.name ,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,sp.name,w.can_assign,w.parent_worker_id
    order by wh.master_weigth desc;";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var workers = new List<WorkerDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            workers.Add(new WorkerDto
                            {
                                Id = dataReader.GetInt32("id"),
                                ServiceCompanyId = dataReader.GetNullableInt("service_id"),
                                ServiceCompanyName = dataReader.GetNullableString("service_name"),
                                SurName = dataReader.GetString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                SpecialityId = dataReader.GetNullableInt("speciality_id"),
                                SpecialityName = dataReader.GetNullableString("speciality_name"),
                                Phone = dataReader.GetNullableString("phone"),
                                CanAssign = dataReader.GetBoolean("can_assign"),
                                SendSms = dataReader.GetBoolean("send_sms"),
                                AppNotification = dataReader.GetBoolean("send_notification"),
                                ParentWorkerId = dataReader.GetNullableInt("parent_worker_id"),
                            });
                        }

                        dataReader.Close();
                    }

                    return workers.ToArray();
                }
            }
        }

        public ClientAddressInfoDto GetLastAddressByClientPhone(int userId, string phone)
        {
            ClientAddressInfoDto result = null;
            var query =
                @"SELECT cp.id,h.street_id,h.building,h.corps,a.flat,name,email,addition FROM CallCenter.ClientPhones cp
            join CallCenter.RequestContacts rc on rc.ClientPhone_id = cp.id
            join CallCenter.Requests r on r.id = rc.request_id
            join CallCenter.Addresses a on a.id = r.address_id
            join CallCenter.Houses h on h.id = a.house_id
            where cp.Number = @phone
            order by r.id desc limit 1";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@phone", phone);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            result = new ClientAddressInfoDto
                            {
                                ClientPhoneId = dataReader.GetInt32("id"),
                                StreetId = dataReader.GetInt32("street_id"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                Flat = dataReader.GetString("flat"),

                                Name = dataReader.GetNullableString("name"),
                                Email = dataReader.GetNullableString("email"),
                                AdditionInfo = dataReader.GetNullableString("addition"),
                            };
                        }

                        dataReader.Close();
                    }

                    return result;
                }
            }
        }

        public CityDto[] GetCities(int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"select id,name from CallCenter.Cities where enabled = 1", conn)
                )
                {
                    var cities = new List<CityDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            cities.Add(new CityDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name")
                            });
                        }

                        dataReader.Close();
                    }

                    return cities.ToArray();
                }
            }
        }

        public string GetActiveCallUniqueIdByCallId(int userId, string callId)
        {
            string retVal = null;
            if (string.IsNullOrEmpty(callId))
                return null;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var query =
                    $@"SELECT case when (A.UniqueID <= ifnull(A2.UniqueID,A.UniqueID)) then A.UniqueID else A2.UniqueID end uniqueId FROM asterisk.ActiveChannels A
 left join asterisk.ActiveChannels A2 on A2.BridgeId = A.BridgeId and A2.UniqueID <> A.UniqueID
 where A.call_id like '{callId}%'";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            retVal = dataReader.GetNullableString("uniqueId");
                        }

                        dataReader.Close();
                    }
                }

                if (!string.IsNullOrEmpty(retVal))
                    return retVal;
                query =
                    $@"SELECT case when (A.UniqueID <= ifnull(A2.UniqueID,A.UniqueID)) then A.UniqueID else A2.UniqueID end uniqueId FROM asterisk.ChannelHistory A
 left join asterisk.ChannelHistory A2 on A2.BridgeId = A.BridgeId and A2.UniqueID <> A.UniqueID
 where A.call_id like '{callId}%'";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            retVal = dataReader.GetNullableString("uniqueId");
                        }

                        dataReader.Close();
                    }
                }

                return retVal;
            }
        }

        public HouseDto GetHouseById(int userId, int houseId)
        {
            HouseDto house = null;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    @"SELECT h.id, h.street_id, h.building, h.corps, h.service_company_id, h.entrance_count, h.flat_count, h.floor_count, h.service_company_id, s.Name service_company_name,commissioning_date,have_parking,elevator_count,region_id, r.name region_name FROM CallCenter.Houses h
 left join CallCenter.ServiceCompanies s on s.id = h.service_company_id
 left join CallCenter.CityRegions r on r.id = h.region_id where h.enabled = 1 and h.id = @HouseId", conn))
                {
                    cmd.Parameters.AddWithValue("@HouseId", houseId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            house = new HouseDto()
                            {
                                Building = dataReader.GetString("building"),
                                StreetId = dataReader.GetInt32("street_id"),
                                Corpus = dataReader.GetNullableString("corps"),
                                RegionId = dataReader.GetNullableInt("region_id"),
                                RegionName = dataReader.GetNullableString("region_name"),
                                ServiceCompanyId = dataReader.GetNullableInt("service_company_id"),
                                ServiceCompanyName = dataReader.GetNullableString("service_company_name"),
                                EntranceCount = dataReader.GetNullableInt("entrance_count"),
                                FlatCount = dataReader.GetNullableInt("flat_count"),
                                FloorCount = dataReader.GetNullableInt("floor_count"),
                                ElevatorCount = dataReader.GetNullableInt("elevator_count"),
                                HaveParking = dataReader.GetBoolean("have_parking"),
                                CommissioningDate = dataReader.GetNullableDateTime("commissioning_date"),
                            };
                        }

                        dataReader.Close();
                    }
                }

            }

            return house;
        }

        public int? GetServiceCompanyIdByHouseId(int userId, int houseId)
        {
            int? retVal = null;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand(@"SELECT service_company_id FROM CallCenter.Houses H where id = @HouseId",
                        conn))
                {
                    cmd.Parameters.AddWithValue("@HouseId", houseId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            retVal = dataReader.GetNullableInt("service_company_id");
                        }

                        dataReader.Close();
                    }

                    return retVal;
                }
            }
        }

        public int AlertCountByHouseId(int userId, int houseId)
        {
            int result = 0;
            var sqlQuery = @"SELECT count(1) alert_count FROM CallCenter.Alerts a
 where a.house_id = @HouseId and (end_date is null or a.end_date > sysdate())";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@HouseId", houseId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            result = dataReader.GetInt32("alert_count");
                        }

                        dataReader.Close();
                        return result;
                    }
                }
            }
        }

        public ScheduleTaskDto GetScheduleTaskByRequestId(int userId, int requestId)
        {
            ScheduleTaskDto result = null;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    @"SELECT s.id,w.id worker_id,w.sur_name,w.first_name,w.patr_name,s.request_id,s.from_date,s.to_date,s.event_description FROM CallCenter.ScheduleTasks s
join CallCenter.Workers w on s.worker_id = w.id
where s.request_id = @RequestId and deleted = 0;";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RequestId", requestId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            result = new ScheduleTaskDto
                            {
                                Id = dataReader.GetInt32("id"),
                                RequestId = dataReader.GetNullableInt("request_id"),
                                Worker = new WorkerDto()
                                {
                                    Id = dataReader.GetInt32("worker_id"),
                                    SurName = dataReader.GetString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                },
                                FromDate = dataReader.GetDateTime("from_date"),
                                ToDate = dataReader.GetDateTime("to_date"),
                                EventDescription = dataReader.GetNullableString("event_description")
                            };
                        }

                        dataReader.Close();
                    }

                    return result;
                }
            }
        }

        public void RequestChangeAddress(int userId, int requestId, int addressId)
        {
            if (requestId <= 0 || addressId <= 0)
                return;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"call CallCenter.ChangeAddress(@RequestId,@Address)", conn))
                {
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@Address", addressId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public AlertDto[] GetAlerts(int userId, DateTime fromDate, DateTime toDate, int? houseId, bool onlyActive)
        {
            var sqlQuery =
                @"SELECT a.id alert_id,s.id street_id, s.name street_name,h.id house_id, h.building,h.corps,a.start_date,a.end_date,a.description,
 at.id alert_type_id, at.name alert_type_name, a.alert_service_type_id,ast.name alert_service_type_name,
 u.id user_id,u.SurName,u.FirstName,u.PatrName,a.create_date
 FROM CallCenter.Alerts a
 join CallCenter.Houses h on h.id = a.house_id
 join CallCenter.Streets s on s.id = h.street_id
 join CallCenter.AlertType at on at.id = a.alert_type_id
 join CallCenter.AlertServiceType ast on ast.id = a.alert_service_type_id
 join CallCenter.Users u on u.id = a.create_user_id
 where 1 = 1 and h.service_company_id in (17,48,88,142)";
            if (onlyActive)
                sqlQuery += " and (end_date is null or a.end_date > sysdate())";
            else
            {
                sqlQuery += @" and (end_date is null or a.end_date between @FromDate and @ToDate)
 and (start_date between @FromDate and @ToDate)";
            }

            if (houseId.HasValue)
                sqlQuery += " and h.id = @HauseId";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    if (!onlyActive)
                    {
                        cmd.Parameters.AddWithValue("@FromDate", fromDate);
                        cmd.Parameters.AddWithValue("@ToDate", toDate);

                    }

                    if (houseId.HasValue)
                        cmd.Parameters.AddWithValue("@HauseId", houseId.Value);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var alertDtos = new List<AlertDto>();
                        while (dataReader.Read())
                        {
                            alertDtos.Add(new AlertDto
                            {
                                Id = dataReader.GetInt32("alert_id"),
                                StreetId = dataReader.GetInt32("street_id"),
                                HouseId = dataReader.GetInt32("house_id"),
                                StreetName = dataReader.GetString("street_name"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                StartDate = dataReader.GetDateTime("start_date"),
                                EndDate = dataReader.GetNullableDateTime("end_date"),
                                Description = dataReader.GetNullableString("description"),
                                Type = new AlertTypeDto
                                {
                                    Id = dataReader.GetInt32("alert_type_id"),
                                    Name = dataReader.GetString("alert_type_name")
                                },
                                ServiceType = new AlertServiceTypeDto
                                {
                                    Id = dataReader.GetInt32("alert_service_type_id"),
                                    Name = dataReader.GetString("alert_service_type_name")
                                },
                                User = new RequestUserDto()
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("SurName"),
                                    FirstName = dataReader.GetNullableString("FirstName"),
                                    PatrName = dataReader.GetNullableString("PatrName"),
                                },
                                CreateDate = dataReader.GetDateTime("create_date")
                            });
                        }

                        dataReader.Close();
                        return alertDtos.ToArray();
                    }
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

        public void ChangeDescription(int userId, int requestId, string description)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var cmd =
                        new MySqlCommand(@"update CallCenter.Requests set description = @Desc where id = @RequestId",
                            conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@Desc", description);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public SmsSettingDto GetSmsSettingsForServiceCompany(int userId, int? serviceCompanyId)
        {
            var result = new SmsSettingDto {SendToClient = false, SendToWorker = false};
            if (serviceCompanyId.HasValue)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var cmd = new MySqlCommand(
                        "SELECT S.sms_to_worker, S.sms_to_abonent, S.sms_sender FROM CallCenter.ServiceCompanies S where id = @ID",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", serviceCompanyId);
                        using (var dataReader = cmd.ExecuteReader())
                        {
                            if (dataReader.Read())
                            {
                                return new SmsSettingDto
                                {
                                    SendToClient = dataReader.GetBoolean("sms_to_abonent"),
                                    SendToWorker = dataReader.GetBoolean("sms_to_worker"),
                                    Sender = dataReader.GetNullableString("sms_sender")
                                };
                            }

                            dataReader.Close();
                        }
                    }
                }
            }

            return result;
        }

        private void SendSmsToWorker(int userId, int requestId, int workerId)
        {
            var request = GetRequest(userId, requestId);
            var worker = GetWorkerById(userId, workerId);
            if (!worker.SendSms)
                return;
            var smsSettings = GetSmsSettingsForServiceCompany(userId, request.ServiceCompanyId);
            var service = GetServiceById(userId, request.Type.Id);
            var parrentService = request.Type.ParentId.HasValue
                ? GetServiceById(userId, request.Type.ParentId.Value)
                : null;
            if (!((parrentService?.CanSendSms ?? true) && service.CanSendSms))
            {
                return;
            }

            string phones = "";
            if (request.Contacts != null && request.Contacts.Length > 0)
                phones = request.Contacts.OrderBy(c => c.IsMain).Select(c =>
                    {
                        var retVal = c.PhoneNumber.Length == 10 ? "8" + c.PhoneNumber : c.PhoneNumber;
                        //if (!string.IsNullOrEmpty(c.Name))
                        //{
                        //    retVal += $" - {c.Name}";
                        //}
                        return retVal;
                    }
                ).FirstOrDefault();
            //.Aggregate((i, j) => i + ";" + j);
            //phones = request.Contacts.Select(c => $"{c.PhoneNumber} - {c.SurName} {c.FirstName} {c.PatrName}").Aggregate((i, j) => i + ";" + j);

            if (smsSettings.SendToWorker)
            {
                var immediateMessage = request.IsImmediate ? "АВАРИЙНАЯ №" : "";
                var smsText =
                    $"{immediateMessage}{request.Id} {phones ?? ""} {request.Address.FullAddress}.{request.Type.Name}({request.Description ?? ""})";
                if ((!request.IsImmediate) && smsText.Length > 70)
                {
                    smsText = smsText.Substring(0, 70);
                }

                //var smsText = $"№ {request.Id}. {request.Type.Name}({request.Description}) {request.Address.FullAddress}. {phones}.";
                SendSms(userId, requestId, smsSettings.Sender, worker.Phone, smsText, false);
            }

            //SendSms(requestId, smsSettings.Sender, worker.Phone,
            //    $"№ {requestId}. {request.Type.ParentName}/{request.Type.Name}({request.Description}) {request.Address.FullAddress}. {phones}.",
            //    false);
            //SendSms(requestId, smsSettings.Sender, worker.Phone, $"Заявка № {requestId}. Услуга {request.Type.ParentName}. Причина {request.Type.Name}. Примечание: {request.Description}. Адрес: {request.Address.FullAddress}. Телефоны {phones}.");
        }

        public void AddNewMaster(int userId, int requestId, int? workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("CALL phone_client.add_new_master(@userId,@requestId,@workerId)",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    cmd.Parameters.AddWithValue("@workerId", workerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddNewExecutor(int userId, int requestId, int? workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("CALL phone_client.add_new_executor(@userId,@requestId,@workerId)",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    cmd.Parameters.AddWithValue("@workerId", workerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddNewExecuteDate(int userId, int requestId, DateTime executeDate, PeriodDto period, string note)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        using (
                            var cmd =
                                new MySqlCommand(
                                    @"insert into CallCenter.RequestExecuteDateHistory (request_id,operation_date,user_id,execute_date,period_time_id,note) 
    values(@RequestId,sysdate(),@UserId,@ExecuteDate,@Period,@Note);", conn))
                        {
                            cmd.Parameters.AddWithValue("@RequestId", requestId);
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@ExecuteDate", executeDate);
                            cmd.Parameters.AddWithValue("@Note", note);
                            cmd.Parameters.AddWithValue("@Period", period.Id);
                            cmd.ExecuteNonQuery();
                        }

                        using (
                            var cmd =
                                new MySqlCommand(
                                    @"update CallCenter.Requests set execute_date = @ExecuteDate, period_time_id = @Period  where id = @RequestId",
                                    conn))
                        {
                            cmd.Parameters.AddWithValue("@RequestId", requestId);
                            cmd.Parameters.AddWithValue("@ExecuteDate", executeDate + period.SetTime.TimeOfDay);
                            cmd.Parameters.AddWithValue("@Period", period.Id);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc.ToString());
            }
        }

        public void EditRequest(int userId, int requestId, int requestTypeId, string requestMessage, bool immediate,
            bool chargeable, bool isBadWork, int garanty, bool isRetry, DateTime? alertTime, DateTime? termOfExecution)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    @"call CallCenter.UpdateRequest(@userId,@requestId,@requestTypeId,@requestMessage,@immediate,@chargeable,@badWork,@garanty,@retry,@alertTime,@termOfExecution);",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    cmd.Parameters.AddWithValue("@requestTypeId", requestTypeId);
                    cmd.Parameters.AddWithValue("@requestMessage", requestMessage);
                    cmd.Parameters.AddWithValue("@immediate", immediate);
                    cmd.Parameters.AddWithValue("@chargeable", chargeable);
                    cmd.Parameters.AddWithValue("@badWork", isBadWork);
                    cmd.Parameters.AddWithValue("@garanty", garanty);
                    cmd.Parameters.AddWithValue("@retry", isRetry);
                    cmd.Parameters.AddWithValue("@alertTime", alertTime);
                    cmd.Parameters.AddWithValue("@termOfExecution", termOfExecution);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteRequestRatingById(int userId, int itemId)
        {
            var query = "delete from CallCenter.RequestRating where id = @Id;";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", itemId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SetRating(int userId, int requestId, int ratingId, string description)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        using (
                            var cmd =
                                new MySqlCommand(
                                    @"insert into CallCenter.RequestRating(request_id,create_date,rating_id,Description,user_id)
 values(@RequestId,sysdate(),@RatingId,@Desc,@UserId);", conn))
                        {
                            cmd.Parameters.AddWithValue("@RequestId", requestId);
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@RatingId", ratingId);
                            cmd.Parameters.AddWithValue("@Desc", description);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc.ToString());
                throw;
            }

        }

        public List<RequestRatingDto> GetRequestRating(int userId)
        {
            var query = "SELECT id, name FROM CallCenter.RatingTypes R order by OrderNum";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var ratings = new List<RequestRatingDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            ratings.Add(new RequestRatingDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name")
                            });
                        }

                        dataReader.Close();
                    }

                    return ratings;
                }
            }
        }

        public List<RequestRatingListDto> GetRequestRatings(int userId, int requestId)
        {
            var result = new List<RequestRatingListDto>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(
                    @"SELECT r.id,request_id,create_date,rating_id,Description,t.name rating_name,
 u.Id user_id, u.SurName, u.FirstName,u.PatrName FROM CallCenter.RequestRating r
 join CallCenter.Users u on u.id = r.user_id
 join CallCenter.RatingTypes t on t.id = rating_id
 where request_id = @RequestId;", conn))
                {
                    cmd.Parameters.AddWithValue("@RequestId", requestId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            result.Add(new RequestRatingListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                CreateDate = dataReader.GetDateTime("create_date"),
                                Description = dataReader.GetNullableString("Description"),
                                Rating = dataReader.GetNullableString("rating_name"),
                                CreateUser = new RequestUserDto()
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("SurName"),
                                    FirstName = dataReader.GetNullableString("FirstName"),
                                    PatrName = dataReader.GetNullableString("PatrName"),
                                }
                            });
                        }

                        dataReader.Close();
                    }
                }

                return result;
            }
        }

        public List<ExecuteDateHistoryDto> GetExecuteDateHistoryByRequest(int userId, int requestId)
        {
            var query =
                @"SELECT R.operation_date,R.user_id,u.surname,u.firstname,u.patrname,R.note,R.execute_date,p.Name FROM CallCenter.RequestExecuteDateHistory R
 join CallCenter.Users u on u.id = user_id
 join CallCenter.PeriodTimes p on p.id = R.period_time_id
 where request_id = @RequestId";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var executeDateHistoryDtos = new List<ExecuteDateHistoryDto>();
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            executeDateHistoryDtos.Add(new ExecuteDateHistoryDto
                            {
                                CreateTime = dataReader.GetDateTime("operation_date"),
                                Note = dataReader.GetNullableString("note"),
                                ExecuteTime = dataReader.GetDateTime("execute_date"),
                                ExecutePeriod = dataReader.GetNullableString("name"),
                                CreateUser = new RequestUserDto
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("surname"),
                                    FirstName = dataReader.GetNullableString("firstname"),
                                    PatrName = dataReader.GetNullableString("patrname"),
                                },
                            });
                        }

                        dataReader.Close();
                    }

                    return executeDateHistoryDtos.OrderByDescending(i => i.CreateTime).ToList();
                }
            }
        }

        public void DeleteScheduleTask(int userId, int sheduleId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        using (
                            var cmd =
                                new MySqlCommand(@"update CallCenter.ScheduleTasks set deleted = 1 where id = @Id;",
                                    conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", sheduleId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc.ToString());
                throw;
            }
        }

        public void SetRequestWorkingTimes(int userId, int requestId, DateTime fromTime, DateTime toTime)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    @"insert into CallCenter.RequestTimesHistory(from_time,to_time,user_id,request_id)
 values(@fromTime,@toTime,@userId,@requestId);", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    cmd.Parameters.AddWithValue("@fromTime", fromTime);
                    cmd.Parameters.AddWithValue("@toTime", toTime);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd =
                    new MySqlCommand(
                        @"update CallCenter.Requests set from_time = @fromTime, to_time = @toTime where id = @requestId;",
                        conn))
                {
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    cmd.Parameters.AddWithValue("@fromTime", fromTime);
                    cmd.Parameters.AddWithValue("@toTime", toTime);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddScheduleTask(int userId, int workerId, int? requestId, DateTime fromDate, DateTime toDate,
            string eventDescription)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        using (
                            var cmd =
                                new MySqlCommand(
                                    @"insert into CallCenter.ScheduleTasks (create_date,worker_id,request_id,from_date,to_date,event_description)
 values(sysdate(),@WorkerId,@RequestId,@FromDate,@ToDate,@Desc);", conn))
                        {
                            cmd.Parameters.AddWithValue("@RequestId", requestId);
                            cmd.Parameters.AddWithValue("@WorkerId", workerId);
                            cmd.Parameters.AddWithValue("@FromDate", fromDate);
                            cmd.Parameters.AddWithValue("@ToDate", toDate);
                            cmd.Parameters.AddWithValue("@Desc", eventDescription);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc.ToString());
                throw;
            }
        }

        public List<AttachmentDto> GetAttachments(int userId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd = new MySqlCommand(
                        @"SELECT a.id,a.request_id,a.name,a.file_name,a.create_date,u.id user_id,u.SurName,u.FirstName,u.PatrName,
a.worker_id, w.sur_name,w.first_name,w.patr_name
FROM CallCenter.RequestAttachments a
 join CallCenter.Users u on u.id = a.user_id
 left join CallCenter.Workers w on w.id = a.worker_id
where a.deleted = 0 and a.request_id = @requestId", conn))
                {
                    cmd.Parameters.AddWithValue("@requestId", requestId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var attachments = new List<AttachmentDto>();
                        RequestUserDto user;

                        while (dataReader.Read())
                        {
                            var workerId = dataReader.GetNullableInt("worker_id");
                            if (workerId.HasValue)
                            {
                                user = new RequestUserDto()
                                {
                                    Id = workerId.Value,
                                    SurName = dataReader.GetNullableString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                };
                            }
                            else
                            {
                                user = new RequestUserDto
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("SurName"),
                                    FirstName = dataReader.GetNullableString("FirstName"),
                                    PatrName = dataReader.GetNullableString("PatrName"),
                                };

                            }

                            attachments.Add(new AttachmentDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                FileName = dataReader.GetString("file_name"),
                                CreateDate = dataReader.GetDateTime("create_date"),
                                RequestId = dataReader.GetInt32("request_id"),
                                User = user
                            });
                        }

                        dataReader.Close();
                        return attachments;
                    }
                }
            }
        }

        public void AddNewNote(int userId, int requestId, string note)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        using (
                            var cmd =
                                new MySqlCommand(
                                    @"insert into CallCenter.RequestNoteHistory (request_id,operation_date,user_id,note)
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
            catch (Exception exc)
            {
                Logger.LogError(exc.ToString());
                throw;
            }

        }

        public void AddNewTermOfExecution(int userId, int requestId, DateTime termOfExecution, string note)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        using (
                            var cmd =
                                new MySqlCommand(
                                    @"update CallCenter.Requests set term_of_execution = @ExecuteDate where id = @RequestId",
                                    conn))
                        {
                            cmd.Parameters.AddWithValue("@RequestId", requestId);
                            cmd.Parameters.AddWithValue("@ExecuteDate", termOfExecution);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc.ToString());
            }
        }

        public void SendSms(int userId, int requestId, string sender, string phone, string message, bool isClient)
        {
            if (requestId <= 0 || string.IsNullOrEmpty(phone) || phone.Length < 10 || string.IsNullOrEmpty(sender))
                return;
            var smsCount = 0;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (
                    var cmd =
                        new MySqlCommand(
                            "SELECT count(1) as count FROM CallCenter.SMSRequest S where request_id = @RequestId and phone = @Phone and create_date > sysdate() - interval 5 minute;",
                            conn))
                {
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@Phone", phone);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        smsCount = dataReader.GetInt32("count");
                    }
                }

                if (smsCount > 0)
                    return;

                using (var cmd =
                    new MySqlCommand(
                        "insert into CallCenter.SMSRequest(request_id,sender,phone,message,create_date, is_client) values(@Request, @Sender, @Phone,@Message,sysdate(),@IsClient)",
                        conn))
                {
                    cmd.Parameters.AddWithValue("@Request", requestId);
                    cmd.Parameters.AddWithValue("@Sender", sender);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@IsClient", isClient);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public IList<RequestForListDto> GetAlertedRequests(int userId)
        {
            var sqlQuery = "call phone_client.get_alert_requests(@userId);";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd =
                    new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    var requests = new List<RequestForListDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var recordUniqueId = dataReader.GetNullableString("recordId");
                            requests.Add(new RequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                HasAttachment = dataReader.GetBoolean("has_attach"),
                                IsBadWork = dataReader.GetBoolean("bad_work"),
                                HasRecord = !string.IsNullOrEmpty(recordUniqueId),
                                RecordUniqueId = recordUniqueId,
                                StreetPrefix = dataReader.GetString("prefix_name"),
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
                                Master = dataReader.GetNullableInt("worker_id") != null
                                    ? new RequestUserDto
                                    {
                                        Id = dataReader.GetInt32("worker_id"),
                                        SurName = dataReader.GetNullableString("sur_name"),
                                        FirstName = dataReader.GetNullableString("first_name"),
                                        PatrName = dataReader.GetNullableString("patr_name"),
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
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                Status = dataReader.GetNullableString("Req_Status"),
                                SpendTime = dataReader.GetNullableString("spend_time"),
                                FromTime = dataReader.GetNullableDateTime("from_time"),
                                ToTime = dataReader.GetNullableDateTime("to_time"),
                                AlertTime = dataReader.GetNullableDateTime("alert_time"),
                            });
                        }

                        dataReader.Close();
                    }

                    return requests;
                }
            }
        }

        public int? SaveNewRequest(int userId, string lastCallId, int addressId, int requestTypeId,
            ContactDto[] contactList, string requestMessage,
            bool chargeable, bool immediate, string callUniqueId, string entrance, string floor, DateTime? alertTime,
            bool isRetry, bool isBedWork, int? equipmentId, int warranty)
        {
            int newId;
            //_logger.Debug($"RequestService.SaveNewRequest({addressId},{requestTypeId},[{contactList.Select(x => $"{x.PhoneNumber}").Aggregate((f1, f2) => f1 + ";" + f2)}],{requestMessage},{chargeable},{immediate},{callUniqueId})");
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        //Определяем УК по адресу
                        var serviceCompanyId = (int?) null;
                        using (var scCmd = new MySqlCommand(@"SELECT h.service_company_id FROM CallCenter.Addresses a
 join CallCenter.Houses h on h.id = a.house_id
 where a.id = @AddressId", conn))
                        {
                            scCmd.Parameters.AddWithValue("@AddressId", addressId);

                            using (var dataReader = scCmd.ExecuteReader())
                            {
                                if (dataReader.Read())
                                {
                                    serviceCompanyId = dataReader.GetNullableInt("service_company_id");
                                }

                                dataReader.Close();
                            }
                        }

                        #region Сохранение заявки в базе данных

                        using (
                            var cmd = new MySqlCommand(
                                @"insert into CallCenter.Requests(address_id,type_id,description,create_time,is_chargeable,create_user_id,state_id,is_immediate, entrance, floor, service_company_id,retry ,bad_work , alert_time, equipment_id, garanty)
 values(@AddressId, @TypeId, @Message, sysdate(),@IsChargeable,@UserId,@State,@IsImmediate,@Entrance,@Floor,@ServiceCompanyId,@Retry,@BadWork,@AlertTime, @EquipmentId, @Garanty);
 select LAST_INSERT_ID();", conn))
                        {
                            cmd.Parameters.AddWithValue("@AddressId", addressId);
                            cmd.Parameters.AddWithValue("@TypeId", requestTypeId);
                            cmd.Parameters.AddWithValue("@Message", requestMessage);
                            cmd.Parameters.AddWithValue("@IsChargeable", chargeable);
                            cmd.Parameters.AddWithValue("@IsImmediate", immediate);
                            cmd.Parameters.AddWithValue("@Entrance", entrance);
                            cmd.Parameters.AddWithValue("@Floor", floor);
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@State", 1);
                            cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);
                            cmd.Parameters.AddWithValue("@AlertTime", alertTime);
                            cmd.Parameters.AddWithValue("@Retry", isRetry);
                            cmd.Parameters.AddWithValue("@BadWork", isBedWork);
                            cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);
                            cmd.Parameters.AddWithValue("@Garanty", warranty);
                            newId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        #endregion

                        #region Прикрепление звонка к заявке

                        if (!string.IsNullOrEmpty(callUniqueId))
                        {
                            using (var cmd =
                                new MySqlCommand(
                                    "insert into CallCenter.RequestCalls(request_id,uniqueID) values(@Request, @UniqueId)",
                                    conn))
                            {
                                cmd.Parameters.AddWithValue("@Request", newId);
                                cmd.Parameters.AddWithValue("@UniqueId", callUniqueId);
                                cmd.ExecuteNonQuery();
                            }

                            AddCallHistory(newId, callUniqueId, userId, lastCallId,
                                "CreateNewRequest", conn);
                        }

                        #endregion

                        #region Сохранение контактных номеров 

                        SaveContacts(newId, contactList, conn);

                        #endregion

                        #region Сохрарнение описания в истории изменений

                        using (
                            var cmd =
                                new MySqlCommand(
                                    @"insert into CallCenter.RequestDescriptionHistory (request_id,operation_date,user_id,description) 
    values(@RequestId,sysdate(),@UserId,@Message);", conn))
                        {
                            cmd.Parameters.AddWithValue("@RequestId", newId);
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@Message", requestMessage);
                            cmd.ExecuteNonQuery();
                        }

                        #endregion

                        transaction.Commit();
                        return newId;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc.ToString());
            }

            return null;
        }

        public void SaveContacts(int requestId, ContactDto[] contactList, MySqlConnection connection)
        {
            MySqlConnection conn;
            if (connection != null)
            {
                conn = connection;
            }
            else
            {
                conn = new MySqlConnection(_connectionString);
                conn.Open();
            }

            foreach (
                var contact in
                contactList.Where(c => !string.IsNullOrEmpty(c.PhoneNumber))
                    .OrderByDescending(c => c.IsMain))
            {
                var clientPhoneId = 0;
                ContactDto currentInfo = null;
                using (
                    var cmd = new MySqlCommand(
                        "SELECT id,name,email,addition FROM CallCenter.ClientPhones C where Number = @Phone",
                        conn))
                {
                    cmd.Parameters.AddWithValue("@Phone", contact.PhoneNumber);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            currentInfo = new ContactDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetNullableString("name"),
                                Email = dataReader.GetNullableString("email"),
                                AdditionInfo = dataReader.GetNullableString("addition"),
                            };
                            clientPhoneId = currentInfo.Id;
                        }

                        dataReader.Close();
                    }
                }

                if (currentInfo == null)
                {
                    using (
                        var cmd = new MySqlCommand(
                            @"insert into CallCenter.ClientPhones(Number,name,email,addition) values(@Phone,@Name,@Email,@AddInfo);
    select LAST_INSERT_ID();", conn))
                    {
                        cmd.Parameters.AddWithValue("@Phone", contact.PhoneNumber);
                        cmd.Parameters.AddWithValue("@Name", contact.Name);
                        cmd.Parameters.AddWithValue("@Email", contact.Email);
                        cmd.Parameters.AddWithValue("@AddInfo", contact.AdditionInfo);
                        clientPhoneId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                else
                {
                    using (
                        var cmd = new MySqlCommand(
                            @"update CallCenter.ClientPhones set name = @Name,email = @Email,addition = @AddInfo where id = @Id;",
                            conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", currentInfo.Id);
                        cmd.Parameters.AddWithValue("@Name",
                            string.IsNullOrEmpty(contact.Name) ? currentInfo.Name : contact.Name);
                        cmd.Parameters.AddWithValue("@Email",
                            string.IsNullOrEmpty(contact.Email) ? currentInfo.Email : contact.Email);
                        cmd.Parameters.AddWithValue("@AddInfo",
                            string.IsNullOrEmpty(contact.AdditionInfo)
                                ? currentInfo.AdditionInfo
                                : contact.AdditionInfo);
                        cmd.ExecuteNonQuery();
                    }
                }

                using (
                    var cmd =
                        new MySqlCommand(
                            @"insert into CallCenter.RequestContacts (request_id,IsMain,ClientPhone_id) 
    values(@RequestId,@IsMain,@PhoneId);
    select LAST_INSERT_ID();", conn))
                {
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@IsMain", contact.IsMain);
                    cmd.Parameters.AddWithValue("@PhoneId", clientPhoneId);
                    contact.Id = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public AlertTimeDto[] GetAlertTimes(int userId, bool isImmediate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd =
                    new MySqlCommand(
                        "SELECT id,name,add_minutes FROM CallCenter.AlertTimes where is_immediate = @Immediate and enabled = 1 order by id",
                        conn))
                {
                    cmd.Parameters.AddWithValue("@Immediate", isImmediate);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var alerts = new List<AlertTimeDto>();
                        while (dataReader.Read())
                        {
                            alerts.Add(new AlertTimeDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                AddMinutes = dataReader.GetInt32("add_minutes"),
                            });
                        }

                        dataReader.Close();
                        return alerts.ToArray();
                    }
                }
            }
        }

        public string GetOnlyActiveCallUniqueIdByCallId(int userId, string callId)
        {
            string retVal = null;
            if (!string.IsNullOrEmpty(callId))
            {
                var query =
                    $@"SELECT case when (A.UniqueID <= ifnull(A2.UniqueID,A.UniqueID)) then A.UniqueID else A2.UniqueID end uniqueId FROM asterisk.ActiveChannels A
 left join asterisk.ActiveChannels A2 on A2.BridgeId = A.BridgeId and A2.UniqueID <> A.UniqueID
 where A.call_id like '{callId}%'";
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var dataReader = cmd.ExecuteReader())
                        {
                            if (dataReader.Read())
                            {
                                retVal = dataReader.GetNullableString("uniqueId");
                            }

                            dataReader.Close();
                        }
                    }
                }

            }

            return retVal;
        }
        public string GetSipServer()
        {
            string retVal = null;
            var query =
                    $@"call phone_client.get_sip_server()";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            retVal = dataReader.GetNullableString("sip_ip");
                        }

                        dataReader.Close();
                    }
                }
            }

            return retVal;
        }

        public ScheduleTaskDto[] GetScheduleTasks(int userId, int workerId, DateTime fromDate, DateTime toDate)
        {
            var query =
                @"SELECT s.id,w.id worker_id,w.sur_name,w.first_name,w.patr_name,s.request_id,s.from_date,s.to_date,s.event_description FROM CallCenter.ScheduleTasks s
join CallCenter.Workers w on s.worker_id = w.id
where w.id = @WorkerId and s.from_date between @FromDate and @ToDate and deleted = 0;";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    var items = new List<ScheduleTaskDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            items.Add(new ScheduleTaskDto
                            {
                                Id = dataReader.GetInt32("id"),
                                RequestId = dataReader.GetNullableInt("request_id"),
                                Worker = new WorkerDto()
                                {
                                    Id = dataReader.GetInt32("worker_id"),
                                    SurName = dataReader.GetString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                },
                                FromDate = dataReader.GetDateTime("from_date"),
                                ToDate = dataReader.GetDateTime("to_date"),
                                EventDescription = dataReader.GetNullableString("event_description")
                            });
                        }

                        dataReader.Close();
                    }

                    return items.ToArray();
                }
            }
        }

        public void AddCallHistory(int requestId, string callUniqueId, int userId, string callId, string methodName,
            MySqlConnection connection = null)
        {
            MySqlConnection conn;
            if (requestId <= 0 || string.IsNullOrEmpty(callUniqueId))
                return;
            if (connection != null)
            {
                conn = connection;
            }
            else
            {
                conn = new MySqlConnection(_connectionString);
                conn.Open();
            }

            using (var cmd =
                new MySqlCommand(
                    @"insert into CallCenter.RequestCallsHistory (request_id, unique_Id, add_date, user_id, call_id,method_name)
 values(@Request, @UniqueId,sysdate(),@UserID,@CallId,@MethodName)", conn))
            {
                cmd.Parameters.AddWithValue("@Request", requestId);
                cmd.Parameters.AddWithValue("@UniqueId", callUniqueId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@CallId", callId);
                cmd.Parameters.AddWithValue("@MethodName", methodName);
                cmd.ExecuteNonQuery();
            }
        }

        public WorkerDto GetWorkerById(int userId, int workerId)
        {
            WorkerDto worker = null;
            var query =
                @"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,
    w.can_assign,w.parent_worker_id,w.is_master,w.is_executer,w.is_dispetcher, sp.name speciality_name,send_sms,w.login,w.password,
    w.filter_by_houses,w.can_create_in_web,w.show_all_request,w.show_only_garanty,w.allow_statistics,w.can_set_rating,w.can_close_request,
    w.can_change_executors,w.send_notification,w.enabled,w.show_only_my
    FROM CallCenter.Workers w
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id
    left join CallCenter.Speciality sp on sp.id = w.speciality_id
    where w.id = @WorkerId";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            worker = new WorkerDto
                            {
                                Id = dataReader.GetInt32("id"),
                                ServiceCompanyId = dataReader.GetNullableInt("service_id"),
                                ServiceCompanyName = dataReader.GetNullableString("service_name"),
                                SurName = dataReader.GetString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                SpecialityName = dataReader.GetNullableString("speciality_name"),
                                SpecialityId = dataReader.GetNullableInt("speciality_id"),
                                Phone = dataReader.GetNullableString("phone"),
                                Login = dataReader.GetNullableString("login"),
                                Password = dataReader.GetNullableString("password"),
                                CanAssign = dataReader.GetBoolean("can_assign"),
                                IsMaster = dataReader.GetBoolean("is_master"),
                                IsExecuter = dataReader.GetBoolean("is_executer"),
                                IsDispetcher = dataReader.GetBoolean("is_dispetcher"),
                                SendSms = dataReader.GetBoolean("send_sms"),
                                AppNotification = dataReader.GetBoolean("send_notification"),
                                ParentWorkerId = dataReader.GetNullableInt("parent_worker_id"),
                                CanSetRating = dataReader.GetBoolean("can_set_rating"),
                                CanCloseRequest = dataReader.GetBoolean("can_close_request"),
                                CanChangeExecutor = dataReader.GetBoolean("can_change_executors"),
                                CanCreateRequest = dataReader.GetBoolean("can_create_in_web"),
                                CanShowStatistic = dataReader.GetBoolean("allow_statistics"),
                                FilterByHouses = dataReader.GetBoolean("filter_by_houses"),
                                ShowOnlyMy = dataReader.GetBoolean("show_only_my"),
                                ShowAllRequest = dataReader.GetBoolean("show_all_request"),
                                ShowOnlyGaranty = dataReader.GetBoolean("show_only_garanty"),
                                Enabled = dataReader.GetBoolean("enabled"),
                            };
                        }

                        dataReader.Close();
                    }

                    return worker;
                }
            }
        }

        public RequestInfoDto GetRequest(int userId, int requestId)
        {
            RequestInfoDto result = null;
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var cmd =
                        new MySqlCommand(
                            @"SELECT R.id req_id,R.Address_id,R.type_id,R.description, R.create_time,R.is_chargeable,R.is_immediate,R.period_time_id,R.state_id,R.worker_id,R.execute_date,R.service_company_id,
    RS.name state_name,RS.description state_descript,
    RT.parrent_id,RT.name as rt_name,RT2.name rt_parrent_name,
    A.type_id address_type_id,A.house_id,A.flat,
    AT.Name type_name,
    H.street_id,H.building,H.corps,H.service_company_id,H.region_id,
    S.name street_name,S.prefix_id,S.city_id,
    SP.Name prefix_name,
    C.name City_name,
    case when create_user_id = 0 then cw.id else create_user_id end create_user_id,
    case when create_user_id = 0 then cw.sur_name else u.surname end surname,
    case when create_user_id = 0 then cw.first_name else u.firstname end firstname,
    case when create_user_id = 0 then cw.patr_name else u.patrname end patrname,
    entrance,floor,
    rtype.rating_id,rating.name RatingName,rtype.Description RatingDesc,
    R.from_time,R.to_time,R.bad_work,R.alert_time,R.garanty,R.retry,
    R.executer_id, R.equipment_id, eqt.name eq_type_name, eq.name eq_name,R.term_of_execution
     FROM CallCenter.Requests R
    join CallCenter.RequestState RS on RS.id = R.state_id
    join CallCenter.RequestTypes RT on RT.id = R.type_id
    left join CallCenter.RequestTypes RT2 on RT2.id = RT.parrent_id
    join CallCenter.Addresses A on A.id = R.address_id
    join CallCenter.AddressesTypes AT on AT.id = A.type_id
    join CallCenter.Houses H on H.id = A.house_id
    join CallCenter.Streets S on S.id = H.street_id
    join CallCenter.StreetPrefixes SP on SP.id = S.prefix_id
    join CallCenter.Cities C on C.id = S.city_id
    join CallCenter.Users u on u.id = R.Create_user_id
    left join (select a.request_id,max(a.id) as max_id from CallCenter.RequestRating a group by a.request_id ) max_rtype on max_rtype.request_id = R.id
    left join CallCenter.RequestRating rtype on rtype.id = max_rtype.max_id
    left join CallCenter.RatingTypes rating on rtype.rating_id = rating.id
    left join CallCenter.Equipments eq on eq.id = R.equipment_id
    left join CallCenter.EquipmentTypes eqt on eqt.id = eq.type_id
    left join CallCenter.Workers cw on cw.id = R.create_worker_id
    where R.id = @reqId", conn))
                    {
                        cmd.Parameters.AddWithValue("@reqId", requestId);
                        using (var dataReader = cmd.ExecuteReader())
                        {
                            if (dataReader.Read())
                            {
                                result = new RequestInfoDto
                                {
                                    Id = dataReader.GetInt32("req_id"),
                                    CreateTime = dataReader.GetDateTime("create_time"),
                                    PeriodId = dataReader.GetNullableInt("period_time_id"),
                                    MasterId = dataReader.GetNullableInt("worker_id"),
                                    ExecuterId = dataReader.GetNullableInt("executer_id"),
                                    ServiceCompanyId = dataReader.GetNullableInt("service_company_id"),
                                    IsChargeable = dataReader.GetBoolean("is_chargeable"),
                                    IsImmediate = dataReader.GetBoolean("is_immediate"),
                                    IsBadWork = dataReader.GetBoolean("bad_work"),
                                    IsRetry = dataReader.GetBoolean("retry"),
                                    GarantyId = dataReader.GetInt32("garanty"),
                                    Description = dataReader.GetNullableString("description"),
                                    Entrance = dataReader.GetNullableString("entrance"),
                                    Floor = dataReader.GetNullableString("floor"),
                                    ExecuteDate = dataReader.GetNullableDateTime("execute_date"),
                                    FromTime = dataReader.GetNullableDateTime("from_time"),
                                    ToTime = dataReader.GetNullableDateTime("to_time"),
                                    AlertTime = dataReader.GetNullableDateTime("alert_time"),
                                    TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                                    Type = new RequestTypeDto
                                    {
                                        Id = dataReader.GetInt32("type_id"),
                                        Name = dataReader.GetString("rt_name"),
                                        ParentId = dataReader.GetNullableInt("parrent_id"),
                                        ParentName = dataReader.GetString("rt_parrent_name"),
                                    },
                                    State = new RequestStateDto
                                    {
                                        Id = dataReader.GetInt32("state_id"),
                                        Name = dataReader.GetString("state_name"),
                                        Description = dataReader.GetString("state_descript")
                                    },
                                    CreateUser = new RequestUserDto
                                    {
                                        Id = dataReader.GetInt32("Create_user_id"),
                                        SurName = dataReader.GetString("SurName"),
                                        FirstName = dataReader.GetNullableString("FirstName"),
                                        PatrName = dataReader.GetNullableString("PatrName")
                                    },
                                    Address = new AddressDto
                                    {
                                        Id = dataReader.GetInt32("Address_id"),
                                        Building = dataReader.GetString("building"),
                                        Corpus = dataReader.GetNullableString("corps"),
                                        City = dataReader.GetString("City_name"),
                                        CityId = dataReader.GetInt32("city_id"),
                                        HouseId = dataReader.GetInt32("house_id"),
                                        StreetName = dataReader.GetString("street_name"),
                                        Flat = dataReader.GetNullableString("flat"),
                                        TypeId = dataReader.GetInt32("address_type_id"),
                                        Type = dataReader.GetString("type_name"),
                                        StreetId = dataReader.GetInt32("street_id"),
                                        StreetPrefixId = dataReader.GetInt32("prefix_id"),
                                        StreetPrefix = dataReader.GetString("prefix_name")
                                    },
                                    Rating = dataReader.GetNullableInt("rating_id").HasValue
                                        ? new RequestRatingDto
                                        {
                                            Id = dataReader.GetInt32("rating_id"),
                                            Name = dataReader.GetString("RatingName"),
                                            Description = dataReader.GetNullableString("RatingDesc")
                                        }
                                        : new RequestRatingDto(),
                                    Equipment = dataReader.GetNullableInt("equipment_id").HasValue
                                        ? new EquipmentDto
                                        {
                                            Id = dataReader.GetInt32("equipment_id"),
                                            Name =
                                                $"{dataReader.GetString("eq_type_name")} - {dataReader.GetString("eq_name")}"
                                        }
                                        : new EquipmentDto {Id = null, Name = "Нет"}
                                };
                            }

                            dataReader.Close();
                            if (result != null)
                            {
                                var contactInfo = new List<ContactDto>();
                                using (
                                    var contact =
                                        new MySqlCommand(
                                            @"SELECT R.id, IsMain,Number,name,email,addition from CallCenter.RequestContacts R
    join CallCenter.ClientPhones P on P.id = R.ClientPhone_id where request_id = @reqId order by IsMain desc",
                                            conn))
                                {
                                    contact.Parameters.AddWithValue("@reqId", requestId);
                                    using (var contactReader = contact.ExecuteReader())
                                    {
                                        while (contactReader.Read())
                                        {
                                            contactInfo.Add(new ContactDto
                                            {
                                                Id = contactReader.GetInt32("id"),
                                                IsMain = contactReader.GetBoolean("IsMain"),
                                                PhoneNumber = contactReader.GetString("Number"),
                                                Name = contactReader.GetNullableString("name"),
                                                Email = contactReader.GetNullableString("email"),
                                                AdditionInfo = contactReader.GetNullableString("addition"),
                                            });
                                        }
                                    }
                                }

                                result.Contacts = contactInfo.ToArray();
                            }

                            return result;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc.ToString());
            }

            return null;
        }

        public FlatDto[] GetFlats(int userId, int houseId)
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

                    return flats.ToArray();
                }
            }
        }

        public StatusDto[] GetStatuses(int userId)
        {
            var sqlQuery = @"CALL phone_client.get_statuses(@userId);";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
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

        public ServiceDto[] GetServices(int userId, long? parentId, int? houseId)
        {

            var sqlQuery = @"CALL phone_client.get_services(@userId,@parentId,@houseId);";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@parentId", parentId);
                    cmd.Parameters.AddWithValue("@houseId", houseId);

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
                                Immediate = dataReader.GetBoolean("immediate")
                            });
                        }

                        dataReader.Close();
                    }

                    return services.ToArray();
                }
            }
        }

        public StreetDto[] GetStreets(int userId, int cityId, int? serviceCompanyId = null)
        {
            var sqlQuery = @"CALL phone_client.get_streets(@userId,@cityId,@serviceCompanyId);";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@cityId", cityId);
                    cmd.Parameters.AddWithValue("@serviceCompanyId", serviceCompanyId);

                    var streets = new List<StreetDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            streets.Add(new StreetDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                CityId = dataReader.GetInt32("city_id"),
                                Prefix = new StreetPrefixDto
                                {
                                    Id = dataReader.GetInt32("Prefix_id"),
                                    Name = dataReader.GetString("Prefix_Name"),
                                    ShortName = dataReader.GetString("ShortName")
                                }
                            });
                        }

                        dataReader.Close();
                    }

                    return streets.ToArray();
                }
            }
        }

        public HouseDto[] GetHouses(int userId, int? serviceCompanyId, int streetId)
        {
            var sqlQuery = @"CALL phone_client.get_houses(@userId,@serviceCompanyId,@streetId);";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@serviceCompanyId", serviceCompanyId);
                    cmd.Parameters.AddWithValue("@streetId", streetId);
                    if (serviceCompanyId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId.Value);
                    }

                    var houses = new List<HouseDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            houses.Add(new HouseDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Building = dataReader.GetString("building"),
                                StreetId = dataReader.GetInt32("street_id"),
                                RegionId = dataReader.GetNullableInt("region_id"),
                                RegionName = dataReader.GetNullableString("region_name"),
                                StreetName = dataReader.GetNullableString("street_name"),
                                Corpus = dataReader.GetNullableString("corps"),
                                ServiceCompanyId = dataReader.GetNullableInt("service_company_id"),
                                ServiceCompanyName = dataReader.GetNullableString("service_company_name"),
                                EntranceCount = dataReader.GetNullableInt("entrance_count"),
                                FlatCount = dataReader.GetNullableInt("flat_count"),
                                FloorCount = dataReader.GetNullableInt("floor_count"),
                                ElevatorCount = dataReader.GetNullableInt("elevator_count"),
                                HaveParking = dataReader.GetBoolean("have_parking"),
                            });
                        }

                        dataReader.Close();
                    }

                    return houses.ToArray();
                }
            }
        }

        public MeterListDto[] GetMetersByDate(int userId, int? serviceCompanyId, DateTime fromDate, DateTime toDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand("CALL phone_client.get_meters(@userId,@ServiceCompanyId,@FromDate,@ToDate)", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var metersDtos = new List<MeterListDto>();
                        while (dataReader.Read())
                        {
                            metersDtos.Add(new MeterListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                StreetId = dataReader.GetInt32("street_id"),
                                HouseId = dataReader.GetInt32("house_id"),
                                AddressId = dataReader.GetInt32("address_id"),
                                ServiceCompany = dataReader.GetNullableString("company_name"),
                                PersonalAccount = dataReader.GetNullableString("personal_account"),
                                StreetName = dataReader.GetString("street_name"),
                                Flat = dataReader.GetString("flat"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                Date = dataReader.GetDateTime("meters_date"),
                                Electro1 = dataReader.GetDouble("electro_t1"),
                                Electro2 = dataReader.GetDouble("electro_t2"),
                                ColdWater1 = dataReader.GetDouble("cool_water1"),
                                HotWater1 = dataReader.GetDouble("hot_water1"),
                                ColdWater2 = dataReader.GetDouble("cool_water2"),
                                HotWater2 = dataReader.GetDouble("hot_water2"),
                                ColdWater3 = dataReader.GetDouble("cool_water3"),
                                HotWater3 = dataReader.GetDouble("hot_water3"),
                                Heating = dataReader.GetDouble("heating"),
                                Heating2 = dataReader.GetNullableDouble("heating2"),
                                Heating3 = dataReader.GetNullableDouble("heating3"),
                                Heating4 = dataReader.GetNullableDouble("heating4"),
                            });
                        }

                        dataReader.Close();
                        return metersDtos.ToArray();
                    }
                }
            }
        }

        public RequestForListDto[] GetAlertRequestList(int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("CALL phone_client.get_alert_requests(@userId)", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
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
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                            });
                        }

                        dataReader.Close();
                    }

                    return requests.ToArray();
                }
            }
        }


        public ActiveChannelsDto[] GetActiveChannels(int userId)
        {
            var readedChannels = new List<ActiveChannelsDto>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("CALL phone_client.get_active_calls(@userId)", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    /* @"SELECT UniqueID,Channel,CallerIDNum,ChannelState,AnswerTime,CreateTime,TIMESTAMPDIFF(SECOND,CreateTime,sysdate()) waitSec,ivr_dtmf,
    null as request_id,s.short_name, w.id,w.sur_name,w.first_name,w.patr_name, w.id worker_id,w.sur_name,w.first_name,w.patr_name
    FROM asterisk.ActiveChannels a
    left join CallCenter.ServiceCompanies s on a.ServiceComp = s.trunk_name
    left join CallCenter.Workers w on w.phone = a.PhoneNum and not exists (select 1 from CallCenter.Workers w2 where w2.phone = w.phone and w2.id> w.id)
    where Application = 'queue' and AppData like 'dispetchers%' and BridgeId is null order by UniqueID"
    */
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

        public CallsListDto[] GetCallList(DateTime fromDate, DateTime toDate, string requestId, int? operatorId,
            int? serviceCompanyId, string phoneNumber)
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
                                ? (RequestUserDto) null
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
        public List<RingUpInfoDto> GetRingUpInfo(int userId, int ringUpId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("CALL phone_client.get_ring_up_info(@userId,@ringUpId)", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@ringUpId", ringUpId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var ringUpHistoryDtos = new List<RingUpInfoDto>();
                        while (dataReader.Read())
                        {
                            ringUpHistoryDtos.Add(new RingUpInfoDto
                            {

                                Phone = dataReader.GetNullableString("phone"),
                                LastCallLength = dataReader.GetNullableInt("last_call_length"),
                                LastCallTime = dataReader.GetNullableDateTime("last_call_time"),
                                CalledCount = dataReader.GetNullableInt("called_count"),
                                DoneCalls = dataReader.GetNullableString("done_calls")
                            });
                        }

                        dataReader.Close();
                        return ringUpHistoryDtos;
                    }
                }
            }

        }
        public List<RingUpHistoryDto> GetRingUpHistory(int userId, DateTime fromDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand("CALL phone_client.get_ring_up_history(@userId,@fromDate)", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@fromDate", fromDate);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var ringUpHistoryDtos = new List<RingUpHistoryDto>();
                        while (dataReader.Read())
                        {
                            ringUpHistoryDtos.Add(new RingUpHistoryDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetNullableString("name"),
                                FromPhone = dataReader.GetNullableString("phone"),
                                CallTime = dataReader.GetDateTime("call_time"),
                                StateId = dataReader.GetInt32("state"),
                                PhoneCount = dataReader.GetInt32("record_count"),
                                DoneCalls = dataReader.GetInt32("done_calls"),
                                NotDoneCalls = dataReader.GetInt32("not_done_calls"),
                                StartTime = dataReader.GetNullableDateTime("start_time"),
                                EndTime = dataReader.GetNullableDateTime("end_time"),
                            });
                        }

                        dataReader.Close();
                        return ringUpHistoryDtos;
                    }
                }
            }

        }
        public void AbortRingUp(int userId, int ringUpId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(@"update asterisk.RingUpList set state = 3 where id = @ListId;",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@ListId", ringUpId);
                    cmd.ExecuteNonQuery();
                }
            }

        }
        public void ContinueRingUp(int userId, int ringUpId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(@"update asterisk.RingUpList set state = 1 where id = @ListId;",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@ListId", ringUpId);
                    cmd.ExecuteNonQuery();
                }
            }

        }
        public List<RingUpConfigDto> GetRingUpConfigs(int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var query = "call phone_client.get_ring_up_configs(@userId);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    var configDtos = new List<RingUpConfigDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            configDtos.Add(new RingUpConfigDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Phone = dataReader.GetString("phone")
                            });
                        }

                        dataReader.Close();
                    }

                    return configDtos.OrderBy(i => i.Name).ToList();
                }
            }
        }
        public void SaveRingUpList(int configId, RingUpImportDto[] records)
        {
            int newId;
            if (records.Length == 0)
                return;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(
                    @"insert into asterisk.RingUpList(config_id,call_time,state,exten) select id,sysdate(),2,exten from asterisk.RingUpConfigs a where a.id = @Config;
    select LAST_INSERT_ID();", conn))
                {
                    cmd.Parameters.AddWithValue("@Config", configId);
                    newId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (var item in records)
                {
                    using (var cmd = new MySqlCommand(@"call asterisk.InsertDolgRingPhone(@ListId, @Phone, @Dolg);",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@ListId", newId);
                        cmd.Parameters.AddWithValue("@Phone", item.Phone);
                        cmd.Parameters.AddWithValue("@Dolg", item.Dolg);
                        cmd.ExecuteNonQuery();
                    }
                }

                using (var cmd = new MySqlCommand(@"update asterisk.RingUpList set state = 0 where id = @ListId;",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@ListId", newId);
                    cmd.ExecuteNonQuery();
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

        public void SendAlive(int userId, string sipUser, string version)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"call phone_client.send_alive(@UserId,@Sip,@version)", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Sip", sipUser);
                    cmd.Parameters.AddWithValue("@version", version);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void IncreaseRingCount(int userId, string callerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    "update asterisk.NotAnsweredQueue set call_count = call_count + 1 where CallerIDNum  = @CallerId;";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CallerId", callerId);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public void DeleteCallListRecord(int userId, int recordId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(@"delete from CallCenter.RequestCalls where id = @ID;", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", recordId);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public MeterCodeDto GetMeterCodes(int userId, int addressId)
        {
            var result = new MeterCodeDto();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(
                    "SELECT * FROM CallCenter.MeterDeviceCodes C where address_id = @AddressId", conn))
                {
                    cmd.Parameters.AddWithValue("@AddressId", addressId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            result.Id = dataReader.GetInt32("id");
                            result.AddressId = dataReader.GetInt32("address_id");
                            result.PersonalAccount = dataReader.GetNullableString("personal_account");
                            result.Electro1Code = dataReader.GetNullableString("electro_t1_code");
                            result.Electro2Code = dataReader.GetNullableString("electro_t2_code");
                            result.ColdWater1Code = dataReader.GetNullableString("cool_water1_code");
                            result.HotWater1Code = dataReader.GetNullableString("hot_water1_code");
                            result.ColdWater2Code = dataReader.GetNullableString("cool_water2_code");
                            result.HotWater2Code = dataReader.GetNullableString("hot_water2_code");
                            result.ColdWater3Code = dataReader.GetNullableString("cool_water3_code");
                            result.HotWater3Code = dataReader.GetNullableString("hot_water3_code");
                            result.HeatingCode = dataReader.GetNullableString("heating_code");
                            result.Heating2Code = dataReader.GetNullableString("heating2_code");
                            result.Heating3Code = dataReader.GetNullableString("heating3_code");
                            result.Heating4Code = dataReader.GetNullableString("heating4_code");
                        }

                        dataReader.Close();
                    }
                }

                return result;
            }
        }
        public List<MetersDto> GetMetersByAddressId(int userId, int addressId)
        {
            var sqlQuery = @"select id, meters_date, personal_account, electro_t1, electro_t2, cool_water1, hot_water1, cool_water2, hot_water2, cool_water3, hot_water3, user_id, heating,heating2,heating3,heating4, client_phone_id  from CallCenter.MeterDeviceValues
 where address_id = @AddressId and meters_date > sysdate() - INTERVAL 3 month
 order by meters_date desc";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@AddressId", addressId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var metersDtos = new List<MetersDto>();
                        while (dataReader.Read())
                        {
                            metersDtos.Add(new MetersDto
                            {
                                Id = dataReader.GetInt32("id"),
                                PersonalAccount = dataReader.GetNullableString("personal_account"),
                                Date = dataReader.GetDateTime("meters_date"),
                                Electro1 = dataReader.GetDouble("electro_t1"),
                                Electro2 = dataReader.GetDouble("electro_t2"),
                                ColdWater1 = dataReader.GetDouble("cool_water1"),
                                HotWater1 = dataReader.GetDouble("hot_water1"),
                                ColdWater2 = dataReader.GetDouble("cool_water2"),
                                HotWater2 = dataReader.GetDouble("hot_water2"),
                                ColdWater3 = dataReader.GetDouble("cool_water3"),
                                HotWater3 = dataReader.GetDouble("hot_water3"),
                                Heating = dataReader.GetDouble("heating"),
                                Heating2 = dataReader.GetNullableDouble("heating2"),
                                Heating3 = dataReader.GetNullableDouble("heating3"),
                                Heating4 = dataReader.GetNullableDouble("heating4"),
                            });
                        }

                        dataReader.Close();
                        return metersDtos;
                    }
                }
            }
        }
        public void AddCallToMeter(int userId, int? meterId, string callUniqueId)
        {

            if (!string.IsNullOrEmpty(callUniqueId))
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var cmd =
                        new MySqlCommand(
                            "insert into CallCenter.MeterCalls(meter_id,uniqueID,insert_date) values(@MeterId, @UniqueId,sysdate())",
                            conn))
                    {
                        cmd.Parameters.AddWithValue("@MeterId", meterId.Value);
                        cmd.Parameters.AddWithValue("@UniqueId", callUniqueId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        public List<AlertTypeDto> GetAlertTypes(int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (
                    var cmd = new MySqlCommand("select id,name from CallCenter.AlertType where enabled = 1",
                        conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var alertTypeDtos = new List<AlertTypeDto>();
                        while (dataReader.Read())
                        {
                            alertTypeDtos.Add(new AlertTypeDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                            });
                        }

                        dataReader.Close();
                        return alertTypeDtos;
                    }
                }
            }
        }

        public List<AlertServiceTypeDto> GetAlertServiceTypes(int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (
                    var cmd = new MySqlCommand(
                        "select id,name from CallCenter.AlertServiceType where enabled = 1 order by order_num",
                        conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var serviceTypeDtos = new List<AlertServiceTypeDto>();
                        while (dataReader.Read())
                        {
                            serviceTypeDtos.Add(new AlertServiceTypeDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                            });
                        }

                        dataReader.Close();
                        return serviceTypeDtos;
                    }
                }
            }
        }
        public void SaveAlert(SaveAlertDto alert)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
            if (alert.Id == 0)
            {
 
                    using (var cmd = new MySqlCommand(@"insert into CallCenter.Alerts(house_id, alert_type_id, alert_service_type_id, create_date, create_user_id, start_date, end_date, description)
 values(@HouseId,@TypeId,@ServiceId,sysdate(),@UserId,@StartDate,@EndDate,@Desc);", conn))
                {
                    cmd.Parameters.AddWithValue("@HouseId", alert.HouseId);
                    cmd.Parameters.AddWithValue("@TypeId", alert.Type.Id);
                    cmd.Parameters.AddWithValue("@ServiceId", alert.ServiceType.Id);
                    cmd.Parameters.AddWithValue("@UserId", alert.UserId);
                    cmd.Parameters.AddWithValue("@StartDate", alert.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", alert.EndDate);
                    cmd.Parameters.AddWithValue("@Desc", alert.Description);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (var cmd = new MySqlCommand(@"update CallCenter.Alerts set start_date = @StartDate, end_date = @EndDate, description = @Desc where id = @alertId;", conn))
                {
                    cmd.Parameters.AddWithValue("@alertId", alert.Id);
                    cmd.Parameters.AddWithValue("@StartDate", alert.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", alert.EndDate);
                    cmd.Parameters.AddWithValue("@Desc", alert.Description);
                    cmd.ExecuteNonQuery();
                }
            }
            }
        }


        public void SaveMeterCodes(int userId, int selectedFlatId, string personalAccount, string electro1Code, string electro2Code,
            string hotWater1Code,
            string coldWater1Code, string hotWater2Code, string coldWater2Code, string hotWater3Code,
            string coldWater3Code, string heatingCode, string heating2Code, string heating3Code, string heating4Code)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(
                    @"insert into CallCenter.MeterDeviceCodes(address_id,personal_account, electro_t1_code, electro_t2_code, cool_water1_code, hot_water1_code,
cool_water2_code, hot_water2_code,cool_water3_code, hot_water3_code, heating_code, heating2_code, heating3_code, heating4_code)
values(@addressId,@persCode,@el1code,@el2code,@cw1code,@hw1code,@cw2code,@hw2code,@cw3code,@hw3code,@h1code,@h2code,@h3code,@h4code) on duplicate KEY 
UPDATE personal_account = @persCode, electro_t1_code = @el1code, electro_t2_code = @el2code,cool_water1_code = @cw1code, hot_water1_code = @hw1code,cool_water2_code = @cw2code,
hot_water2_code = @hw2code,cool_water3_code = @cw3code,hot_water3_code = @hw3code, heating_code = @h1code, heating2_code = @h2code, heating3_code = @h3code, heating4_code = @h4code;",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@addressId", selectedFlatId);
                    cmd.Parameters.AddWithValue("@persCode", personalAccount);
                    cmd.Parameters.AddWithValue("@el1code", electro1Code);
                    cmd.Parameters.AddWithValue("@el2code", electro2Code);
                    cmd.Parameters.AddWithValue("@hw1code", hotWater1Code);
                    cmd.Parameters.AddWithValue("@cw1code", coldWater1Code);
                    cmd.Parameters.AddWithValue("@cw2code", coldWater2Code);
                    cmd.Parameters.AddWithValue("@hw2code", hotWater2Code);
                    cmd.Parameters.AddWithValue("@cw3code", coldWater3Code);
                    cmd.Parameters.AddWithValue("@hw3code", hotWater3Code);
                    cmd.Parameters.AddWithValue("@h1code", heatingCode);
                    cmd.Parameters.AddWithValue("@h2code", heating2Code);
                    cmd.Parameters.AddWithValue("@h3code", heating3Code);
                    cmd.Parameters.AddWithValue("@h4code", heating4Code);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int? SaveMeterValues(int userId, string phoneNumber, int addressId, double electro1, double electro2, double hotWater1, double coldWater1, double hotWater2, double coldWater2, double hotWater3, double coldWater3, double heating, int? meterId, string personalAccount, double heating2, double heating3, double heating4)
        {
            var result = meterId;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    var clientPhoneId = (int?) null;
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        using (var cmd = new MySqlCommand(
                            "SELECT id FROM CallCenter.ClientPhones C where Number = @Phone", conn))
                        {
                            cmd.Parameters.AddWithValue("@Phone", phoneNumber);

                            using (var dataReader = cmd.ExecuteReader())
                            {
                                if (dataReader.Read())
                                {
                                    clientPhoneId = dataReader.GetInt32("id");
                                }

                                dataReader.Close();
                            }
                        }

                        if (clientPhoneId == 0)
                        {
                            using (
                                var cmd = new MySqlCommand(@"insert into CallCenter.ClientPhones(Number) values(@Phone);
    select LAST_INSERT_ID();", conn))
                            {
                                cmd.Parameters.AddWithValue("@Phone", phoneNumber);
                                clientPhoneId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }
                    }

                    if (meterId.HasValue)
                    {
                        using (var cmd = new MySqlCommand(
                            "update CallCenter.MeterDeviceValues" +
                            " set electro_t1 = @Electro1, electro_t2 = @Electro2, cool_water1 = @Cool1," +
                            " hot_water1 = @Hot1, cool_water2 = @Cool2, hot_water2 = @Hot2 , cool_water3 = @Cool3, hot_water3 = @Hot3, heating = @Heating," +
                            " personal_account = @PersonalAccount, heating2 = @Heating2, heating3 = @Heating3, heating4 = @Heating4" +
                            " where id = @MeterId",
                            conn))
                        {

                            cmd.Parameters.AddWithValue("@PersonalAccount", personalAccount);
                            cmd.Parameters.AddWithValue("@Electro1", electro1);
                            cmd.Parameters.AddWithValue("@Electro2", electro2);
                            cmd.Parameters.AddWithValue("@Cool1", coldWater1);
                            cmd.Parameters.AddWithValue("@Cool2", coldWater2);
                            cmd.Parameters.AddWithValue("@Cool3", coldWater3);
                            cmd.Parameters.AddWithValue("@Hot1", hotWater1);
                            cmd.Parameters.AddWithValue("@Hot2", hotWater2);
                            cmd.Parameters.AddWithValue("@Hot3", hotWater3);
                            cmd.Parameters.AddWithValue("@Heating", heating);
                            cmd.Parameters.AddWithValue("@Heating2", heating2);
                            cmd.Parameters.AddWithValue("@Heating3", heating3);
                            cmd.Parameters.AddWithValue("@Heating4", heating4);
                            cmd.Parameters.AddWithValue("@MeterId", meterId.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd = new MySqlCommand(
                            "insert into CallCenter.MeterDeviceValues(address_id, meters_date, electro_t1, electro_t2, cool_water1, hot_water1, cool_water2, hot_water2, cool_water3, hot_water3, user_id, heating, client_phone_id,personal_account , heating2, heating3, heating4 )" +
                            " values(@AddressId,sysdate(),@Electro1,@Electrio2,@Cool1,@Hot1,@Cool2,@Hot2,@Cool3,@Hot3,@UserId,@Heating,@ClentPhoneId,@PersonalAccount,@Heating2,@Heating3,@Heating4)",
                            conn))
                        {

                            cmd.Parameters.AddWithValue("@PersonalAccount", personalAccount);
                            cmd.Parameters.AddWithValue("@AddressId", addressId);
                            cmd.Parameters.AddWithValue("@Electro1", electro1);
                            cmd.Parameters.AddWithValue("@Electrio2", electro2);
                            cmd.Parameters.AddWithValue("@Cool1", coldWater1);
                            cmd.Parameters.AddWithValue("@Cool2", coldWater2);
                            cmd.Parameters.AddWithValue("@Cool3", coldWater3);
                            cmd.Parameters.AddWithValue("@Hot1", hotWater1);
                            cmd.Parameters.AddWithValue("@Hot2", hotWater2);
                            cmd.Parameters.AddWithValue("@Hot3", hotWater3);
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@Heating", heating);
                            cmd.Parameters.AddWithValue("@Heating2", heating2);
                            cmd.Parameters.AddWithValue("@Heating3", heating3);
                            cmd.Parameters.AddWithValue("@Heating4", heating4);
                            cmd.Parameters.AddWithValue("@ClentPhoneId", clientPhoneId);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new MySqlCommand("SELECT LAST_INSERT_ID() as id", conn))
                        {
                            using (var dataReader = cmd.ExecuteReader())
                            {
                                if (dataReader.Read())
                                {
                                    result = dataReader.GetInt32("id");
                                }

                                dataReader.Close();
                            }
                        }
                    }

                    transaction.Commit();
                }

                return result;
            }
        }
        public string GetRecordFileNameByUniqueId(int userId, string uniqueId)
        {
            var sqlQuery = @"select MonitorFile FROM asterisk.ChannelHistory ch
 join CallCenter.RequestCalls rc on ch.UniqueId = rc.UniqueId
 where rc.uniqueID = @UniqueID";
            var result = string.Empty;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@UniqueID", uniqueId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            result = dataReader.GetNullableString("MonitorFile");
                        }

                        dataReader.Close();
                        return result;
                    }
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

        public List<StatusDto> GetRequestStatuses(int userId)
        {
            var query = "SELECT id, name, Description FROM CallCenter.RequestState R order by Description";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
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

                    return types;
                }
            }
        }

        public List<NoteDto> GetNotes(int userId, int requestId)
        {
            return GetNotesCore(requestId).OrderByDescending(n => n.Date).ToList();
        }

        public List<NoteDto> GetNotesCore(int requestId)
        {
            var sqlQuery =
                @"SELECT n.id,n.operation_date,n.request_id,n.user_id,n.note,n.worker_id,u.SurName,u.FirstName,u.PatrName,w.sur_name,w.first_name,w.patr_name
    from CallCenter.RequestNoteHistory n
    join CallCenter.Users u on u.id = n.user_id
    left join CallCenter.Workers w on w.id = n.worker_id where request_id = @RequestId order by operation_date";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var noteList = new List<NoteDto>();
                        RequestUserDto user;
                        while (dataReader.Read())
                        {
                            var workerId = dataReader.GetNullableInt("worker_id");
                            if (workerId.HasValue)
                            {
                                user = new RequestUserDto()
                                {
                                    Id = workerId.Value,
                                    SurName = dataReader.GetNullableString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                };
                            }
                            else
                            {
                                user = new RequestUserDto
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("SurName"),
                                    FirstName = dataReader.GetNullableString("FirstName"),
                                    PatrName = dataReader.GetNullableString("PatrName"),
                                };

                            }

                            noteList.Add(new NoteDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Date = dataReader.GetDateTime("operation_date"),
                                Note = dataReader.GetNullableString("note"),
                                User = user
                            });
                        }

                        dataReader.Close();
                        return noteList;
                    }
                }
            }

        }

        public List<WorkerHistoryDto> GetMasterHistoryByRequest(int userId, int requestId)
        {
            var query =
                @"SELECT operation_date, R.worker_id, w.sur_name,w.first_name,w.patr_name, user_id,u.surname,u.firstname,u.patrname FROM CallCenter.RequestWorkerHistory R
 left join CallCenter.Workers w on w.id = R.worker_id
 join CallCenter.Users u on u.id = user_id
 where request_id = @RequestId";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(query, conn))
                {
                    var historyDtos = new List<WorkerHistoryDto>();
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var workerId = dataReader.GetNullableInt("worker_id");
                            historyDtos.Add(new WorkerHistoryDto
                            {
                                CreateTime = dataReader.GetDateTime("operation_date"),
                                Worker = workerId != null
                                    ? new RequestUserDto
                                    {
                                        Id = dataReader.GetInt32("worker_id"),
                                        SurName = dataReader.GetNullableString("sur_name"),
                                        FirstName = dataReader.GetNullableString("first_name"),
                                        PatrName = dataReader.GetNullableString("patr_name"),
                                    }
                                    : new RequestUserDto {Id = -1, SurName = "Нет мастера"},
                                CreateUser = new RequestUserDto
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("surname"),
                                    FirstName = dataReader.GetNullableString("firstname"),
                                    PatrName = dataReader.GetNullableString("patrname"),
                                },
                            });
                        }

                        dataReader.Close();
                    }

                    return historyDtos.OrderByDescending(i => i.CreateTime).ToList();
                }
            }
        }

        public List<WorkerHistoryDto> GetExecutorHistoryByRequest(int userId, int requestId)
        {
            var query =
                @"SELECT operation_date, R.executer_id, w.sur_name,w.first_name,w.patr_name, user_id,u.surname,u.firstname,u.patrname FROM CallCenter.RequestExecuterHistory R
 left join CallCenter.Workers w on w.id = R.executer_id
 join CallCenter.Users u on u.id = user_id
 where request_id = @RequestId";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var historyDtos = new List<WorkerHistoryDto>();
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var executerId = dataReader.GetNullableInt("executer_id");
                            historyDtos.Add(new WorkerHistoryDto
                            {
                                CreateTime = dataReader.GetDateTime("operation_date"),
                                Worker = executerId != null
                                    ? new RequestUserDto
                                    {
                                        Id = dataReader.GetInt32("executer_id"),
                                        SurName = dataReader.GetNullableString("sur_name"),
                                        FirstName = dataReader.GetNullableString("first_name"),
                                        PatrName = dataReader.GetNullableString("patr_name"),
                                    }
                                    : new RequestUserDto {Id = -1, SurName = "Нет исполнителя"},
                                CreateUser = new RequestUserDto
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("surname"),
                                    FirstName = dataReader.GetNullableString("firstname"),
                                    PatrName = dataReader.GetNullableString("patrname"),
                                },
                            });
                        }

                        dataReader.Close();
                    }

                    return historyDtos.OrderByDescending(i => i.CreateTime).ToList();
                }
            }
        }

        public List<StatusHistoryDto> GetStatusHistoryByRequest(int userId, int requestId)
        {
            var query = @"SELECT operation_date, R.state_id, s.name, s.description,
    case when user_id = 0 then cw.id else user_id end user_id,
    case when user_id = 0 then cw.sur_name else u.surname end surname,
    case when user_id = 0 then cw.first_name else u.firstname end firstname,
    case when user_id = 0 then cw.patr_name else u.patrname end patrname
FROM CallCenter.RequestStateHistory R
 join CallCenter.RequestState s on s.id = R.state_id
 join CallCenter.Users u on u.id = user_id
 left join CallCenter.Workers cw on cw.id = R.worker_id
 where request_id = @RequestId";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(query, conn))
                {
                    var historyDtos = new List<StatusHistoryDto>();
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            historyDtos.Add(new StatusHistoryDto
                            {
                                CreateTime = dataReader.GetDateTime("operation_date"),
                                Status = new StatusDto
                                {
                                    Id = dataReader.GetInt32("state_id"),
                                    Name = dataReader.GetNullableString("name"),
                                    Description = dataReader.GetNullableString("description"),
                                },
                                CreateUser = new RequestUserDto
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("surname"),
                                    FirstName = dataReader.GetNullableString("firstname"),
                                    PatrName = dataReader.GetNullableString("patrname"),
                                },
                            });
                        }

                        dataReader.Close();
                    }

                    return historyDtos.OrderByDescending(i => i.CreateTime).ToList();
                }
            }
        }

        public void AddNewState(int userId, int requestId, int stateId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        using (
                            var cmd =
                                new MySqlCommand(
                                    @"insert into CallCenter.RequestStateHistory (request_id,operation_date,user_id,state_id) 
    values(@RequestId,sysdate(),@UserId,@StatusId);", conn))
                        {
                            cmd.Parameters.AddWithValue("@RequestId", requestId);
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@StatusId", stateId);
                            cmd.ExecuteNonQuery();
                        }

                        using (
                            var cmd =
                                new MySqlCommand(
                                    @"update CallCenter.Requests set state_id = @StatusId where id = @RequestId",
                                    conn))
                        {
                            cmd.Parameters.AddWithValue("@RequestId", requestId);
                            cmd.Parameters.AddWithValue("@StatusId", stateId);
                            cmd.ExecuteNonQuery();
                        }

                        if (stateId == 3)
                        {
                            using (
                                var cmd =
                                    new MySqlCommand(
                                        @"update CallCenter.Requests set close_date = sysdate() where close_date is null and id = @RequestId",
                                        conn))
                            {
                                cmd.Parameters.AddWithValue("@RequestId", requestId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc.ToString());
                throw;
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

        public string GetUniqueIdByCallId(int userId, string callId)
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
                        if (dataReader.Read())
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

        public List<CallsListDto> GetCallListByRequestId(int userId, int requestId)
        {
            var sqlQuery =
                @"SELECT rc.id,ch.UniqueID,Direction,PhoneNum CallerIDNum,CreateTime,AnswerTime,EndTime,BridgedTime,
 MonitorFile, timestampdiff(SECOND,ch.BridgedTime,ch.EndTime) AS TalkTime,
(timestampdiff(SECOND,ch.CreateTime,ch.EndTime) - ifnull(timestampdiff(SECOND,ch.BridgedTime,ch.EndTime),0)) AS WaitingTime,
 group_concat(rc.request_id order by rc.request_id separator ', ') AS RequestId
 FROM asterisk.ChannelHistory ch
 join CallCenter.RequestCalls rc on ch.UniqueId = rc.UniqueId
 where rc.request_id = @RequestNum
 group by UniqueId";
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@RequestNum", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var callList = new List<CallsListDto>();
                        while (dataReader.Read())
                        {
                            callList.Add(new CallsListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                UniqueId = dataReader.GetNullableString("UniqueID"),
                                CallerId = dataReader.GetNullableString("CallerIDNum"),
                                Direction = dataReader.GetNullableString("Direction"),
                                AnswerTime = dataReader.GetNullableDateTime("AnswerTime"),
                                CreateTime = dataReader.GetNullableDateTime("CreateTime"),
                                BridgedTime = dataReader.GetNullableDateTime("BridgedTime"),
                                EndTime = dataReader.GetNullableDateTime("EndTime"),
                                TalkTime = dataReader.GetNullableInt("TalkTime"),
                                WaitingTime = dataReader.GetNullableInt("WaitingTime"),
                                MonitorFileName = dataReader.GetNullableString("MonitorFile"),
                                Requests = dataReader.GetNullableString("RequestId"),
                                User = null
                            });
                        }

                        dataReader.Close();
                        return callList;
                    }
                }
            }
        }

        public List<SmsListDto> GetSmsByRequestId(int userId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd =
                    new MySqlCommand(
                        "select id,sender,phone,message,create_date,state_desc, is_client,price*sms_count price from CallCenter.SMSRequest where request_id = @RequestId order by id",
                        conn))
                {
                    cmd.Parameters.AddWithValue("@RequestId", requestId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var alertTypeDtos = new List<SmsListDto>();
                        while (dataReader.Read())
                        {
                            alertTypeDtos.Add(new SmsListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Sender = dataReader.GetNullableString("sender"),
                                SendTime = dataReader.GetDateTime("create_date"),
                                Phone = dataReader.GetNullableString("phone"),
                                Message = dataReader.GetNullableString("message"),
                                State = dataReader.GetNullableString("state_desc"),
                                Price = dataReader.GetNullableDouble("price"),
                                ClientOrWorker = dataReader.GetBoolean("is_client") ? "Жилец" : "Испол."
                            });
                        }

                        dataReader.Close();
                        return alertTypeDtos;
                    }
                }
            }

        }

        public void DeleteAttachment(int userId, int attachmentId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd =
                    new MySqlCommand(@"update CallCenter.RequestAttachments set deleted = 1 where id = @attachId;",
                        conn))
                {
                    cmd.Parameters.AddWithValue("@attachId", attachmentId);
                    cmd.ExecuteNonQuery();
                }

            }
        }

        public void AddAttachmentToRequest(int userId, int requestId, string fileName, string name = "")
        {
            if (!File.Exists(fileName))
                return;
            if (string.IsNullOrEmpty(name))
                name = Path.GetFileName(fileName);
            var fileExtension = Path.GetExtension(fileName);
            string newFile;
            using (var fileStream = File.OpenRead(fileName))
            {
                newFile = SaveFile(requestId, fileExtension, fileStream);
            }

            AttachFileToRequest(userId, requestId, name, newFile);
        }

        public void AttachFileToRequest(int userId, int requestId, string fileName, string generatedFileName)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(
                            @"insert into CallCenter.RequestAttachments(request_id,name,file_name,create_date,user_id)
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

        public string SaveFile(int requestId, string fileName, Stream fileStream)
        {


            //using (var saveService = new WcfSaveService.SaveServiceClient())
            //{
            //    return saveService.UploadFile(FileName: fileName, RequestId: requestId, FileStream: fileStream);
            //}
            return null;
        }

        public static byte[] DownloadFile(int requestId, string fileName, string rootDir)
        {
            if (!string.IsNullOrEmpty(rootDir) && Directory.Exists($"{rootDir}\\{requestId}"))
            {
                return File.ReadAllBytes($"{rootDir}\\{requestId}\\{fileName}");
            }

            return null;
        }

        public byte[] GetFile(int requestId, string fileName)
        {
            //using (var saveService = new WcfSaveService.SaveServiceClient())
            //{
            //    return saveService.DownloadFile(requestId, fileName);
            //}
            return null;
        }
    }
}
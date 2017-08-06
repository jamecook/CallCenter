﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using NLog;
using RequestServiceImpl.Dto;

namespace RequestServiceImpl
{
    public class RequestService
    {
        private static Logger _logger;
        private MySqlConnection _dbConnection;

        public RequestService(MySqlConnection dbConnection)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _dbConnection = dbConnection;
        }

        public int? SaveNewRequest(int addressId, int requestTypeId, ContactDto[] contactList, string requestMessage,
            bool chargeable, bool immediate, string callUniqueId, string entrance, string floor)
        {
            int newId;
            _logger.Debug(
                $"RequestService.SaveNewRequest({addressId},{requestTypeId},[{contactList.Select(x => $"{x.PhoneNumber}").Aggregate((f1, f2) => f1 + ";" + f2)}],{requestMessage},{chargeable},{immediate},{callUniqueId})");
            try
            {

                using (var transaction = _dbConnection.BeginTransaction())
                {
                    #region Сохранение заявки в базе данных

                    using (
                        var cmd = new MySqlCommand(
                            @"insert into CallCenter.Requests(address_id,type_id,description,create_time,is_chargeable,create_user_id,state_id,is_immediate, entrance, floor)
values(@AddressId, @TypeId, @Message, sysdate(),@IsChargeable,@UserId,@State,@IsImmediate,@Entrance,@Floor);
select LAST_INSERT_ID();", _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@AddressId", addressId);
                        cmd.Parameters.AddWithValue("@TypeId", requestTypeId);
                        cmd.Parameters.AddWithValue("@Message", requestMessage);
                        cmd.Parameters.AddWithValue("@IsChargeable", chargeable);
                        cmd.Parameters.AddWithValue("@IsImmediate", immediate);
                        cmd.Parameters.AddWithValue("@Entrance", entrance);
                        cmd.Parameters.AddWithValue("@Floor", floor);
                        cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                        cmd.Parameters.AddWithValue("@State", 1);
                        newId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    #endregion
                    #region Прикрепление звонка к заявке

                    if (!string.IsNullOrEmpty(callUniqueId))
                    {
                        using (var cmd =
                                new MySqlCommand("insert into CallCenter.RequestCalls(request_id,uniqueID) values(@Request, @UniqueId)", _dbConnection))
                        {
                            cmd.Parameters.AddWithValue("@Request", newId);
                            cmd.Parameters.AddWithValue("@UniqueId", callUniqueId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    #endregion

                    #region Сохранение контактных номеров 

                    foreach (
                        var contact in
                            contactList.Where(c => !string.IsNullOrEmpty(c.PhoneNumber))
                                .OrderByDescending(c => c.IsMain))
                    {
                        var clientPhoneId = 0;
                        using (
                            var cmd = new MySqlCommand(
                                "SELECT id FROM CallCenter.ClientPhones C where Number = @Phone", _dbConnection))
                        {
                            cmd.Parameters.AddWithValue("@Phone", contact.PhoneNumber);

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
                                var cmd = new MySqlCommand(@"insert into CallCenter.ClientPhones(Number,sur_name,first_name,patr_name) values(@Phone,@SurName,@FirstName,@PatrName);
    select LAST_INSERT_ID();", _dbConnection))
                            {
                                cmd.Parameters.AddWithValue("@Phone", contact.PhoneNumber);
                                cmd.Parameters.AddWithValue("@SurName", contact.SurName);
                                cmd.Parameters.AddWithValue("@FirstName", contact.FirstName);
                                cmd.Parameters.AddWithValue("@PatrName", contact.PatrName);
                                clientPhoneId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }
                        else
                        {
                            using (
    var cmd = new MySqlCommand(@"update CallCenter.ClientPhones set sur_name = @SurName,first_name = @FirstName,patr_name = @PatrName where Number = @Phone;", _dbConnection))
                            {
                                cmd.Parameters.AddWithValue("@Phone", contact.PhoneNumber);
                                cmd.Parameters.AddWithValue("@SurName", contact.SurName);
                                cmd.Parameters.AddWithValue("@FirstName", contact.FirstName);
                                cmd.Parameters.AddWithValue("@PatrName", contact.PatrName);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        using (
                            var cmd =
                                new MySqlCommand(@"insert into CallCenter.RequestContacts (request_id,IsMain,ClientPhone_id) 
    values(@RequestId,@IsMain,@PhoneId);", _dbConnection))
                        {
                            cmd.Parameters.AddWithValue("@RequestId", newId);
                            cmd.Parameters.AddWithValue("@IsMain", contact.IsMain);
                            cmd.Parameters.AddWithValue("@PhoneId", clientPhoneId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    #endregion

                    #region Сохрарнение описания в истории изменений

                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestDescriptionHistory (request_id,operation_date,user_id,description) 
    values(@RequestId,sysdate(),@UserId,@Message);", _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", newId);
                        cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                        cmd.Parameters.AddWithValue("@Message", requestMessage);
                        cmd.ExecuteNonQuery();
                    }

                    #endregion

                    transaction.Commit();
                    return newId;
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
            }
            return null;
        }

        public byte[] GetMeiaByRequestId(int requestId)
        {
            using (var cmd =
                    new MySqlCommand(@"SELECT MonitorFile FROM asterisk.CallsHistory C where RequestId = @reqId", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@reqId", requestId);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        var fileName = dataReader.GetNullableString("MonitorFile");
                        var serverIpAddress = _dbConnection.DataSource;
                        var localFileName = fileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\");
                        return File.ReadAllBytes(localFileName);
                    }
                }
            }
            return new byte[0];
        }

        public void AddNewDescription(int requestId, string requestMessage)
        {
            _logger.Debug($"RequestService.AddNewDescription({requestId},{requestMessage})");
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestDescriptionHistory (request_id,operation_date,user_id,description) 
    values(@RequestId,sysdate(),@UserId,@Message);", _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                        cmd.Parameters.AddWithValue("@Message", requestMessage);
                        cmd.ExecuteNonQuery();
                    }
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"update CallCenter.Requests set description = @Message where id = @RequestId",
                                _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@Message", requestMessage);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
                throw;
            }
        }

        public void AddNewWorker(int requestId, int workerId)
        {
            _logger.Debug($"RequestService.AddNewWorker({requestId},{workerId})");
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestWorkerHistory (request_id,operation_date,user_id,worker_id) 
    values(@RequestId,sysdate(),@UserId,@WorkerId);", _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.ExecuteNonQuery();
                    }
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"update CallCenter.Requests set worker_id = @WorkerId where id = @RequestId",
                                _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
                throw;
            }

        }

        public void AddNewState(int requestId, int stateId)
        {
            _logger.Debug($"RequestService.AddNewState({requestId},{stateId})");
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestStateHistory (request_id,operation_date,user_id,state_id) 
    values(@RequestId,sysdate(),@UserId,@StatusId);", _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                        cmd.Parameters.AddWithValue("@StatusId", stateId);
                        cmd.ExecuteNonQuery();
                    }
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"update CallCenter.Requests set state_id = @StatusId where id = @RequestId",
                                _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@StatusId", stateId);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
                throw;
            }

        }

        public void SetRating(int requestId, int ratingId, string description)
        {
            _logger.Debug($"RequestService.SetRating({requestId},{ratingId},{description})");
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestRating(request_id,create_date,rating_id,Description,user_id)
 values(@RequestId,sysdate(),@RatingId,@Desc,@UserId);", _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                        cmd.Parameters.AddWithValue("@RatingId", ratingId);
                        cmd.Parameters.AddWithValue("@Desc", description);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
                throw;
            }

        }
        public void AddNewExecuteDate(int requestId, DateTime executeDate, PeriodDto period, string note)
        {
            _logger.Debug($"RequestService.AddNewExecuteDate({requestId},{executeDate},{period.Id},{note})");
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestExecuteDateHistory (request_id,operation_date,user_id,execute_date,period_time_id,note) 
    values(@RequestId,sysdate(),@UserId,@ExecuteDate,@Period,@Note);", _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                        cmd.Parameters.AddWithValue("@ExecuteDate", executeDate);
                        cmd.Parameters.AddWithValue("@Note", note);
                        cmd.Parameters.AddWithValue("@Period", period.Id);
                        cmd.ExecuteNonQuery();
                    }
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"update CallCenter.Requests set execute_date = @ExecuteDate, period_time_id = @Period  where id = @RequestId",
                                _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@ExecuteDate", executeDate + period.SetTime.TimeOfDay);
                        cmd.Parameters.AddWithValue("@Period", period.Id);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
                throw;
            }
        }

        public string GetActiveCallUniqueId()
        {
            string retVal = null;
            var query = @"SELECT case when A.MonitorFile is null then A2.UniqueId else A.UniqueId end uniqueId FROM asterisk.ActiveChannels A
 left join asterisk.ActiveChannels A2 on A2.BridgeId = A.BridgeId and A2.UniqueID<> A.UniqueID
 where A.UserID = @UserId";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        retVal = dataReader.GetNullableString("uniqueId");
                    }
                    dataReader.Close();
                }
                return retVal;
            }
        }

        public void AddNewNote(int requestId, string note)
        {
            _logger.Debug($"RequestService.AddNewNote({requestId},{note})");
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestNoteHistory (request_id,operation_date,user_id,note) 
    values(@RequestId,sysdate(),@UserId,@Note);", _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                        cmd.Parameters.AddWithValue("@Note", note);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
                throw;
            }

        }

        public RequestInfoDto GetRequest(int requestId)
        {
            _logger.Debug($"RequestService.GetRequest({requestId})");

            RequestInfoDto result = null;
            try
            {
                using (var cmd =
                    new MySqlCommand(@"SELECT R.id req_id,R.Address_id,R.type_id,R.description, R.create_time,R.is_chargeable,R.is_immediate,R.period_time_id,R.Create_user_id,R.state_id,R.worker_id,R.execute_date,
    RS.name state_name,RS.description state_descript,
    RT.parrent_id,RT.name as rt_name,RT2.name rt_parrent_name,
    A.type_id address_type_id,A.house_id,A.flat,
    AT.Name type_name,
    H.street_id,H.building,H.corps,H.service_company_id,H.region_id,
    S.name street_name,S.prefix_id,S.city_id,
    SP.Name prefix_name,
    C.name City_name,
    U.SurName,U.FirstName,U.PatrName,
    entrance,floor,
    rtype.rating_id,rating.name RatingName,rtype.Description RatingDesc
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
    join CallCenter.Users U on U.id = R.Create_user_id
    left join CallCenter.RequestRating rtype on rtype.request_id = R.id
    left join CallCenter.RatingTypes rating on rtype.rating_id = rating.id
    where R.id = @reqId", _dbConnection))
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
                                ExecutorId = dataReader.GetNullableInt("worker_id"),
                                IsChargeable = dataReader.GetBoolean("is_chargeable"),
                                IsImmediate = dataReader.GetBoolean("is_immediate"),
                                Description = dataReader.GetNullableString("description"),
                                Entrance = dataReader.GetNullableString("entrance"),
                                Floor = dataReader.GetNullableString("floor"),
                                ExecuteDate = dataReader.GetNullableDateTime("execute_date"),
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
                                Rating = dataReader.GetNullableInt("rating_id").HasValue ? new RequestRatingDto
                                {
                                    Id = dataReader.GetInt32("rating_id"),
                                    Name = dataReader.GetString("RatingName"),
                                    Description = dataReader.GetNullableString("RatingDesc")
                                } : new RequestRatingDto()
                            };
                        }
                        dataReader.Close();
                        if (result != null)
                        {
                            var contactInfo = new List<ContactDto>();
                            using (
                                var contact =
                                    new MySqlCommand(
                                        @"SELECT R.id, IsMain,Number,sur_name,first_name,patr_name from CallCenter.RequestContacts R
    join CallCenter.ClientPhones P on P.id = R.ClientPhone_id where request_id = @reqId order by IsMain desc",
                                        _dbConnection))
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
                                            SurName = contactReader.GetNullableString("sur_name"),
                                            FirstName = contactReader.GetNullableString("first_name"),
                                            PatrName = contactReader.GetNullableString("patr_name"),
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
            catch (Exception exc)
            {
                _logger.Error(exc);
            }
            return null;
        }

        public IList<RequestForListDto> GetRequestList(string requestId, bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int? streetId, int? houseId, int? addressId, int? parentServiceId, int? serviceId, int? statusId, int? workerId)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            var sqlQuery =
                @"SELECT R.id,R.create_time,sp.name as prefix_name,s.name as street_name,h.building,h.corps,at.Name address_type, a.flat,
    R.worker_id, w.sur_name,w.first_name,w.patr_name, create_user_id,u.surname,u.firstname,u.patrname,
    R.execute_date,p.Name Period_Name, R.description,rt.name service_name, rt2.name parent_name, group_concat(cp.Number order by rc.IsMain desc separator ', ') client_phones,
    rating.Name Rating,
    RS.Description Req_Status
    FROM CallCenter.Requests R
    join CallCenter.RequestState RS on RS.id = R.state_id
    join CallCenter.Addresses a on a.id = R.address_id
    join CallCenter.AddressesTypes at on at.id = a.type_id
    join CallCenter.Houses h on h.id = house_id
    join CallCenter.Streets s on s.id = street_id
    join CallCenter.StreetPrefixes sp on sp.id = s.prefix_id
    join CallCenter.RequestTypes rt on rt.id = R.type_id
    join CallCenter.RequestTypes rt2 on rt2.id = rt.parrent_id
    left join CallCenter.Workers w on w.id = R.worker_id
    left join CallCenter.RequestContacts rc on rc.request_id = R.id
    left join CallCenter.ClientPhones cp on cp.id = rc.clientPhone_id
    join CallCenter.Users u on u.id = create_user_id
    left join CallCenter.PeriodTimes p on p.id = R.period_time_id
    left join CallCenter.RequestRating rtype on rtype.request_id = R.id
    left join CallCenter.RatingTypes rating on rtype.rating_id = rating.id";
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
                if (streetId.HasValue)
                    sqlQuery += $" and s.id = {streetId.Value}";
                if (houseId.HasValue)
                    sqlQuery += $" and h.id = {houseId.Value}";
                if (addressId.HasValue)
                    sqlQuery += $" and a.id = {addressId.Value}";
                if (serviceId.HasValue)
                    sqlQuery += $" and rt.id = {serviceId.Value}";
                if (parentServiceId.HasValue)
                    sqlQuery += $" and rt2.id = {parentServiceId.Value}";
                if (statusId.HasValue)
                    sqlQuery += $" and R.state_id = {statusId.Value}";
                if (workerId.HasValue)
                    sqlQuery += $" and w.id = {workerId.Value}";
            }
            else
            {
                sqlQuery += " where R.id = @RequestId";
            }
            sqlQuery += " group by R.id order by id desc";
            using (var cmd =
                new MySqlCommand(sqlQuery, _dbConnection))
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
                            Worker = dataReader.GetNullableInt("worker_id") != null ? new RequestUserDto
                            {
                                Id = dataReader.GetInt32("worker_id"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                            } : null,
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
                            Status = dataReader.GetNullableString("Req_Status"),
                        });
                    }
                    dataReader.Close();
                }
                return requests;
            }
        }

        public IList<WorkerDto> GetWorkers(int? serviceCompanyId)
        {
            var query = @"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,sp.name speciality_name FROM CallCenter.Workers w
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id
    left join CallCenter.Speciality sp on sp.id = w.speciality_id
    where w.enabled = 1";
            query += serviceCompanyId.HasValue ? " and service_company_id = " + serviceCompanyId : "";
            query += " order by sur_name,first_name,patr_name";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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
                        });
                    }
                    dataReader.Close();
                }
                return workers;
            }
        }
        public WorkerDto GetWorkerById(int workerId)
        {
            WorkerDto worker = null;
            var query = @"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id FROM CallCenter.Workers w
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id   
    where w.enabled = 1 and w.id = @WorkerId";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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
                            SpecialityId = dataReader.GetNullableInt("speciality_id"),
                            Phone = dataReader.GetNullableString("phone"),
                        };
                    }
                    dataReader.Close();
                }
                return worker;
            }
        }
        public IList<CityDto> GetCities()
        {
            using (var cmd = new MySqlCommand(@"select id,name from CallCenter.Cities where enabled = 1", _dbConnection)
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
                return cities;
            }
        }
        public IList<StreetPrefixDto> GetStreetPrefixes()
        {
            using (
                var cmd =
                    new MySqlCommand(@"SELECT P.id as Prefix_id,P.Name as Prefix_Name,P.ShortName FROM CallCenter.StreetPrefixes P", _dbConnection))
            {
                var streetPrefixes = new List<StreetPrefixDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        streetPrefixes.Add( new StreetPrefixDto
                            {
                                Id = dataReader.GetInt32("Prefix_id"),
                                Name = dataReader.GetString("Prefix_Name"),
                                ShortName = dataReader.GetString("ShortName")
                            }
                        );
                    }
                    dataReader.Close();
                }
                return streetPrefixes;
            }
        }


        public IList<StreetDto> GetStreets(int cityId)
        {
            using (
                var cmd =
                    new MySqlCommand(@"SELECT S.id,S.city_id,S.name,P.id as Prefix_id,P.Name as Prefix_Name,P.ShortName FROM CallCenter.Streets S
    join CallCenter.StreetPrefixes P on P.id = S.prefix_id
    where S.enabled = 1;", _dbConnection))
            {
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
                return streets;
            }
        }

        public IList<HouseDto> GetHouses(int streetId)
        {
            using (
                var cmd =
                    new MySqlCommand(
                        @"SELECT h.id,h.street_id,h.building,h.corps,h.entrance_count,h.flat_count,h.floor_count,service_company_id,s.Name service_company_name FROM CallCenter.Houses h
    left join CallCenter.ServiceCompanies s on s.id = h.service_company_id where h.street_id = @StreetId and h.enabled = 1;",
                        _dbConnection))
            {
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
                            StreetId = dataReader.GetInt32("street_id"),
                            Corpus = dataReader.GetNullableString("corps"),
                            ServiceCompanyId = dataReader.GetNullableInt("service_company_id"),
                            ServiceCompanyName = dataReader.GetNullableString("service_company_name"),
                            EntranceCount = dataReader.GetNullableInt("entrance_count"),
                            FlatCount = dataReader.GetNullableInt("flat_count"),
                            FloorCount = dataReader.GetNullableInt("floor_count"),
                        });
                    }
                    dataReader.Close();
                }
                return houses;
            }
        }

        public IList<FlatDto> GetFlats(int houseId)
        {
            using (var cmd = new MySqlCommand(@"SELECT A.id,A.type_id,A.flat,T.Name FROM CallCenter.Addresses A
    join CallCenter.AddressesTypes T on T.id = A.type_id
    where A.enabled = true and A.house_id = @HouseId", _dbConnection))
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
                return flats;
            }
        }
        public int? GetServiceCompany(int houseId)
        {
            int? retVal = null;
            using (var cmd = new MySqlCommand(@"SELECT service_company_id FROM CallCenter.Houses H where id = @HouseId", _dbConnection))
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

        public IList<ServiceDto> GetServices(long? parentId)
        {
            var query = parentId.HasValue
                ? @"SELECT id,name FROM CallCenter.RequestTypes R where parrent_id = @ParentId and enabled = 1 order by name"
                : @"SELECT id,name FROM CallCenter.RequestTypes R where parrent_id is null and enabled = 1 order by name";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                if (parentId.HasValue)
                    cmd.Parameters.AddWithValue("@ParentId", parentId.Value);

                var services = new List<ServiceDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        services.Add(new ServiceDto
                        {
                            Id = dataReader.GetInt32("id"),
                            Name = dataReader.GetString("name")
                        });
                    }
                    dataReader.Close();
                }
                return services;
            }
        }

        public List<AddressTypeDto> GetAddressTypes()
        {
            var query = "SELECT id,Name FROM CallCenter.AddressesTypes A order by OrderNum";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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
                return types;
            }
        }

        public List<StatusDto> GetRequestStatuses()
        {
            var query = "SELECT id, name, Description FROM CallCenter.RequestState R order by Description";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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

        public List<RequestRatingDto> GetRequestRating()
        {
            var query = "SELECT id, name FROM CallCenter.RatingTypes R order by OrderNum";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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

        public List<PeriodDto> GetPeriods()
        {
            var query = "SELECT id,Name,SetTime,OrderNum FROM CallCenter.PeriodTimes P";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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
                return periods.OrderBy(i => i.OrderNum).ToList();
            }
        }
        public List<ServiceCompanyDto> GetServiceCompanies()
        {
            var query = "SELECT id,name FROM CallCenter.ServiceCompanies S where Enabled = 1 order by S.Name";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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
        public List<SpecialityDto> GetSpecialities()
        {
            var query = "SELECT id,name FROM CallCenter.Speciality S order by S.name";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                var specialityDtos = new List<SpecialityDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        specialityDtos.Add(new SpecialityDto
                        {
                            Id = dataReader.GetInt32("id"),
                            Name = dataReader.GetString("name")
                        });
                    }
                    dataReader.Close();
                }
                return specialityDtos.OrderBy(i => i.Name).ToList();
            }
        }
        public SpecialityDto GetSpecialityById(int id)
        {
            SpecialityDto specialityDto = null;
            var query = "SELECT id,name FROM CallCenter.Speciality S where id = @ID";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        specialityDto = new SpecialityDto
                        {
                            Id = dataReader.GetInt32("id"),
                            Name = dataReader.GetString("name")
                        };
                    }
                    dataReader.Close();
                }
                return specialityDto;
            }
        }
        public ServiceCompanyDto GetServiceCompanyById(int id)
        {
            ServiceCompanyDto serviceCompany = null;
            var query = "SELECT id,name FROM CallCenter.ServiceCompanies S where Enabled = 1 and id = @ID";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        serviceCompany =  new ServiceCompanyDto
                                            {
                                                Id = dataReader.GetInt32("id"),
                                                Name = dataReader.GetString("name")
                                            };
                    }
                    dataReader.Close();
                }
                return serviceCompany;
            }
        }

        public List<WorkerHistoryDto> GetWorkerHistoryByRequest(int requestId)
        {
            var query = @"SELECT operation_date, R.worker_id, w.sur_name,w.first_name,w.patr_name, user_id,u.surname,u.firstname,u.patrname FROM CallCenter.RequestWorkerHistory R
 join CallCenter.Workers w on w.id = R.worker_id
 join CallCenter.Users u on u.id = user_id
 where request_id = @RequestId";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                var historyDtos = new List<WorkerHistoryDto>();
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        historyDtos.Add(new WorkerHistoryDto
                        {
                            CreateTime = dataReader.GetDateTime("operation_date"),
                            Worker = new RequestUserDto
                            {
                                Id = dataReader.GetInt32("worker_id"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
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
        public List<StatusHistoryDto> GetStatusHistoryByRequest(int requestId)
        {
            var query = @"SELECT operation_date, R.state_id, s.name, s.description, user_id,u.surname,u.firstname,u.patrname FROM CallCenter.RequestStateHistory R
 join CallCenter.RequestState s on s.id = R.state_id
 join CallCenter.Users u on u.id = user_id
 where request_id = @RequestId";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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
        public List<ExecuteDateHistoryDto> GetExecuteDateHistoryByRequest(int requestId)
        {
            var query = @"SELECT R.operation_date,R.user_id,u.surname,u.firstname,u.patrname,R.note,R.execute_date,p.Name FROM CallCenter.RequestExecuteDateHistory R
 join CallCenter.Users u on u.id = user_id
 join CallCenter.PeriodTimes p on p.id = R.period_time_id
 where request_id = @RequestId";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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

        public WebUserDto WebLogin(string userName, string password)
        {
            using (var cmd = new MySqlCommand($"Call CallCenter.WebLogin('{userName}','{password}')", _dbConnection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        return new WebUserDto
                        {
                            UserId = dataReader.GetInt32("UserId"),
                            SurName = dataReader.GetString("SurName"),
                            FirstName = dataReader.GetNullableString("FirstName"),
                            PatrName = dataReader.GetNullableString("PatrName"),
                            WorkerId = dataReader.GetInt32("worker_id"),
                            ServiceCompanyId = dataReader.GetInt32("service_company_id"),
                            SpecialityId = dataReader.GetInt32("speciality_id"),
                        };
                    }
                    dataReader.Close();
                }
            }
            return null;
        }
        #region Web
        public RequestForListDto[] WebRequestList(int currentWorkerId, string requestId, bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int? streetId, int? houseId, int? addressId, int? parentServiceId, int? serviceId, int? statusId, int? workerId)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            var sqlQuery =
                @"SELECT R.id,R.create_time,sp.name as prefix_name,s.name as street_name,h.building,h.corps,at.Name address_type, a.flat,
    R.worker_id, w.sur_name,w.first_name,w.patr_name, create_user_id,u.surname,u.firstname,u.patrname,
    R.execute_date,p.Name Period_Name, R.description,rt.name service_name, rt2.name parent_name, group_concat(cp.Number order by rc.IsMain desc separator ', ') client_phones,
    rating.Name Rating,
    RS.Description Req_Status
    FROM CallCenter.Requests R
    join CallCenter.RequestState RS on RS.id = R.state_id
    join CallCenter.WebStateToState WS on WS.state_id = R.state_id
    join CallCenter.Addresses a on a.id = R.address_id
    join CallCenter.AddressesTypes at on at.id = a.type_id
    join CallCenter.Houses h on h.id = house_id
    join CallCenter.Streets s on s.id = street_id
    join CallCenter.StreetPrefixes sp on sp.id = s.prefix_id
    join CallCenter.RequestTypes rt on rt.id = R.type_id
    join CallCenter.RequestTypes rt2 on rt2.id = rt.parrent_id
    left join CallCenter.Workers w on w.id = R.worker_id
    left join CallCenter.RequestContacts rc on rc.request_id = R.id
    left join CallCenter.ClientPhones cp on cp.id = rc.clientPhone_id
    join CallCenter.Users u on u.id = create_user_id
    left join CallCenter.PeriodTimes p on p.id = R.period_time_id
    left join CallCenter.RequestRating rtype on rtype.request_id = R.id
    left join CallCenter.RatingTypes rating on rtype.rating_id = rating.id";
            if (string.IsNullOrEmpty(requestId))
            {
                if (filterByCreateDate)
                {
                    sqlQuery += " where R.worker_id in (select id from CallCenter.Workers where id = @CurWorker union SELECT dependent_worker_id FROM CallCenter.WorkersRelations W where parent_worker_id =  @CurWorker and can_view = 1)" +
                                " and R.create_time between @FromDate and @ToDate";
                }
                else
                {
                    findFromDate = executeFromDate.Date;
                    findToDate = executeToDate.Date.AddDays(1).AddSeconds(-1);

                    sqlQuery += " where R.worker_id in (select id from CallCenter.Workers where id = @CurWorker union SELECT dependent_worker_id FROM CallCenter.WorkersRelations W where parent_worker_id =  @CurWorker and can_view = 1)" +
                                " and R.execute_date between @FromDate and @ToDate";
                }
                if (streetId.HasValue)
                    sqlQuery += $" and s.id = {streetId.Value}";
                if (houseId.HasValue)
                    sqlQuery += $" and h.id = {houseId.Value}";
                if (addressId.HasValue)
                    sqlQuery += $" and a.id = {addressId.Value}";
                if (serviceId.HasValue)
                    sqlQuery += $" and rt.id = {serviceId.Value}";
                if (parentServiceId.HasValue)
                    sqlQuery += $" and rt2.id = {parentServiceId.Value}";
                if (statusId.HasValue)
                    sqlQuery += $" and WS.Web_State_Id = {statusId.Value}";
                if (workerId.HasValue)
                    sqlQuery += $" and w.id = {workerId.Value}";
            }
            else
            {
                sqlQuery += " where R.id = @RequestId";
            }
            sqlQuery += " group by R.id order by id desc";
            using (var cmd =
                new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
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
                            Worker = dataReader.GetNullableInt("worker_id") != null ? new RequestUserDto
                            {
                                Id = dataReader.GetInt32("worker_id"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                            } : null,
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
                            Status = dataReader.GetNullableString("Req_Status"),
                        });
                    }
                    dataReader.Close();
                }
                return requests.ToArray();
            }
        }

        public WorkerDto[] GetWorkersByWorkerId(int workerId)
        {
            var sqlQuery = @"SELECT id, service_company_id, sur_name, first_name, patr_name, speciality_id FROM CallCenter.Workers w where(w.id = @WorkerId or
    w.service_company_id = (select service_company_id from CallCenter.Workers where id = @WorkerId)
    or w.id in (select dependent_worker_id from CallCenter.WorkersRelations where parent_worker_id = @WorkerId)
    ) and enabled = 1  order by sur_name,first_name,patr_name";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
        public StreetDto[] GetStreetsByWorkerId(int workerId)
        {
            var sqlQuery = @"SELECT s.id,s.name,s.city_id,p.id as Prefix_id,p.Name as Prefix_Name,p.ShortName FROM CallCenter.Houses h
    join CallCenter.Streets s on s.id = h.street_id
    join CallCenter.StreetPrefixes p on p.id = s.prefix_id
    join CallCenter.Workers w on w.service_company_id = h.service_company_id
    where w.id = @WorkerId and s.enabled = 1
    group by s.id,s.name";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@WorkerId", workerId);
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
        public StreetDto GetStreetById(int streetId)
        {
            var sqlQuery = @"SELECT s.id,s.name,s.city_id,p.id as Prefix_id,p.Name as Prefix_Name,p.ShortName FROM CallCenter.Streets s
    join CallCenter.StreetPrefixes p on p.id = s.prefix_id
    where s.id = @StreetId and s.enabled = 1";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@StreetId", streetId);
                StreetDto street = null;
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        street = new StreetDto
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
                        };
                    }
                    dataReader.Close();
                }
                return street;
            }
        }

        public WebHouseDto[] GetHousesByStreetAndWorkerId(int streetId,int workerId)
        {
            var sqlQuery = @"SELECT h.id,h.Building,h.Corps FROM CallCenter.Houses h
    join CallCenter.Workers w on w.service_company_id = h.service_company_id
    where w.id = @WorkerId and h.enabled = 1 and h.street_id = @StreetId";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
                return houses.Select(h=> new WebHouseDto { Id= h.Id,Name = h.FullName}).ToArray();
            }
        }
        public WebStatusDto[] GetWebStatuses()
        {

            var sqlQuery = @"SELECT id,name FROM CallCenter.WebState w order by id";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                var states = new List<WebStatusDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var status_id = dataReader.GetInt32("id");
                        states.Add(new WebStatusDto
                        {
                            Id = status_id,
                            Name = dataReader.GetString("name"),
                            //IsDefault = status_id == 2 ? true : false
                        });
                    }
                    dataReader.Close();
                }
                return states.ToArray();
            }
        }
        #endregion

        public List<CallsListDto> GetCallList(DateTime fromDate, DateTime toDate, string requestId, int? operatorId)
        {
            var sqlQuery = @"SELECT UniqueId,CallDirection,CallerIDNum,CreateTime,AnswerTime,EndTime,BridgedTime, 
 MonitorFile,TalkTime,WaitingTime, userId, SurName, FirstName, PatrName, RequestId FROM asterisk.CallsHistory C";
            if (!string.IsNullOrEmpty(requestId))
            {
                sqlQuery += " where RequestId = @RequestNum";
            }
            else
            {
                sqlQuery += " where CreateTime between @fromdate and @todate";
                if (operatorId.HasValue)
                {
                    sqlQuery += " and userId = @UserNum";

                }
            }
            sqlQuery += " order by UniqueId";

            using (
            var cmd = new MySqlCommand(sqlQuery, AppSettings.DbConnection))
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
                }
                    using (var dataReader = cmd.ExecuteReader())
                {
                    var callList = new List<CallsListDto>();
                    while (dataReader.Read())
                    {
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
                            RequestId = dataReader.GetNullableInt("RequestId"),
                            User = dataReader.GetNullableInt("userId").HasValue
                                ? new RequestUserDto
                                {
                                    Id = dataReader.GetInt32("userId"),
                                    SurName = dataReader.GetNullableString("SurName"),
                                    FirstName = dataReader.GetNullableString("FirstName"),
                                    PatrName = dataReader.GetNullableString("PatrName")
                                }
                                : null
                        });
                    }
                    dataReader.Close();
                    return callList;
                }
            }
        }

        public List<RequestUserDto> GetOperators()
        {
            using (
                var cmd = new MySqlCommand(@"SELECT u.id, SurName, FirstName, PatrName FROM CallCenter.Users u where u.enabled = 1 and u.ShowInForm = 1 order by SurName,FirstName",
                    AppSettings.DbConnection))
            {
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
                    return usersList;
                }
            }

        }

        public void ChangeDescription(int requestId, string description)
        {
            _logger.Debug($"RequestService.ChangeDescription({requestId},{description})");
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (var cmd = new MySqlCommand(@"update CallCenter.Requests set description = @Desc where id = @RequestId", _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@Desc", description);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
                throw;
            }
        }

        public string ServiceCompanyByIncommingPhoneNumber(string phoneNumber)
        {
            using (var cmd = new MySqlCommand(@"SELECT S.id,S.Name FROM asterisk.ActiveChannels A
            join CallCenter.ServiceCompanies S on S.trunk_name = A.context where A.CallerIDNum = @phoneNumber", AppSettings.DbConnection))
            {
                cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        return dataReader.GetNullableString("Name");
                    }
                    dataReader.Close();
                }

            }
            return "неизвестная УК";
        }

        public void SaveServiceCompany(int? serviceCompanyId, string serviceCompanyName)
        {
            if (serviceCompanyId.HasValue)
            {
                using (var cmd = new MySqlCommand(@"update CallCenter.ServiceCompanies set Name = @ServiceCompanyName where id = @ID;", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ID", serviceCompanyId.Value);
                    cmd.Parameters.AddWithValue("@ServiceCompanyName", serviceCompanyName);
                    cmd.ExecuteNonQuery();
                }

            }
            else
            {
                using (var cmd = new MySqlCommand(@"insert into CallCenter.ServiceCompanies(Name) values(@ServiceCompanyName);", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ServiceCompanyName", serviceCompanyName);
                    cmd.ExecuteNonQuery();
                }
            }

        }
        public void SaveStreet(int? streetId, string streetName, int cityId, int streetPrefixId)
        {
            if (streetId.HasValue)
            {
                using (var cmd = new MySqlCommand(@"update CallCenter.Streets set name = @StreetName,prefix_id = @PrefixId, city_id = @CityId where id = @ID;", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ID", streetId.Value);
                    cmd.Parameters.AddWithValue("@StreetName", streetName);
                    cmd.Parameters.AddWithValue("@CityId", cityId);
                    cmd.Parameters.AddWithValue("@PrefixId", streetPrefixId);
                    cmd.ExecuteNonQuery();
                }

            }
            else
            {
                using (var cmd = new MySqlCommand(@"insert into CallCenter.Streets(name,prefix_id,city_id) values(@StreetName, @PrefixId, @CityId);", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@StreetName", streetName);
                    cmd.Parameters.AddWithValue("@CityId", cityId);
                    cmd.Parameters.AddWithValue("@PrefixId", streetPrefixId);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public void SaveSpeciality(int? specialityId, string specialityName)
        {
            if (specialityId.HasValue)
            {
                using (var cmd = new MySqlCommand(@"update CallCenter.Speciality set Name = @specialityName where id = @ID;", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ID", specialityId.Value);
                    cmd.Parameters.AddWithValue("@specialityName", specialityName);
                    cmd.ExecuteNonQuery();
                }

            }
            else
            {
                using (var cmd = new MySqlCommand(@"insert into CallCenter.Speciality(Name) values(@specialityName);", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@specialityName", specialityName);
                    cmd.ExecuteNonQuery();
                }
            }

        }
        public void SaveWorker(int? workerId, int serviceCompanyId,string surName,string firstName,string patrName,string phone,int specialityId)
        {
            if (workerId.HasValue)
            {
                using (var cmd = new MySqlCommand(@"update CallCenter.Workers set sur_name = @surName,first_name = @firstName,patr_name = @patrName,phone = @phone,service_company_id = @serviceCompanyId, speciality_id = @specialityId where id = @ID;", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ID", workerId.Value);
                    cmd.Parameters.AddWithValue("@surName", surName);
                    cmd.Parameters.AddWithValue("@firstName", firstName);
                    cmd.Parameters.AddWithValue("@patrName", patrName);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@serviceCompanyId", serviceCompanyId);
                    cmd.Parameters.AddWithValue("@specialityId", specialityId);
                    cmd.ExecuteNonQuery();
                }

            }
            else
            {
                using (var cmd = new MySqlCommand(@"insert into CallCenter.Workers(sur_name,first_name,patr_name,phone,service_company_id,speciality_id) values(@surName,@firstName,@patrName,@phone,@serviceCompanyId,@specialityId);", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@surName", surName);
                    cmd.Parameters.AddWithValue("@firstName", firstName);
                    cmd.Parameters.AddWithValue("@patrName", patrName);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@serviceCompanyId", serviceCompanyId);
                    cmd.Parameters.AddWithValue("@specialityId", specialityId);

                    cmd.ExecuteNonQuery();
                }
            }

        }

        public void DeleteWorker(int selectedWorkerId)
        {
            using (var cmd = new MySqlCommand(@"update CallCenter.Workers set enabled = false where id = @ID;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ID", selectedWorkerId);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteService(int id)
        {
            using (var cmd = new MySqlCommand(@"update CallCenter.RequestTypes set enabled = false where id = @ID;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteStreet(int streetId)
        {
            using (var cmd = new MySqlCommand(@"update CallCenter.Streets set enabled = false where id = @ID;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ID", streetId);
                cmd.ExecuteNonQuery();
            }
        }


        public ServiceDto GetServiceById(int serviceId)
        {
            ServiceDto service = null;
            var query = "SELECT id,name FROM CallCenter.RequestTypes  where enabled = 1 and id = @ID";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ID", serviceId);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        service = new ServiceDto
                        {
                            Id = dataReader.GetInt32("id"),
                            Name = dataReader.GetString("name")
                        };
                    }
                    dataReader.Close();
                }
            }
            return service;
        }
        public void SaveService(int? serviceId, int? parentId,  string serviceName)
        {
            if (serviceId.HasValue)
            {
                using (var cmd = new MySqlCommand(@"update CallCenter.RequestTypes set Name = @serviceName, parrent_id = @parentId where id = @ID;", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ID", serviceId.Value);
                    cmd.Parameters.AddWithValue("@serviceName", serviceName);
                    cmd.Parameters.AddWithValue("@parentId", parentId);
                    cmd.ExecuteNonQuery();
                }

            }
            else
            {
                using (var cmd = new MySqlCommand(@"insert into CallCenter.RequestTypes(parrent_id,Name) values(@parentId, @serviceName);", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@serviceName", serviceName);
                    cmd.Parameters.AddWithValue("@parentId", parentId);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public HouseDto GetHouseById(int houseId)
        {
            HouseDto house = null;
            using (var cmd = new MySqlCommand(@"SELECT h.id, h.street_id, h.building, h.corps, h.service_company_id, h.entrance_count, h.flat_count, h.floor_count, h.service_company_id, s.Name service_company_name FROM CallCenter.Houses h
 left join CallCenter.ServiceCompanies s on s.id = h.service_company_id where h.enabled = 1 and h.id = @HouseId", _dbConnection))
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
                            ServiceCompanyId = dataReader.GetNullableInt("service_company_id"),
                            ServiceCompanyName = dataReader.GetNullableString("service_company_name"),
                            EntranceCount = dataReader.GetNullableInt("entrance_count"),
                            FlatCount = dataReader.GetNullableInt("flat_count"),
                            FloorCount = dataReader.GetNullableInt("floor_count"),
                        };
                    }
                    dataReader.Close();
                }

            }
            return house;
        }

        public void SaveHouse(int? houseId, int streetId, string buildingNumber, string corpus, int serviceCompanyId,
            int? entranceCount, int? floorCount, int? flatsCount)
        {
            using (var cmd = new MySqlCommand(
                    @"call CallCenter.AddOrUndateHouse(@houseId,@streetId,@buildingNumber,@corpus,@serviceCompanyId,@entranceCount,@floorCount,@flatsCount);",
                    _dbConnection))
            {
                cmd.Parameters.AddWithValue("@houseId", houseId);
                cmd.Parameters.AddWithValue("@streetId", streetId);
                cmd.Parameters.AddWithValue("@buildingNumber", buildingNumber);
                cmd.Parameters.AddWithValue("@corpus", corpus);
                cmd.Parameters.AddWithValue("@serviceCompanyId", serviceCompanyId);
                cmd.Parameters.AddWithValue("@entranceCount", entranceCount);
                cmd.Parameters.AddWithValue("@floorCount", floorCount);
                cmd.Parameters.AddWithValue("@flatsCount", flatsCount);
                cmd.ExecuteNonQuery();
            }
        }

        public HouseDto FindHouse(int streetId, string buildingNumber, string corpus)
        {
            HouseDto house = null;
            var sqlQuery =
                @"SELECT h.id, h.street_id, h.building, h.corps, h.service_company_id, h.entrance_count, h.flat_count, h.floor_count, h.service_company_id, s.Name service_company_name FROM CallCenter.Houses h
 left join CallCenter.ServiceCompanies s on s.id = h.service_company_id where h.enabled = 1 and h.street_id = @streetId and building = @buildingNumber";
            if (!string.IsNullOrEmpty(corpus))
                sqlQuery += " and corps = @corpus";

            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@streetId", streetId);
                cmd.Parameters.AddWithValue("@buildingNumber", buildingNumber);
                if (!string.IsNullOrEmpty(corpus))
                    cmd.Parameters.AddWithValue("@corpus", corpus);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        house = new HouseDto()
                        {
                            Building = dataReader.GetString("building"),
                            StreetId = dataReader.GetInt32("street_id"),
                            Corpus = dataReader.GetNullableString("corps"),
                            ServiceCompanyId = dataReader.GetNullableInt("service_company_id"),
                            ServiceCompanyName = dataReader.GetNullableString("service_company_name"),
                            EntranceCount = dataReader.GetNullableInt("entrance_count"),
                            FlatCount = dataReader.GetNullableInt("flat_count"),
                            FloorCount = dataReader.GetNullableInt("floor_count"),
                        };
                    }
                    dataReader.Close();
                }
            }
            return house;
        }
        public void DeleteHouse(int houseId)
        {
            using (var cmd = new MySqlCommand(@"update CallCenter.Houses set enabled = false where id = @ID;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ID", houseId);
                cmd.ExecuteNonQuery();
            }
    }

        public string GetRedirectPhone()
        {
            string phone = null;
            using (var cmd = new MySqlCommand("SELECT Phone FROM asterisk.RedirectPhone where id = 1;", _dbConnection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        phone = dataReader.GetNullableString("Phone");
                    }
                    dataReader.Close();
                }
            }
            return phone;
        }

        public void SaveRedirectPhone(string phoneNumber)
        {
            using (var cmd = new MySqlCommand(@"update asterisk.RedirectPhone set Phone = @PhoneNumber where id = 1;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                cmd.ExecuteNonQuery();
            }
        }
    }

}

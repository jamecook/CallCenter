using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using CRMPhone.Dto;
using CRMPhone.ViewModel;
using MySql.Data.MySqlClient;
using NLog;

namespace CRMPhone
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
            bool chargeable, bool immediate, string callUniqueId,string entrance,string floor)
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
                                new MySqlCommand("insert into CallCenter.RequestCalls(request_id,uniqueID) values(@Request, @UniqueId)",_dbConnection))
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

        public void AddNewExecuteDate(int requestId, DateTime executeDate,PeriodDto period, string note)
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

        internal string GetActiveCallUniqueId()
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
    entrance,floor
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
                                ExecuteDate =  dataReader.GetNullableDateTime("execute_date"),
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
                                }

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

        public IList<RequestForListDto> GetRequestList(DateTime fromDate, DateTime toDate, int? streetId, int? houseId,int? addressId,int? parentServiceId,int? serviceId,int? statusId,int? workerId)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            var sqlQuery = @"SELECT R.id,R.create_time,sp.name as prefix_name,s.name as street_name,h.building,h.corps,at.Name address_type, a.flat,
    R.worker_id, w.sur_name,w.first_name,w.patr_name, create_user_id,u.surname,u.firstname,u.patrname,
    R.execute_date,p.Name Period_Name, R.description,rt.name service_name, rt2.name parent_name, group_concat(cp.Number order by rc.IsMain desc separator ', ') client_phones
    FROM CallCenter.Requests R
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
    where R.create_time between @FromDate and @ToDate";
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

            sqlQuery += " group by R.id order by id desc";
            using (var cmd =
                new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@FromDate", findFromDate);
                cmd.Parameters.AddWithValue("@ToDate", findToDate);

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
                            Worker = dataReader.GetNullableInt("worker_id")!=null?new RequestUserDto
                            {
                                Id = dataReader.GetInt32("worker_id"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                            }:null,
                            CreateUser = new RequestUserDto
                            {
                                Id = dataReader.GetInt32("create_user_id"),
                                SurName = dataReader.GetNullableString("surname"),
                                FirstName = dataReader.GetNullableString("firstname"),
                                PatrName = dataReader.GetNullableString("patrname"),
                            },
                            ExecuteTime = dataReader.GetNullableDateTime("execute_date"),
                            ExecutePeriod = dataReader.GetNullableString("Period_Name"),


                        });
                    }
                    dataReader.Close();
                }
                return requests;
            }
        }

        public IList<WorkerDto> GetWorkers(int? serviceCompanyId)
        {
            var query = @"select * from CallCenter.Workers where enabled = 1";
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
                            SurName = dataReader.GetString("sur_name"),
                            FirstName = dataReader.GetNullableString("first_name"),
                            PatrName = dataReader.GetNullableString("patr_name"),
                            Speciality = dataReader.GetNullableString("speciality"),
                        });
                    }
                    dataReader.Close();
                }
                return workers;
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
                        @"SELECT id,street_id,building,corps,service_company_id FROM CallCenter.Houses where street_id = @StreetId and enabled = 1;",
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
    where A.house_id = @HouseId", _dbConnection))
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
                return periods.OrderBy(i=>i.OrderNum).ToList();
            }
        }
        public List<ServiceCompanyDto> GetServiceCompanies()
        {
            var query = "SELECT id,name FROM CallCenter.ServiceCompanies S where Enabled = 1";
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
                return companies.OrderBy(i=>i.Name).ToList();
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
    }
}
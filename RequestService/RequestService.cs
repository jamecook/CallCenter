using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NLog;
using NLog.LayoutRenderers;
using NLog.Targets;
using RequestServiceImpl.Dto;

namespace RequestServiceImpl
{
    public partial class RequestService
    {
        private static Logger _logger;
        private MySqlConnection _dbConnection;

        public RequestService(MySqlConnection dbConnection)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _dbConnection = dbConnection;
        }

        public DateTime GetCurrentDate()
        {
            using (var cmd =
                new MySqlCommand(@"select sysdate() curdate", _dbConnection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    dataReader.Read();
                    return dataReader.GetDateTime("curdate");
                }
            }
        }

        public bool IsEnableIvrNotification(int serviceCompanyId)
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                using (var scCmd = new MySqlCommand(
                            @"SELECT enabled FROM CallCenter.IvrNotification  where service_company_id = @ServiceCompanyId",_dbConnection))
                {
                    scCmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);

                    using (var dataReader = scCmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return dataReader.GetBoolean("enabled");
                        }
                        dataReader.Close();
                    }
                    return false;
                }
            }
        }
        public void SaveIvrNotification(int serviceCompanyId,bool isEnabled)
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                using (var scCmd = new MySqlCommand(
                            @"Update CallCenter.IvrNotification set enabled = @Enabled where service_company_id = @ServiceCompanyId",_dbConnection))
                {
                    scCmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);
                    scCmd.Parameters.AddWithValue("@Enabled", isEnabled);
                    scCmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        public void EditRequest(int requestId, int requestTypeId, string requestMessage, bool immediate, bool chargeable, bool isBadWork, int garanty,bool isRetry,DateTime? alertTime, DateTime? termOfExecution)
        {
            using (var cmd = new MySqlCommand(
                   @"call CallCenter.UpdateRequest(@userId,@requestId,@requestTypeId,@requestMessage,@immediate,@chargeable,@badWork,@garanty,@retry,@alertTime,@termOfExecution);",
                   _dbConnection))
            {
                cmd.Parameters.AddWithValue("@userId", AppSettings.CurrentUser.Id);
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

        public int? SaveNewRequest(int addressId, int requestTypeId, ContactDto[] contactList, string requestMessage,
            bool chargeable, bool immediate, string callUniqueId, string entrance, string floor, DateTime? alertTime,bool isRetry, bool isBedWork,int? equipmentId)
        {
            int newId;
            //_logger.Debug($"RequestService.SaveNewRequest({addressId},{requestTypeId},[{contactList.Select(x => $"{x.PhoneNumber}").Aggregate((f1, f2) => f1 + ";" + f2)}],{requestMessage},{chargeable},{immediate},{callUniqueId})");
            try
            {

                using (var transaction = _dbConnection.BeginTransaction())
                {
                    //Определяем УК по адресу
                    var serviceCompanyId = (int?)null;
                    using (var scCmd = new MySqlCommand(@"SELECT h.service_company_id FROM CallCenter.Addresses a
 join CallCenter.Houses h on h.id = a.house_id
 where a.id = @AddressId", _dbConnection))
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
                            @"insert into CallCenter.Requests(address_id,type_id,description,create_time,is_chargeable,create_user_id,state_id,is_immediate, entrance, floor, service_company_id,retry ,bad_work , alert_time, equipment_id)
 values(@AddressId, @TypeId, @Message, sysdate(),@IsChargeable,@UserId,@State,@IsImmediate,@Entrance,@Floor,@ServiceCompanyId,@Retry,@BadWork,@AlertTime, @EquipmentId);
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
                        cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);
                        cmd.Parameters.AddWithValue("@AlertTime", alertTime);
                        cmd.Parameters.AddWithValue("@Retry", isRetry);
                        cmd.Parameters.AddWithValue("@BadWork", isBedWork);
                        cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);
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
                        AddCallHistory(newId, callUniqueId, AppSettings.CurrentUser.Id, AppSettings.LastCallId, "CreateNewRequest");
                    }

                    #endregion

                    #region Сохранение контактных номеров 

                    foreach (
                        var contact in
                            contactList.Where(c => !string.IsNullOrEmpty(c.PhoneNumber))
                                .OrderByDescending(c => c.IsMain))
                    {
                        var clientPhoneId = 0;
                        ContactDto currentInfo = null;
                        using (
                            var cmd = new MySqlCommand(
                                "SELECT id,name,email,addition FROM CallCenter.ClientPhones C where Number = @Phone", _dbConnection))
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
                                var cmd = new MySqlCommand(@"insert into CallCenter.ClientPhones(Number,name,email,addition) values(@Phone,@Name,@Email,@AddInfo);
    select LAST_INSERT_ID();", _dbConnection))
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
    var cmd = new MySqlCommand(@"update CallCenter.ClientPhones set name = @Name,email = @Email,addition = @AddInfo where id = @Id;", _dbConnection))
                            {
                                cmd.Parameters.AddWithValue("@Id", currentInfo.Id);
                                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(contact.Name) ? currentInfo.Name : contact.Name);
                                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(contact.Email) ? currentInfo.Email : contact.Email);
                                cmd.Parameters.AddWithValue("@AddInfo", string.IsNullOrEmpty(contact.AdditionInfo) ? currentInfo.AdditionInfo : contact.AdditionInfo);
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

        public byte[] GetMediaByRequestId(int requestId)
        {
            using (var cmd =
                    new MySqlCommand(@"SELECT MonitorFile FROM CallCenter.RequestCalls r
    join asterisk.ChannelHistory c on c.UniqueID = r.uniqueID
    where request_id = @reqId", _dbConnection))
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
        public byte[] GetRecordById(int recordId)
        {
            using (var cmd =
                    new MySqlCommand(@"SELECT MonitorFile FROM CallCenter.RequestCalls r
    join asterisk.ChannelHistory c on c.UniqueID = r.uniqueID
    where r.id = @reqId", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@reqId", recordId);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        var fileName = dataReader.GetNullableString("MonitorFile");
                        var serverIpAddress = _dbConnection.DataSource;
                        var localFileName = fileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\").Replace("/", "\\");
                        var localFileNameMp3 = localFileName.Replace(".wav", ".mp3");
                        if (File.Exists(localFileNameMp3))
                        {
                            return File.ReadAllBytes(localFileName);
                        }
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
        public void AddNewMaster(int requestId, int? workerId)
        {
            
            _logger.Debug($"RequestService.AddNewMaster({requestId},{workerId})");
            if (workerId.HasValue)
            {
                SendSmsToWorker(requestId, workerId.Value);
            }

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

        private void SendSmsToWorker(int requestId, int workerId)
        {
            var request = GetRequest(requestId);
            var worker = GetWorkerById(workerId);
            if (!worker.SendSms)
                return;
            var smsSettings = GetSmsSettingsForServiceCompany(request.ServiceCompanyId);
            var service = GetServiceById(request.Type.Id);
            var parrentService = request.Type.ParentId.HasValue ? GetServiceById(request.Type.ParentId.Value) : null;
            if (!((parrentService?.CanSendSms??true) && service.CanSendSms))
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
                var smsText = $"{request.Id} {phones ?? ""} {request.Address.FullAddress}.{request.Type.Name}({request.Description ?? ""})";
                if (smsText.Length > 70)
                {
                    smsText = smsText.Substring(0, 70);
                }
                //var smsText = $"№ {request.Id}. {request.Type.Name}({request.Description}) {request.Address.FullAddress}. {phones}.";
                SendSms(requestId, smsSettings.Sender, worker.Phone, smsText, false);
            }
                //SendSms(requestId, smsSettings.Sender, worker.Phone,
                //    $"№ {requestId}. {request.Type.ParentName}/{request.Type.Name}({request.Description}) {request.Address.FullAddress}. {phones}.",
                //    false);
            //SendSms(requestId, smsSettings.Sender, worker.Phone, $"Заявка № {requestId}. Услуга {request.Type.ParentName}. Причина {request.Type.Name}. Примечание: {request.Description}. Адрес: {request.Address.FullAddress}. Телефоны {phones}.");
        }

        public void AddNewExecuter(int requestId, int? workerId)
        {
            _logger.Debug($"RequestService.AddNewExecuter({requestId},{workerId})");
            if (workerId.HasValue)
            {
                SendSmsToWorker(requestId, workerId.Value);
            }
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestExecuterHistory (request_id,operation_date,user_id,executer_id) 
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
                                @"update CallCenter.Requests set executer_id = @WorkerId where id = @RequestId",
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

        public void AddNewState(int requestId, int stateId,int userId = 0)
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
                        cmd.Parameters.AddWithValue("@UserId", userId==0 ? AppSettings.CurrentUser.Id : userId);
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
                    if(stateId == 3)
                    { 
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"update CallCenter.Requests set close_date = sysdate() where close_date is null and id = @RequestId",
                                _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.ExecuteNonQuery();
                    }
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

        public void AddNewTermOfExecution(int requestId, DateTime termOfExecution, string note)
        {
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"update CallCenter.Requests set term_of_execution = @ExecuteDate where id = @RequestId",
                                _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@ExecuteDate", termOfExecution);
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

 //       public string GetActiveCallUniqueId()
 //       {
 //           string retVal = null;
 //           var query = @"SELECT case when A.MonitorFile is null then A2.UniqueId else A.UniqueId end uniqueId FROM asterisk.ActiveChannels A
 //left join asterisk.ActiveChannels A2 on A2.BridgeId = A.BridgeId and A2.UniqueID <> A.UniqueID
 //where A.UserID = @UserId";
 //           using (var cmd = new MySqlCommand(query, _dbConnection))
 //           {
 //               cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
 //               using (var dataReader = cmd.ExecuteReader())
 //               {
 //                   if (dataReader.Read())
 //                   {
 //                       retVal = dataReader.GetNullableString("uniqueId");
 //                   }
 //                   dataReader.Close();
 //               }
 //               return retVal;
 //           }
 //       }
        public string GetActiveCallUniqueIdByCallId(string callId)
        {
            string retVal = null;
            if (string.IsNullOrEmpty(callId))
                return null;
            var query = $@"SELECT case when A.MonitorFile is null then ifnull(A2.UniqueId,A.UniqueId) else A.UniqueId end uniqueId FROM asterisk.ActiveChannels A
 left join asterisk.ActiveChannels A2 on A2.BridgeId = A.BridgeId and A2.UniqueID <> A.UniqueID
 where A.call_id like '{callId}%'";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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
            if(!string.IsNullOrEmpty(retVal))
                    return retVal;
                query = $@"SELECT case when A.MonitorFile is null then ifnull(A2.UniqueId,A.UniqueId) else A.UniqueId end uniqueId FROM asterisk.ChannelHistory A
 left join asterisk.ChannelHistory A2 on A2.BridgeId = A.BridgeId and A2.UniqueID <> A.UniqueID
 where A.call_id like '{callId}%'";
                using (var cmd = new MySqlCommand(query, _dbConnection))
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
        public string GetOnlyActiveCallUniqueIdByCallId(string callId)
        {
            string retVal = null;
            if (!string.IsNullOrEmpty(callId))
            {
                var query =
                    $@"SELECT case when A.MonitorFile is null then ifnull(A2.UniqueId,A.UniqueId) else A.UniqueId end uniqueId FROM asterisk.ActiveChannels A
 left join asterisk.ActiveChannels A2 on A2.BridgeId = A.BridgeId and A2.UniqueID <> A.UniqueID
 where A.call_id like '{callId}%'";
                using (var cmd = new MySqlCommand(query, _dbConnection))
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
            //Логирование состояния
            var lineState = JsonConvert.SerializeObject(AppSettings.SipLines);
            var sipInfo = JsonConvert.SerializeObject(AppSettings.SipInfo);

            using (var transaction = _dbConnection.BeginTransaction())
            {
                using (var cmd =
                        new MySqlCommand(@"insert into CallCenter.GetOnlyActiveCall (call_id,line_info,oper_date,user_id,sip_info)
 values(@CallId,@LineInfo,sysdate(),@UserId,@SipInfo);", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@CallId", callId);
                    cmd.Parameters.AddWithValue("@LineInfo", lineState);
                    cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                    cmd.Parameters.AddWithValue("@SipInfo", sipInfo);
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }


            return retVal;
        }
        public string GetActiveCallUniqueIdByPhone(string phone)
        {
            string retVal = null;
            var query = @"call asterisk.GetUniqueIdByPhone(@Phone)";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Phone", phone);
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

        public void AddNewNote(int requestId, string note, int? userId = null)
        {
            //_logger.Debug($"RequestService.AddNewNote({requestId},{note})");
            var currentUserId = userId ?? AppSettings.CurrentUser.Id;
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
                        cmd.Parameters.AddWithValue("@UserId", currentUserId);
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

        public List<NoteDto> GetNotesWeb(int requestId)
        {
            return GetNotesCore(requestId, _dbConnection);
        }
        public List<NoteDto> GetNotes(int requestId)
        {
            return GetNotesCore(requestId, _dbConnection).OrderByDescending(n => n.Date).ToList();
        }

        public List<NoteDto> GetNotesCore(int requestId, MySqlConnection dbConnection)
        {
            var sqlQuery = @"SELECT n.id,n.operation_date,n.request_id,n.user_id,n.note,n.worker_id,u.SurName,u.FirstName,u.PatrName,w.sur_name,w.first_name,w.patr_name
    from CallCenter.RequestNoteHistory n
    join CallCenter.Users u on u.id = n.user_id
    left join CallCenter.Workers w on w.id = n.worker_id where request_id = @RequestId order by operation_date";
            using (
            var cmd = new MySqlCommand(sqlQuery, dbConnection))
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


        public RequestInfoDto GetRequest(int requestId)
        {
            _logger.Debug($"RequestService.GetRequest({requestId})");

            RequestInfoDto result = null;
            try
            {
                using (var cmd =
                    new MySqlCommand(@"SELECT R.id req_id,R.Address_id,R.type_id,R.description, R.create_time,R.is_chargeable,R.is_immediate,R.period_time_id,R.state_id,R.worker_id,R.execute_date,R.service_company_id,
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
                                Rating = dataReader.GetNullableInt("rating_id").HasValue ? new RequestRatingDto
                                {
                                    Id = dataReader.GetInt32("rating_id"),
                                    Name = dataReader.GetString("RatingName"),
                                    Description = dataReader.GetNullableString("RatingDesc")
                                } : new RequestRatingDto(),
                                Equipment = dataReader.GetNullableInt("equipment_id").HasValue ? new EquipmentDto
                                {
                                    Id = dataReader.GetInt32("equipment_id"),
                                    Name = $"{dataReader.GetString("eq_type_name")} - {dataReader.GetString("eq_name")}"
                                } : new EquipmentDto{ Id = null,Name = "Нет"}
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
            catch (Exception exc)
            {
                _logger.Error(exc);
            }
            return null;
        }

        public IList<FondDto> GetServiceCompanyFondList(int[] streetsId, int? houseId, int? addressId)
        {
            var sqlQuery = @"SELECT s.name street_name, h.building,h.corps,a.flat,c.* FROM CallCenter.CitizenAddresses c
    join CallCenter.Addresses a on c.address_id = a.id
    join CallCenter.Houses h on h.id = a.house_id
    join CallCenter.Streets s on s.id = h.street_id";
            if (streetsId != null && streetsId.Length > 0)
                sqlQuery += $" and s.id in ({streetsId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";
            if (houseId.HasValue)
                sqlQuery += $" and h.id = {houseId.Value}";
            if (addressId.HasValue)
                sqlQuery += $" and a.id = {addressId.Value}";
            sqlQuery += " order by s.name,h.building,h.corps,a.flat";
            using (var cmd =
                new MySqlCommand(sqlQuery, _dbConnection))
            {
                var fondList = new List<FondDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        fondList.Add(new FondDto
                        {
                            Id = dataReader.GetInt32("id"),
                            StreetName = dataReader.GetString("street_name"),
                            Flat = dataReader.GetString("flat"),
                            Building = dataReader.GetString("building"),
                            Corpus = dataReader.GetNullableString("corps"),
                            Name = dataReader.GetNullableString("name"),
                            Phones = dataReader.GetNullableString("phones"),
                            KeyDate = dataReader.GetNullableDateTime("key_date"),
                        });
                    }
                    dataReader.Close();
                }
                return fondList;
            }
        }

        public IList<RequestForListDto> GetRequestList(string requestId, bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int[] streetsId, int? houseId, int? addressId, int[] parentServicesId, int? serviceId, int[] statusesId, int[] mastersId, int[] executersId, int[] serviceCompaniesId,int[] usersId, int[] ratingsId, int? payment, bool onlyBadWork, bool onlyRetry, string clientPhone, bool onlyGaranty, bool onlyImmediate, bool onlyByClient)
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
                    sqlQuery += $" and s.id in ({streetsId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";
                if (houseId.HasValue)
                    sqlQuery += $" and h.id = {houseId.Value}";
                if (addressId.HasValue)
                    sqlQuery += $" and a.id = {addressId.Value}";
                if (serviceId.HasValue)
                    sqlQuery += $" and rt.id = {serviceId.Value}";

                if (parentServicesId != null && parentServicesId.Length >0)
                    sqlQuery += $" and rt2.id in ({parentServicesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";

                if (statusesId !=null && statusesId.Length >0)
                    sqlQuery += $" and R.state_id in ({statusesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)})";

                if (mastersId != null && mastersId.Length>0)
                    sqlQuery += $" and w.id in ({mastersId.Select(x=>x.ToString()).Aggregate((x,y)=>x+","+y)})";

                if (executersId != null && executersId.Length>0)
                    sqlQuery += $" and R.executer_id in ({executersId.Select(x=>x.ToString()).Aggregate((x,y)=>x+","+y)})";

                if (serviceCompaniesId != null && serviceCompaniesId.Length>0)
                    sqlQuery += $" and R.service_company_id in ({serviceCompaniesId.Select(x=>x.ToString()).Aggregate((x,y)=>x+","+y)})";

                if (usersId != null && usersId.Length>0)
                    sqlQuery += $" and R.create_user_id in ({usersId.Select(x=>x.ToString()).Aggregate((x,y)=>x+","+y)})";

                if (ratingsId != null && ratingsId.Length>0)
                    sqlQuery += $" and rtype.rating_id in ({ratingsId.Select(x=>x.ToString()).Aggregate((x,y)=>x+","+y)})";

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
                            Master = dataReader.GetNullableInt("worker_id") != null ? new RequestUserDto
                            {
                                Id = dataReader.GetInt32("worker_id"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                            } : null,
                            Executer = dataReader.GetNullableInt("executer_id") != null ? new RequestUserDto
                            {
                                Id = dataReader.GetInt32("executer_id"),
                                SurName = dataReader.GetNullableString("exec_sur_name"),
                                FirstName = dataReader.GetNullableString("exec_first_name"),
                                PatrName = dataReader.GetNullableString("exec_patr_name"),
                            } : null,
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
                return requests;
            }
        }
        public IList<RequestForListDto> GetAlertedRequests()
        {
            var sqlQuery = "call CallCenter.GetAlertRequests();";
            using (var cmd =
                new MySqlCommand(sqlQuery, _dbConnection))
            {
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
                            Master = dataReader.GetNullableInt("worker_id") != null ? new RequestUserDto
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
        public IList<StatCallListDto> GetStatCalls(DateTime fromDate,DateTime toDate)
        {
            var sqlQuery = "call CallCenter.StatGetRings(@From,@To);";
            using (var cmd =
                new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@From", fromDate);
                cmd.Parameters.AddWithValue("@To", toDate);

                var requests = new List<StatCallListDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        requests.Add(new StatCallListDto
                        {
                            Direction = dataReader.GetNullableString("Direction"),
                            Exten = dataReader.GetNullableString("ExtenNum"),
                            ServiceCompany = dataReader.GetNullableString("ServiceComp"),
                            PhoneNum = dataReader.GetNullableString("PhoneNum"),
                            CreateDate = dataReader.GetNullableString("CreateDate"),
                            CreateTime = dataReader.GetNullableString("CreateTime"),
                            BridgeDate = dataReader.GetNullableString("BridgedDate"),
                            BridgeTime = dataReader.GetNullableString("BridgedTime"),
                            EndDate = dataReader.GetNullableString("EndDate"),
                            EndTime = dataReader.GetNullableString("EndTime"),
                            WaitSec = dataReader.GetNullableInt("waitSec"),
                            CallTime = dataReader.GetInt32("callTime"),
                            UserId = dataReader.GetNullableInt("Id"),
                            Fio = dataReader.GetNullableString("Fio")
                        });
                    }
                    dataReader.Close();
                }
                return requests;
            }
        } public IList<StatIvrCallListDto> GetIvrStatCalls(DateTime fromDate,DateTime toDate)
        {
            var sqlQuery = "call CallCenter.StatGetIvrRedirectsNew(@From,@To);";
            using (var cmd =
                new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@From", fromDate);
                cmd.Parameters.AddWithValue("@To", toDate);

                var requests = new List<StatIvrCallListDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        requests.Add(new StatIvrCallListDto
                        {
                            LinkedId = dataReader.GetNullableString("LinkedID"),
                            CallerIdNum = dataReader.GetNullableString("CallerIDNum"),
                            InCreateTime = dataReader.GetDateTime("InCreateTime"),
                            InEndTime = dataReader.GetDateTime("InEndTime"),
                            InBridgedTime = dataReader.GetNullableDateTime("InBridgedTime"),
                            Phone = dataReader.GetNullableString("phone"),
                            HangupCause = dataReader.GetNullableString("hangup_cause"),
                            Result = dataReader.GetNullableString("result"),
                            CreateTime = dataReader.GetDateTime("CreateTime"),
                            EndTime = dataReader.GetDateTime("EndTime"),
                            BridgedTime = dataReader.GetNullableDateTime("BridgedTime"),
                            TalkDuration = dataReader.GetNullableInt("talkDuration"),
                            ClientWaitSec = dataReader.GetInt32("clientWait"),
                            CallDuration = dataReader.GetInt32("callDuration")
                        });
                    }
                    dataReader.Close();
                }
                return requests;
            }
        }
        public IList<DispexForListDto> GetDispexRequests()
        {
            var sqlQuery = "SELECT * FROM Dispex.requests r order by id desc;";
            using (var cmd =
                new MySqlCommand(sqlQuery, _dbConnection))
            {
                var requests = new List<DispexForListDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        requests.Add(new DispexForListDto
                        {
                            Id = dataReader.GetInt32("id"),
                            BitrixId = dataReader.GetNullableString("bitrix_id"),
                            FromPhone = dataReader.GetNullableString("from_phone"),
                            CreateDate = dataReader.GetDateTime("create_date"),
                            StreetName = dataReader.GetNullableString("street_name"),
                            Building = dataReader.GetNullableString("building"),
                            Corpus = dataReader.GetNullableString("corpus"),
                            Flat = dataReader.GetNullableString("flat"),
                            BitrixServiceId = dataReader.GetNullableInt("bitrix_service_id"),
                            BitrixServiceName = dataReader.GetNullableString("bitrix_service_name"),
                            Description = dataReader.GetNullableString("descript"),
                            Status = dataReader.GetNullableString("status"),
                        });
                    }
                    dataReader.Close();
                }
                return requests;
            }
        }
        public IList<RequestForListDto> GetAlertRequestList(int? serviceCompanyId,bool showDoned)
        {
            var sqlQuery =
                @"SELECT R.id,case when count(ra.id)=0 then false else true end has_attach,R.create_time,sp.name as prefix_name,s.name as street_name,h.building,h.corps,at.Name address_type, a.flat,
    R.worker_id, w.sur_name,w.first_name,w.patr_name, case when create_user_id = 0 then cw.id else create_user_id end create_user_id,
    case when create_user_id = 0 then cw.sur_name else u.surname end surname,
    case when create_user_id = 0 then cw.first_name else u.firstname end firstname,
    case when create_user_id = 0 then cw.patr_name else u.patrname end patrname,
    R.execute_date,p.Name Period_Name, R.description,rt.name service_name, rt2.name parent_name, group_concat(distinct cp.Number order by rc.IsMain desc separator ', ') client_phones,
    rating.Name Rating, rtype.Description RatingDesc,
    RS.Description Req_Status,R.to_time, R.from_time, TIMEDIFF(R.to_time,R.from_time) spend_time,
    min(rcalls.uniqueID) recordId, R.alert_time
    FROM CallCenter.Requests R
    join CallCenter.RequestState RS on RS.id = R.state_id
    join CallCenter.Addresses a on a.id = R.address_id
    join CallCenter.AddressesTypes at on at.id = a.type_id
    join CallCenter.Houses h on h.id = house_id
    join CallCenter.Streets s on s.id = street_id
    join CallCenter.StreetPrefixes sp on sp.id = s.prefix_id
    join CallCenter.RequestTypes rt on rt.id = R.type_id
    join CallCenter.RequestTypes rt2 on rt2.id = rt.parrent_id
    left join CallCenter.RequestAttachments ra on ra.request_id = R.id
    left join CallCenter.Workers w on w.id = R.worker_id
    left join CallCenter.RequestContacts rc on rc.request_id = R.id
    left join CallCenter.ClientPhones cp on cp.id = rc.clientPhone_id
    join CallCenter.Users u on u.id = create_user_id
    left join CallCenter.PeriodTimes p on p.id = R.period_time_id
    left join (select request_id,max(id) max_id from CallCenter.RequestRating rr group by request_id) rr_max on rr_max.request_id = R.Id
    left join CallCenter.RequestRating rtype on rtype.id = rr_max.max_id
    left join CallCenter.RatingTypes rating on rtype.rating_id = rating.id
    left join CallCenter.RequestCalls rcalls on rcalls.request_id = R.id
    left join CallCenter.Workers cw on cw.id = R.create_worker_id
    where is_immediate = 1";
                if (serviceCompanyId.HasValue)
                    sqlQuery += $" and h.service_company_id = {serviceCompanyId.Value}";
                if(!showDoned)
                sqlQuery += " and R.state_id in (1,2,5)";
            sqlQuery += " group by R.id order by id desc";
            using (var cmd =
                new MySqlCommand(sqlQuery, _dbConnection))
            {
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
                            Master = dataReader.GetNullableInt("worker_id") != null ? new RequestUserDto
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

        public void PlayRecord(string serverIpAddress, string fileName)
        {
            var localFileName = fileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\").Replace("/", "\\");
            var localFileNameMp3 = localFileName.Replace(".wav", ".mp3");
            if (File.Exists(localFileNameMp3))
                Process.Start(localFileNameMp3);
            else if (File.Exists(localFileName))
                Process.Start(localFileName);
            else
                MessageBox.Show($"Файл с записью недоступен!\r\n{localFileNameMp3}", "Ошибка");

        }
        public string GetRecordFileNameByUniqueId(string uniqueId)
        {
            var sqlQuery = @"select MonitorFile FROM asterisk.ChannelHistory ch
 join CallCenter.RequestCalls rc on ch.UniqueId = rc.UniqueId
 where rc.uniqueID = @UniqueID";
            var result = string.Empty;
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
        public IList<WorkerDto> GetExecuters(int? serviceCompanyId, bool showOnlyExecutors = true)
        {
            var query = @"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,sp.name speciality_name,w.can_assign,w.parent_worker_id,w.send_sms,is_master,is_executer,is_dispetcher,send_notification FROM CallCenter.Workers w
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id
    left join CallCenter.Speciality sp on sp.id = w.speciality_id
    where w.enabled = 1 and w.is_executer = true";
            if (showOnlyExecutors)
                query += " and can_assign = true";
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
                return workers;
            }
        }
        public IList<WorkerDto> GetExecutersByServiceType(int serviceCompanyId, int typeId)
        {
            var query = @"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,
w.speciality_id,sp.name speciality_name,w.can_assign,w.parent_worker_id,w.send_sms,is_master,is_executer,is_dispetcher,send_notification
FROM CallCenter.executer_to_type t
  join CallCenter.Workers w on t.executer_id = w.id
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id
    left join CallCenter.Speciality sp on sp.id = w.speciality_id
    where w.enabled = 1 and
    t.service_company_id = @ServiceCompanyId and t.type_id = @TypeId
group by w.id
order by t.weigth, w.sur_name, w.first_name, w.patr_name";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);
                cmd.Parameters.AddWithValue("@TypeId", typeId);

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
                return workers;
            }
        }
        public IList<WorkerDto> GetAllWorkers(int? serviceCompanyId)
        {
            var query = @"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,sp.name speciality_name,w.can_assign,w.parent_worker_id,w.send_sms,is_master,is_executer,is_dispetcher,w.login,w.send_notification,w.enabled FROM CallCenter.Workers w
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id
    left join CallCenter.Speciality sp on sp.id = w.speciality_id";
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
                            Login = dataReader.GetNullableString("login"),
                            CanAssign = dataReader.GetBoolean("can_assign"),
                            SendSms = dataReader.GetBoolean("send_sms"),
                            AppNotification = dataReader.GetBoolean("send_notification"),
                            IsMaster = dataReader.GetBoolean("is_master"),
                            IsExecuter = dataReader.GetBoolean("is_executer"),
                            IsDispetcher = dataReader.GetBoolean("is_dispetcher"),
                            Enabled = dataReader.GetBoolean("enabled"),
                            ParentWorkerId = dataReader.GetNullableInt("parent_worker_id"),
                        });
                    }
                    dataReader.Close();
                }
                return workers;
            }
        }


        public IList<WorkerDto> GetMasters(int? serviceCompanyId, bool showOnlyExecutors = true)
        {
            var query = @"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,sp.name speciality_name,w.can_assign,w.parent_worker_id,send_sms,is_master,is_executer,is_dispetcher,w.send_notification FROM CallCenter.Workers w
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id
    left join CallCenter.Speciality sp on sp.id = w.speciality_id
    where w.enabled = 1 and w.is_master = 1";
            if (showOnlyExecutors)
                query += " and can_assign = true";
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
                return workers;
            }
        }
        public IList<ScheduleTaskDto> GetScheduleTasks(int workerId, DateTime fromDate, DateTime toDate)
        {
            var query = @"SELECT s.id,w.id worker_id,w.sur_name,w.first_name,w.patr_name,s.request_id,s.from_date,s.to_date,s.event_description FROM CallCenter.ScheduleTasks s
join CallCenter.Workers w on s.worker_id = w.id
where w.id = @WorkerId and s.from_date between @FromDate and @ToDate and deleted = 0;";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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
                return items;
            }
        }
        public ScheduleTaskDto GetScheduleTaskByRequestId(int requestId)
        {
            ScheduleTaskDto result = null;
            var query = @"SELECT s.id,w.id worker_id,w.sur_name,w.first_name,w.patr_name,s.request_id,s.from_date,s.to_date,s.event_description FROM CallCenter.ScheduleTasks s
join CallCenter.Workers w on s.worker_id = w.id
where s.request_id = @RequestId and deleted = 0;";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                
                using (var dataReader = cmd.ExecuteReader())
                {
                    if(dataReader.Read())
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

        public void AddScheduleTask(int workerId, int? requestId, DateTime fromDate, DateTime toDate, string eventDescription)
        {
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.ScheduleTasks (create_date,worker_id,request_id,from_date,to_date,event_description)
 values(sysdate(),@WorkerId,@RequestId,@FromDate,@ToDate,@Desc);", _dbConnection))
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
            catch (Exception exc)
            {
                _logger.Error(exc);
                throw;
            }
        }
        public void DeleteScheduleTask(int sheduleId)
        {
            try
            {
                using (var transaction = _dbConnection.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"update CallCenter.ScheduleTasks set deleted = 1 where id = @Id;", _dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@Id", sheduleId);
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
        
        public IList<WorkerDto> GetWorkersByHouseAndService(int houseId, int parentServiceTypeId, bool showMasters = true)
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
    query += @" group by s.id,s.name ,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,sp.name,w.can_assign,w.parent_worker_id
    order by wh.master_weigth desc;";
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
                            CanAssign = dataReader.GetBoolean("can_assign"),
                            SendSms = dataReader.GetBoolean("send_sms"),
                            AppNotification = dataReader.GetBoolean("send_notification"),
                            ParentWorkerId = dataReader.GetNullableInt("parent_worker_id"),
                        });
                    }
                    dataReader.Close();
                }
                return workers;
            }
        }

        public IList<WorketHouseAndTypeListDto> GetHouseAndTypesByWorkerId(int workerId)
        {
            var query = $@"SELECT wh.id,s.name,h.building,h.corps,r.name type_name,wh.master_weigth FROM CallCenter.WorkerHouseAndType wh
join CallCenter.Houses h on h.id = wh.house_id
join CallCenter.Streets s on s.id = h.street_id
left join CallCenter.RequestTypes r on r.id = wh.type_id
where wh.worker_id = {workerId}";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                var result = new List<WorketHouseAndTypeListDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var corps = dataReader.GetNullableString("corps");
                        corps = corps != null ? $"/{corps}" : "";
                        result.Add(new WorketHouseAndTypeListDto
                        {
                            Id = dataReader.GetInt32("id"),
                            StreetAndHouse = $"{dataReader.GetNullableString("name")} {dataReader.GetNullableString("building")}{corps}",
                            ServiceType = dataReader.GetNullableString("type_name")??"Все",
                            Weigth = dataReader.GetInt32("master_weigth"),
                        });
                    }
                    dataReader.Close();
                }
                return result;
            }
        }
        public IList<HouseDto> GetBindedToWorkerHouse(int workerId)
        {
            var query = @"SELECT w.id binding_id,w.house_id,s.name,h.building,h.corps FROM CallCenter.WebWorkerHouses w
join CallCenter.Houses h on h.id = w.house_id
join CallCenter.Streets s on s.id = h.street_id
where w.worker_id = @WorkerId";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@WorkerId", workerId);
                var result = new List<HouseDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var house = new HouseDto()
                        {
                            Id = dataReader.GetInt32("house_id"),
                            StreetName = dataReader.GetNullableString("name"),
                            Building = dataReader.GetNullableString("building"),
                            Corpus = dataReader.GetNullableString("corps"),
                        };
                        result.Add(house);
                    }
                    dataReader.Close();
                }
                return result;
            }
        }
        public void AddBindedToWorkerHouse(int workerId, int houseId)
        {
            using (var cmd = new MySqlCommand(@"insert into CallCenter.WebWorkerHouses(worker_id, house_id) 
    values(@WorkerId,@HouseId);", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@WorkerId", workerId);
                cmd.Parameters.AddWithValue("@HouseId", houseId);
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteBindedToWorkerHouse(int workerId, int houseId)
        {
            using (var cmd = new MySqlCommand(@"delete from CallCenter.WebWorkerHouses where worker_id = @WorkerId and house_id = @HouseId;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@WorkerId", workerId);
                cmd.Parameters.AddWithValue("@HouseId", houseId);
                cmd.ExecuteNonQuery();
            }
        }


        public IList<WorkerCompanyDto> GetBindedToWorkerCompany(int workerId)
        {
            var query = @"SELECT c.id,sc.Name as company_name,s.Name as speciality_name FROM CallCenter.WorkersCompany c
join CallCenter.ServiceCompanies sc on sc.id = c.service_company_id
join CallCenter.Speciality s on s.id = c.speciality_id
where worker_id = @WorkerId";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@WorkerId", workerId);
                var result = new List<WorkerCompanyDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var house = new WorkerCompanyDto()
                        {
                            Id = dataReader.GetInt32("id"),
                            ServiceCompany = dataReader.GetNullableString("company_name"),
                            Speciality = dataReader.GetNullableString("speciality_name"),
                        };
                        result.Add(house);
                    }
                    dataReader.Close();
                }
                return result;
            }
        }
        public IList<ServiceDto> GetBindedTypeToHouse(int houseId)
        {
            var query = @"SELECT r.id,name,can_send_sms,immediate
 FROM CallCenter.RequestTypesToHouses t
 join CallCenter.RequestTypes r where r.id = t.type_id and t.enabled = 1 and r.enabled = 1 and t.house_id = @HouseId order by name";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
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
                            Immediate = dataReader.GetBoolean("immediate")
                        });
                    }
                    dataReader.Close();
                }
                return services;
            }
        }
        public void AddBindedTypeToHouse(int houseId, int typeId)
        {
            using (var cmd = new MySqlCommand(@"insert into CallCenter.RequestTypesToHouses(type_id, house_id, enabled) 
    values(@TypeId,@HouseId,1);", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@TypeId", typeId);
                cmd.Parameters.AddWithValue("@HouseId", houseId);
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteBindedTypeToHouse(int houseId, int typeId)
        {
            using (var cmd = new MySqlCommand(@"delete from CallCenter.RequestTypesToHouses where house_id = @HouseId and type_id = @TypeId;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@TypeId", typeId);
                cmd.Parameters.AddWithValue("@HouseId", houseId);
                cmd.ExecuteNonQuery();
            }
        }


        public void AddBindedToWorkerCompany(int workerId, int companyId,int? specialityId)
        {
            using (var cmd = new MySqlCommand(@"insert into CallCenter.WorkersCompany(worker_id, service_company_id,speciality_id) 
    values(@WorkerId,@CompanyId,@SpecialityId);", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@WorkerId", workerId);
                cmd.Parameters.AddWithValue("@CompanyId", companyId);
                cmd.Parameters.AddWithValue("@SpecialityId", specialityId);
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteBindedToWorkerCompany(int workerId, int id)
        {
            using (var cmd = new MySqlCommand(@"delete from CallCenter.WorkersCompany where worker_id = @WorkerId and id = @Id;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@WorkerId", workerId);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }


        public void DeleteHouseAndTypesByWorkerId(int recordId)
        {
            using (var cmd = new MySqlCommand(@"delete from CallCenter.WorkerHouseAndType where id = @recordId;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@recordId", recordId);
                cmd.ExecuteNonQuery();
            }
        }

        public void AddHouseAndTypesForWorker(int workerId,int houseId, int? serviceType, int weigth)
        {
            using (var cmd = new MySqlCommand(@"insert into CallCenter.WorkerHouseAndType(worker_id, house_id,type_id, master_weigth) 
    values(@WorkerId,@HouseId,@TypeId,@Weigth);", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@WorkerId", workerId);
                cmd.Parameters.AddWithValue("@HouseId", houseId);
                cmd.Parameters.AddWithValue("@TypeId", serviceType);
                cmd.Parameters.AddWithValue("@Weigth", weigth);
                cmd.ExecuteNonQuery();
            }
        }

        public SmsSettingDto GetSmsSettingsForServiceCompany(int? serviceCompanyId)
        {
            var result = new SmsSettingDto { SendToClient = false, SendToWorker = false };
            if (serviceCompanyId.HasValue)
            {
                using (var cmd = new MySqlCommand(
                            "SELECT S.sms_to_worker, S.sms_to_abonent, S.sms_sender FROM CallCenter.ServiceCompanies S where id = @ID",
                            _dbConnection))
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
            return result;
        }
        public WorkerDto GetWorkerById(int workerId)
        {
            WorkerDto worker = null;
            var query = @"SELECT s.id service_id, s.name service_name,w.id,w.sur_name,w.first_name,w.patr_name,w.phone,w.speciality_id,
    w.can_assign,w.parent_worker_id,w.is_master,w.is_executer,w.is_dispetcher, sp.name speciality_name,send_sms,w.login,w.password,
    w.filter_by_houses,w.can_create_in_web,w.show_all_request,w.show_only_garanty,w.allow_statistics,w.can_set_rating,w.can_close_request,
    w.can_change_executors,w.send_notification,w.enabled,w.show_only_my
    FROM CallCenter.Workers w
    left join CallCenter.ServiceCompanies s on s.id = w.service_company_id
    left join CallCenter.Speciality sp on sp.id = w.speciality_id
    where w.id = @WorkerId";
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


        public IList<StreetDto> GetStreets(int cityId, int? serviceCompanyId = null)
        {
            var sqlQuery = @"SELECT S.id,S.city_id,S.name,P.id as Prefix_id,P.Name as Prefix_Name,P.ShortName FROM CallCenter.Streets S
    join CallCenter.StreetPrefixes P on P.id = S.prefix_id
    where S.enabled = 1 and S.city_id = @CityId
    group by S.id;";
            if (serviceCompanyId.HasValue)
            {
                sqlQuery = @"SELECT S.id,S.city_id,S.name,P.id as Prefix_id,P.Name as Prefix_Name,P.ShortName FROM CallCenter.Streets S
    join CallCenter.StreetPrefixes P on P.id = S.prefix_id
    join CallCenter.Houses h on h.street_id = S.id and h.service_company_id = @ServiceCompanyId
    where S.enabled = 1 and S.city_id = @CityId
    group by S.id;";
            }
                
            using (
                var cmd =
                    new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@CityId", cityId);
                if (serviceCompanyId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId.Value);
                }
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

        public IList<HouseDto> GetHouses(int streetId,int? serviceCompanyId = null)
        {
            var sqlQuery = @"SELECT h.id,h.street_id,s.Name street_name,h.building,h.corps,h.entrance_count,h.flat_count,h.floor_count,have_parking,elevator_count,service_company_id,sс.Name service_company_name,region_id, r.name region_name FROM CallCenter.Houses h
    join CallCenter.Streets s on s.id = h.street_id
    left join CallCenter.CityRegions r on r.id = h.region_id
    left join CallCenter.ServiceCompanies sс on sс.id = h.service_company_id where h.street_id = @StreetId and h.enabled = 1";
            if (serviceCompanyId.HasValue)
            {
                sqlQuery += @" and h.service_company_id = @ServiceCompanyId";
            }


            using (var cmd = new MySqlCommand(sqlQuery,_dbConnection))
            {
                cmd.Parameters.AddWithValue("@StreetId", streetId);
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
                return houses;
            }
        }

        public IList<HouseDto> GetHousesByServiceCompany(int serviceCompanyId)
        {
            using (
                var cmd =
                    new MySqlCommand(
                        @"SELECT h.id,h.street_id,s.Name street_name,h.building,h.corps,h.entrance_count,h.flat_count,h.floor_count,have_parking,elevator_count,service_company_id,sc.Name service_company_name FROM CallCenter.Houses h
    join CallCenter.Streets s on s.id = h.street_id
    join CallCenter.ServiceCompanies sc on sc.id = h.service_company_id where h.service_company_id = @ServiceCompanyId and h.enabled = 1;",
                        _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);
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

        public IList<ServiceDto> GetServices(long? parentId,int? houseId = null)
        {
            if (!parentId.HasValue && houseId.HasValue)
                return GetBindedTypeToHouse(houseId.Value);

            var query = parentId.HasValue
                ? @"SELECT id,name,can_send_sms,immediate FROM CallCenter.RequestTypes R where parrent_id = @ParentId and enabled = 1 order by name"
                : @"SELECT id,name,can_send_sms,immediate FROM CallCenter.RequestTypes R where parrent_id is null and enabled = 1 order by name";
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
                            Name = dataReader.GetString("name"),
                            CanSendSms = dataReader.GetBoolean("can_send_sms"),
                            Immediate = dataReader.GetBoolean("immediate")
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
        public ClientAddressInfoDto GetLastAddressByClientPhone(string phone)
        {
            ClientAddressInfoDto result = null;
            var query = @"SELECT cp.id,h.street_id,h.building,h.corps,a.flat,name,email,addition FROM CallCenter.ClientPhones cp
            join CallCenter.RequestContacts rc on rc.ClientPhone_id = cp.id
            join CallCenter.Requests r on r.id = rc.request_id
            join CallCenter.Addresses a on a.id = r.address_id
            join CallCenter.Houses h on h.id = a.house_id
            where cp.Number = @phone
            order by r.id desc limit 1";
            using (var cmd = new MySqlCommand(query, _dbConnection))
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
        public List<ServiceCompanyDto> GetServiceCompaniesForCalls()
        {
            var query = "SELECT id,name,prefix,short_name,phone FROM CallCenter.ServiceCompanies S where Enabled = 1 and trunk_name is not null order by case when prefix is null then 0 else 1 end,S.Name";
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
                            Name = dataReader.GetNullableString("name"),
                            Prefix = dataReader.GetNullableString("prefix"),
                            Phone = dataReader.GetNullableString("phone"),
                            ShortName = dataReader.GetNullableString("short_name")
                        });
                    }
                    dataReader.Close();
                }
                return companies.ToList();
            }
        }
        public List<RingUpConfigDto> GetRingUpConfigs()
        {
            var query = "SELECT id,name,phone FROM asterisk.RingUpConfigs";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
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
        public List<BlackListPhoneDto> GetBlackListPhones()
        {
            var query = "SELECT id,phone FROM asterisk.BlackList where enabled = 1 order by phone";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                var phones = new List<BlackListPhoneDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        phones.Add(new BlackListPhoneDto
                        {
                            Id = dataReader.GetInt32("id"),
                            Phone = dataReader.GetString("phone")
                        });
                    }
                    dataReader.Close();
                }
                return phones.OrderBy(i => i.Phone).ToList();
            }
        }

        public void AddPhoneToBlackList(string phone)
        {
            using (var cmd = new MySqlCommand(@"insert into asterisk.BlackList(phone,enabled) values(@Phone,1)
 ON DUPLICATE KEY UPDATE enabled = 1;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.ExecuteNonQuery();
            }
        }
        public void DeletePhoneFromBlackList(BlackListPhoneDto phoneDto)
        {
            using (var cmd = new MySqlCommand(@"update asterisk.BlackList set enabled = 0 where phone = @Phone;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Phone", phoneDto.Phone);
                cmd.ExecuteNonQuery();
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
            var query = "SELECT id,name,info, sms_to_worker, sms_to_abonent, sms_sender FROM CallCenter.ServiceCompanies S where Enabled = 1 and id = @ID";
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
                            Name = dataReader.GetString("name"),
                            Info = dataReader.GetNullableString("info"),
                            SendToClient = dataReader.GetBoolean("sms_to_abonent"),
                            SendToWorker = dataReader.GetBoolean("sms_to_worker"),
                            Sender = dataReader.GetNullableString("sms_sender")
                        };
                    }
                    dataReader.Close();
                }
                return serviceCompany;
            }
        }

        public List<WorkerHistoryDto> GetMasterHistoryByRequest(int requestId)
        {
            var query = @"SELECT operation_date, R.worker_id, w.sur_name,w.first_name,w.patr_name, user_id,u.surname,u.firstname,u.patrname FROM CallCenter.RequestWorkerHistory R
 left join CallCenter.Workers w on w.id = R.worker_id
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
                        var workerId = dataReader.GetNullableInt("worker_id");
                        historyDtos.Add(new WorkerHistoryDto
                        {
                            CreateTime = dataReader.GetDateTime("operation_date"),
                            Worker = workerId!= null ? new RequestUserDto
                            {
                                Id = dataReader.GetInt32("worker_id"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                            } : new RequestUserDto { Id = -1, SurName = "Нет мастера" },
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
        public List<WorkerHistoryDto> GetExecuterHistoryByRequest(int requestId)
        {
            var query = @"SELECT operation_date, R.executer_id, w.sur_name,w.first_name,w.patr_name, user_id,u.surname,u.firstname,u.patrname FROM CallCenter.RequestExecuterHistory R
 left join CallCenter.Workers w on w.id = R.executer_id
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
                        var executerId = dataReader.GetNullableInt("executer_id");
                        historyDtos.Add(new WorkerHistoryDto
                        {
                            CreateTime = dataReader.GetDateTime("operation_date"),
                            Worker = executerId!= null ? new RequestUserDto
                            {
                                Id = dataReader.GetInt32("executer_id"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                            } : new  RequestUserDto {Id = -1, SurName = "Нет исполнителя"},
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

        public List<CallsListDto> GetCallList(DateTime fromDate, DateTime toDate, string requestId, int? operatorId, int? serviceCompanyId, string phoneNumber)
        {
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
                if (serviceCompanyId.HasValue)
                {
                    sqlQuery += " and sc.id = @ServiceCompanyId";

                }
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    sqlQuery +=
                        " and (case when C.PhoneNum is not null then C.PhoneNum when(C.CallerIDNum in ('scvip500415','594555')) then C.Exten else C.CallerIDNum end) like @PhoneNumber";
                }
            }
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
                if (serviceCompanyId.HasValue)
                {
                    sqlQuery += " and sc.id = @ServiceCompanyId";

                }
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    sqlQuery +=
                        " and (case when C.PhoneNum is not null then C.PhoneNum when(C.CallerIDNum in ('scvip500415','594555')) then C.Exten else C.CallerIDNum end) like @PhoneNumber";
                }
            }
            sqlQuery += @" group by C.UniqueId
) a
left join CallCenter.Users u on u.id = a.userId";


            using (
            var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
                    if (serviceCompanyId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);
                    }
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        cmd.Parameters.AddWithValue("@PhoneNumber", "%"+phoneNumber+"%");
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
                            redirectPhone = redirectPhone.Substring(position+1);
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
                    return callList;
                }
            }
        }
        public List<CallsListDto> GetCallListByRequestId(int requestId)
        {
            var sqlQuery = @"SELECT rc.id,ch.UniqueID,Direction,PhoneNum CallerIDNum,CreateTime,AnswerTime,EndTime,BridgedTime,
 MonitorFile, timestampdiff(SECOND,ch.BridgedTime,ch.EndTime) AS TalkTime,
(timestampdiff(SECOND,ch.CreateTime,ch.EndTime) - ifnull(timestampdiff(SECOND,ch.BridgedTime,ch.EndTime),0)) AS WaitingTime,
 group_concat(rc.request_id order by rc.request_id separator ', ') AS RequestId
 FROM asterisk.ChannelHistory ch
 join CallCenter.RequestCalls rc on ch.UniqueId = rc.UniqueId
 where rc.request_id = @RequestNum
 group by UniqueId";

            using (
            var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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

        public List<RequestUserDto> GetOperators()
        {
            using (
                var cmd = new MySqlCommand(@"SELECT u.id, SurName, FirstName, PatrName FROM CallCenter.Users u where u.enabled = 1 and u.ShowInForm = 1 order by SurName,FirstName",
                    _dbConnection))
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

        public List<MetersDto> GetMetersByPeriod(int addressId)
        {
            var sqlQuery = @"select id, meters_date, electro_t1, electro_t2, cool_water1, hot_water1, cool_water2, hot_water2, cool_water2, hot_water2, user_id, heating,heating2,heating3,heating4, client_phone_id  from CallCenter.MeterDeviceValues
 where address_id = @AddressId and meters_date > sysdate() - INTERVAL 3 month
 order by meters_date desc";

            using (
            var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
        public List<MetersDto> GetMetersByAddressId(int addressId)
        {
            var sqlQuery = @"select id, meters_date, personal_account, electro_t1, electro_t2, cool_water1, hot_water1, cool_water2, hot_water2, cool_water3, hot_water3, user_id, heating,heating2,heating3,heating4, client_phone_id  from CallCenter.MeterDeviceValues
 where address_id = @AddressId and meters_date > sysdate() - INTERVAL 3 month
 order by meters_date desc";

            using (
            var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
        public List<MeterListDto> GetMetersByDate(int? serviceCompanyId, DateTime fromDate, DateTime toDate)
        {
            var sqlQuery =
                @"select m.id, meters_date,house_id, m.address_id, street_id, electro_t1, electro_t2, cool_water1, hot_water1, cool_water2, hot_water2,cool_water3, hot_water3, user_id, heating,
 a.flat,h.building, h.corps, s.name street_name, sc.name company_name,m.personal_account,m.heating2,m.heating3,m.heating4
 from CallCenter.MeterDeviceValues m
  join CallCenter.Addresses a on a.id = m.address_id
  join CallCenter.Houses h on h.id = house_id
  join CallCenter.Streets s on s.id = street_id
  left join CallCenter.ServiceCompanies sc on sc.id = h.service_company_id
 where meters_date between @FromDate and @ToDate";
            if (serviceCompanyId.HasValue)
                sqlQuery += " and sc.id = @ServiceCompanyId";

            sqlQuery += " order by meters_date desc";

            using (
            var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                if(serviceCompanyId.HasValue)
                    cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate.AddDays(1).AddSeconds(-1));
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
                    return metersDtos;
                }
            }
        }

        public MeterCodeDto GetMeterCodes(int addressId)
        {
            var result = new MeterCodeDto();
            using (var cmd = new MySqlCommand(
                "SELECT * FROM CallCenter.MeterDeviceCodes C where address_id = @AddressId", _dbConnection))
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

        public void DeleteMeter(int meterId)
        {
            using (var cmd = new MySqlCommand(@"delete from CallCenter.MeterDeviceValues where id = @ID and send_date is null;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ID", meterId);
                cmd.ExecuteNonQuery();
            }
        }
        public int? SaveMeterValues(string phoneNumber, int addressId, double electro1, double electro2, double hotWater1, double coldWater1, double hotWater2, double coldWater2, double hotWater3, double coldWater3, double heating, int? meterId, string personalAccount, double heating2, double heating3, double heating4)
        {
            var result = meterId;
            using (var transaction = _dbConnection.BeginTransaction())
            {
                var clientPhoneId = (int?)null;
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    using (var cmd = new MySqlCommand(
                        "SELECT id FROM CallCenter.ClientPhones C where Number = @Phone", _dbConnection))
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
    select LAST_INSERT_ID();", _dbConnection))
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
                        _dbConnection))
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
                        _dbConnection))
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
                        cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                        cmd.Parameters.AddWithValue("@Heating", heating);
                        cmd.Parameters.AddWithValue("@Heating2", heating2);
                        cmd.Parameters.AddWithValue("@Heating3", heating3);
                        cmd.Parameters.AddWithValue("@Heating4", heating4);
                        cmd.Parameters.AddWithValue("@ClentPhoneId", clientPhoneId);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new MySqlCommand("SELECT LAST_INSERT_ID() as id", _dbConnection))
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
        public void SaveMeterCodes(int selectedFlatId, string personalAccount, string electro1Code, string electro2Code, string hotWater1Code, 
            string coldWater1Code, string hotWater2Code, string coldWater2Code, string hotWater3Code, string coldWater3Code, string heatingCode, string heating2Code, string heating3Code, string heating4Code)
        {
            using (var cmd = new MySqlCommand(@"insert into CallCenter.MeterDeviceCodes(address_id,personal_account, electro_t1_code, electro_t2_code, cool_water1_code, hot_water1_code,
cool_water2_code, hot_water2_code,cool_water3_code, hot_water3_code, heating_code, heating2_code, heating3_code, heating4_code)
values(@addressId,@persCode,@el1code,@el2code,@cw1code,@hw1code,@cw2code,@hw2code,@cw3code,@hw3code,@h1code,@h2code,@h3code,@h4code) on duplicate KEY 
UPDATE personal_account = @persCode, electro_t1_code = @el1code, electro_t2_code = @el2code,cool_water1_code = @cw1code, hot_water1_code = @hw1code,cool_water2_code = @cw2code,
hot_water2_code = @hw2code,cool_water3_code = @cw3code,hot_water3_code = @hw3code, heating_code = @h1code, heating2_code = @h2code, heating3_code = @h3code, heating4_code = @h4code;", _dbConnection))
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


        public ServiceCompanyDto ServiceCompanyByIncommingPhoneNumber(string phoneNumber)
        {
            using (var cmd = new MySqlCommand(@"SELECT S.id,S.Name FROM asterisk.ActiveChannels A
            join CallCenter.ServiceCompanies S on S.trunk_name = A.ServiceComp where A.CallerIDNum = @phoneNumber", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        return new ServiceCompanyDto {
                            Id = dataReader.GetInt32("id"),
                            Name = dataReader.GetNullableString("Name")
                        };
                    }
                    dataReader.Close();
                }

            }
            return new ServiceCompanyDto { Id = -1, Name = "неизвестная УК" };
        }

        public void SaveServiceCompany(int? serviceCompanyId, string serviceCompanyName,string info,bool sendSmsToClient,bool sendSmsToWorker,string smsSenderName)
        {
            if (serviceCompanyId.HasValue)
            {
                using (var cmd = new MySqlCommand(@"update CallCenter.ServiceCompanies set Name = @ServiceCompanyName,info=@Info,
 sms_to_worker = @SmsToWorker, sms_to_abonent = @SmsToClient, sms_sender = @SmsSender where id = @ID;", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ID", serviceCompanyId.Value);
                    cmd.Parameters.AddWithValue("@ServiceCompanyName", serviceCompanyName);
                    cmd.Parameters.AddWithValue("@Info", info);
                    cmd.Parameters.AddWithValue("@SmsToClient", sendSmsToClient);
                    cmd.Parameters.AddWithValue("@SmsToWorker", sendSmsToWorker);
                    cmd.Parameters.AddWithValue("@SmsSender", smsSenderName);
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
                using (var cmd = new MySqlCommand(@"insert into CallCenter.Streets(name,prefix_id,city_id) values(@StreetName, @PrefixId, @CityId)
 ON DUPLICATE KEY UPDATE enabled = 1;", _dbConnection))
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
        public void  SaveWorker(int? workerId, int serviceCompanyId,string surName,string firstName,string patrName,string phone,int specialityId,bool canAssign, bool isMaster,
            bool isExecuter, bool isDispetcher, bool sendSms,string login, string password, int? parentWorkerId, bool canSetRating, bool canCloseRequest,
            bool canChangeExecutor, bool canCreateRequest, bool canShowStatistic, bool filterByHouses, bool showAllRequest, bool showOnlyGaranty,bool appNotification,bool enabled,bool showOnlyMy)
        {
            if (workerId.HasValue)
            {
                using (var cmd = new MySqlCommand(@"update CallCenter.Workers set sur_name = @surName,first_name = @firstName,patr_name = @patrName,phone = @phone,service_company_id = @serviceCompanyId, speciality_id = @specialityId, can_assign = @canAssign,
is_master = @isMaster, is_executer = @IsExecuter, is_dispetcher = @IsDispetcher, send_sms = @SendSms,  parent_worker_id = @parentWorkerId,
login = @Login, password = @Password,can_set_rating = @CanSetRating,can_close_request = @CanCloseRequest,can_change_executors = @CanChangeExecutor,
can_create_in_web = @CanCreateRequest, allow_statistics = @CanShowStatistic, filter_by_houses = @FilterByHouses,show_all_request = @ShowAllRequest, send_notification = @AppNotification,
show_only_garanty = @ShowOnlyGaranty, enabled = @enabled, show_only_my = @showOnlyMy where id = @ID;", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ID", workerId.Value);
                    cmd.Parameters.AddWithValue("@surName", surName);
                    cmd.Parameters.AddWithValue("@firstName", firstName);
                    cmd.Parameters.AddWithValue("@patrName", patrName);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@serviceCompanyId", serviceCompanyId);
                    cmd.Parameters.AddWithValue("@specialityId", specialityId);
                    cmd.Parameters.AddWithValue("@canAssign", canAssign);
                    cmd.Parameters.AddWithValue("@isMaster", isMaster);
                    cmd.Parameters.AddWithValue("@IsExecuter", isExecuter);
                    cmd.Parameters.AddWithValue("@IsDispetcher", isDispetcher);
                    cmd.Parameters.AddWithValue("@Login", login);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@SendSms", sendSms);
                    cmd.Parameters.AddWithValue("@AppNotification", appNotification);
                    cmd.Parameters.AddWithValue("@parentWorkerId", parentWorkerId);
                    cmd.Parameters.AddWithValue("@CanCreateRequest", canCreateRequest);
                    cmd.Parameters.AddWithValue("@CanShowStatistic", canShowStatistic);
                    cmd.Parameters.AddWithValue("@FilterByHouses", filterByHouses);
                    cmd.Parameters.AddWithValue("@ShowAllRequest", showAllRequest);
                    cmd.Parameters.AddWithValue("@CanSetRating", canSetRating);
                    cmd.Parameters.AddWithValue("@CanCloseRequest", canCloseRequest);
                    cmd.Parameters.AddWithValue("@CanChangeExecutor", canChangeExecutor);
                    cmd.Parameters.AddWithValue("@ShowOnlyGaranty", showOnlyGaranty);
                    cmd.Parameters.AddWithValue("@enabled", enabled);
                    cmd.Parameters.AddWithValue("@showOnlyMy", showOnlyMy);
                    cmd.ExecuteNonQuery();
                }

            }
            else
            {
                using (var cmd = new MySqlCommand(@"insert into CallCenter.Workers(sur_name,first_name,patr_name,phone,service_company_id,
speciality_id,can_assign, parent_worker_id,is_master,is_executer, is_dispetcher, send_sms,login,password,can_set_rating,can_close_request,
can_change_executors,can_create_in_web, allow_statistics,filter_by_houses, show_all_request, show_only_garanty,send_notification,enabled,show_only_my) 
values(@surName,@firstName,@patrName,@phone,@serviceCompanyId,@specialityId,@canAssign,@parentWorkerId,@isMaster,@IsExecuter,@IsDispetcher,
@SendSms,@Login,@Password, @CanSetRating, @CanCloseRequest, @CanChangeExecutor, @CanCreateRequest, @CanShowStatistic, @FilterByHouses,
@ShowAllRequest, @ShowOnlyGaranty, @AppNotification,@enabled,@showOnlyMy);", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@surName", surName);
                    cmd.Parameters.AddWithValue("@firstName", firstName);
                    cmd.Parameters.AddWithValue("@patrName", patrName);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@serviceCompanyId", serviceCompanyId);
                    cmd.Parameters.AddWithValue("@specialityId", specialityId);
                    cmd.Parameters.AddWithValue("@canAssign", canAssign);
                    cmd.Parameters.AddWithValue("@isMaster", isMaster);
                    cmd.Parameters.AddWithValue("@IsExecuter", isExecuter);
                    cmd.Parameters.AddWithValue("@IsDispetcher", isDispetcher);
                    cmd.Parameters.AddWithValue("@SendSms", sendSms);
                    cmd.Parameters.AddWithValue("@parentWorkerId", parentWorkerId);
                    cmd.Parameters.AddWithValue("@Login", login);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@CanCreateRequest", canCreateRequest);
                    cmd.Parameters.AddWithValue("@CanShowStatistic", canShowStatistic);
                    cmd.Parameters.AddWithValue("@FilterByHouses", filterByHouses);
                    cmd.Parameters.AddWithValue("@ShowAllRequest", showAllRequest);
                    cmd.Parameters.AddWithValue("@CanSetRating", canSetRating);
                    cmd.Parameters.AddWithValue("@CanCloseRequest", canCloseRequest);
                    cmd.Parameters.AddWithValue("@CanChangeExecutor", canChangeExecutor);
                    cmd.Parameters.AddWithValue("@ShowOnlyGaranty", showOnlyGaranty);
                    cmd.Parameters.AddWithValue("@AppNotification", appNotification);
                    cmd.Parameters.AddWithValue("@enabled", enabled);
                    cmd.Parameters.AddWithValue("@showOnlyMy", showOnlyMy);
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
            var query = "SELECT id,name,can_send_sms,immediate FROM CallCenter.RequestTypes  where id = @ID";
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
        public void SaveService(int? serviceId, int? parentId,  string serviceName, bool immediate)
        {
            if (serviceId.HasValue)
            {
                using (var cmd = new MySqlCommand(@"update CallCenter.RequestTypes set Name = @serviceName, parrent_id = @parentId, immediate = @immediate where id = @ID;", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ID", serviceId.Value);
                    cmd.Parameters.AddWithValue("@serviceName", serviceName);
                    cmd.Parameters.AddWithValue("@parentId", parentId);
                    cmd.Parameters.AddWithValue("@immediate", immediate);
                    cmd.ExecuteNonQuery();
                }

            }
            else
            {
                using (var cmd = new MySqlCommand(@"insert into CallCenter.RequestTypes(parrent_id,Name,immediate) values(@parentId, @serviceName, @immediate)
                ON DUPLICATE KEY UPDATE enabled = true;", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@serviceName", serviceName);
                    cmd.Parameters.AddWithValue("@parentId", parentId);
                    cmd.Parameters.AddWithValue("@immediate", immediate);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public HouseDto GetHouseById(int houseId)
        {
            HouseDto house = null;
            using (var cmd = new MySqlCommand(@"SELECT h.id, h.street_id, h.building, h.corps, h.service_company_id, h.entrance_count, h.flat_count, h.floor_count, h.service_company_id, s.Name service_company_name,commissioning_date,have_parking,elevator_count,region_id, r.name region_name FROM CallCenter.Houses h
 left join CallCenter.ServiceCompanies s on s.id = h.service_company_id
 left join CallCenter.CityRegions r on r.id = h.region_id where h.enabled = 1 and h.id = @HouseId", _dbConnection))
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
            return house;
        }

        public void SaveHouse(int? houseId, int streetId, string buildingNumber, string corpus, int serviceCompanyId,
            int? entranceCount, int? floorCount, int? flatsCount,int? elevatorCount, bool haveParking, DateTime? commissioningDate,int? regionId)
        {
            using (var cmd = new MySqlCommand(
                    @"call CallCenter.AddOrUndateHouse2(@houseId,@streetId,@buildingNumber,@corpus,@serviceCompanyId,@entranceCount,@floorCount,@flatsCount,@elevatorCount,@haveParking,@commissioningDate,@regionId);",
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
                cmd.Parameters.AddWithValue("@elevatorCount", elevatorCount);
                cmd.Parameters.AddWithValue("@haveParking", haveParking);
                cmd.Parameters.AddWithValue("@commissioningDate", commissioningDate);
                cmd.Parameters.AddWithValue("@regionId", regionId);
                cmd.ExecuteNonQuery();
            }
        }

        public HouseDto FindHouse(int streetId, string buildingNumber, string corpus)
        {
            HouseDto house = null;
            var sqlQuery =
                @"SELECT h.id, h.street_id, h.building, h.corps, h.service_company_id, h.entrance_count, h.flat_count, h.floor_count,have_parking,elevator_count, h.service_company_id, s.Name service_company_name,region_id, r.name region_name FROM CallCenter.Houses h
 left join CallCenter.ServiceCompanies s on s.id = h.service_company_id
 left join CallCenter.CityRegions r on r.id = h.region_id where h.enabled = 1 and h.street_id = @streetId and building = @buildingNumber";
            if (!string.IsNullOrEmpty(corpus))
                sqlQuery += " and corps = @corpus";
            else
                sqlQuery += " and corps is null";
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
                            RegionId = dataReader.GetNullableInt("region_id"),
                            RegionName = dataReader.GetNullableString("region_name"),
                            ServiceCompanyId = dataReader.GetNullableInt("service_company_id"),
                            ServiceCompanyName = dataReader.GetNullableString("service_company_name"),
                            EntranceCount = dataReader.GetNullableInt("entrance_count"),
                            FlatCount = dataReader.GetNullableInt("flat_count"),
                            FloorCount = dataReader.GetNullableInt("floor_count"),
                            ElevatorCount = dataReader.GetNullableInt("elevator_count"),
                            HaveParking = dataReader.GetBoolean("have_parking"),
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

        public void SetRequestWorkingTimes(int requestId, DateTime fromTime, DateTime toTime, int userId)
        {
            using (var cmd = new MySqlCommand(@"insert into CallCenter.RequestTimesHistory(from_time,to_time,user_id,request_id)
 values(@fromTime,@toTime,@userId,@requestId);", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@requestId", requestId);
                cmd.Parameters.AddWithValue("@fromTime", fromTime);
                cmd.Parameters.AddWithValue("@toTime", toTime);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new MySqlCommand(@"update CallCenter.Requests set from_time = @fromTime, to_time = @toTime where id = @requestId;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@requestId", requestId);
                cmd.Parameters.AddWithValue("@fromTime", fromTime);
                cmd.Parameters.AddWithValue("@toTime", toTime);
                cmd.ExecuteNonQuery();
            }
        }

        public string SaveFile(int requestId,string fileName, Stream fileStream)
        {
            using (var saveService = new WcfSaveService.SaveServiceClient())
            {
                return saveService.UploadFile(FileName: fileName, RequestId:requestId,  FileStream: fileStream);
            }
        }

        public byte[] GetFile(int requestId, string fileName)
        {
            using (var saveService = new WcfSaveService.SaveServiceClient())
            {
                return saveService.DownloadFile(requestId, fileName);
            }

        }

        public List<AttachmentDto> GetAttachments(int requestId)
        {
            return GetAttachmentsCore(requestId, _dbConnection);
        }

        public List<AttachmentDto> GetAttachmentsCore(int requestId,MySqlConnection dbConnection)
        {
            using (
                var cmd = new MySqlCommand(@"SELECT a.id,a.request_id,a.name,a.file_name,a.create_date,u.id user_id,u.SurName,u.FirstName,u.PatrName,
a.worker_id, w.sur_name,w.first_name,w.patr_name
FROM CallCenter.RequestAttachments a
 join CallCenter.Users u on u.id = a.user_id
 left join CallCenter.Workers w on w.id = a.worker_id
where a.deleted = 0 and a.request_id = @requestId", dbConnection))
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

        public void DeleteAttachment(int attachmentId)
        {
            using (var cmd = new MySqlCommand(@"update CallCenter.RequestAttachments set deleted = 1 where id = @attachId;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@attachId", attachmentId);
                cmd.ExecuteNonQuery();
            }

        }
        public void AddAttachmentToRequest(int requestId, string fileName, string name = "")
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
            AttachFileToRequest(AppSettings.CurrentUser.Id,requestId, name, newFile);
        }

        public void AttachFileToRequest(int userId, int requestId, string fileName, string generatedFileName)
        {
            using (
                var cmd =
                    new MySqlCommand(@"insert into CallCenter.RequestAttachments(request_id,name,file_name,create_date,user_id)
 values(@RequestId,@Name,@FileName,sysdate(),@userId);", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                cmd.Parameters.AddWithValue("@Name", fileName);
                cmd.Parameters.AddWithValue("@FileName", generatedFileName);
                cmd.ExecuteNonQuery();
            }
        }

        public List<EquipmentDto> GetEquipments()
        {
            var query = "SELECT e.id,t.name type_name,e.name FROM CallCenter.Equipments e join CallCenter.EquipmentTypes t on t.id = e.type_id order by t.name,e.name";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                var equipment = new List<EquipmentDto>();
                equipment.Add(new EquipmentDto(){Id=null,Name = "Нет"});
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
                return equipment;
            };
        }

        public void AddCallToRequest(int requestId, string callUniqueId)
        {
            if (requestId <= 0 || string.IsNullOrEmpty(callUniqueId))
                return;
            using (var cmd =
                new MySqlCommand(
                    "insert into CallCenter.RequestCalls(request_id,uniqueID) values(@Request, @UniqueId) ON DUPLICATE KEY UPDATE uniqueID = @UniqueId",
                    _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Request", requestId);
                cmd.Parameters.AddWithValue("@UniqueId", callUniqueId);
                cmd.ExecuteNonQuery();
            }
        }

        public void AddCallHistory(int requestId, string callUniqueId, int userId, string callId,string methodName)
            {
                if (requestId <= 0 || string.IsNullOrEmpty(callUniqueId))
                    return;

                using (var cmd =
                    new MySqlCommand(@"insert into CallCenter.RequestCallsHistory (request_id, unique_Id, add_date, user_id, call_id,method_name)
 values(@Request, @UniqueId,sysdate(),@UserID,@CallId,@MethodName)", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@Request", requestId);
                    cmd.Parameters.AddWithValue("@UniqueId", callUniqueId);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@CallId", callId);
                    cmd.Parameters.AddWithValue("@MethodName", methodName);
                    cmd.ExecuteNonQuery();
                }
        }
        public void SendSms(int requestId, string sender, string phone, string message,bool isClient)
        {
            if (requestId <= 0 || string.IsNullOrEmpty(phone) || phone.Length < 10 || string.IsNullOrEmpty(sender))
                return;
            var smsCount = 0;
            using (
                var cmd =
                    new MySqlCommand(
                        "SELECT count(1) as count FROM CallCenter.SMSRequest S where request_id = @RequestId and phone = @Phone and create_date > sysdate() - interval 5 minute;",
                        _dbConnection))
            {
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                cmd.Parameters.AddWithValue("@Phone", phone);

                using (var dataReader = cmd.ExecuteReader())
                {
                    dataReader.Read();
                    smsCount = dataReader.GetInt32("count");
                }
            }
            if(smsCount > 0)
                return;

            using (var cmd =
                new MySqlCommand("insert into CallCenter.SMSRequest(request_id,sender,phone,message,create_date, is_client) values(@Request, @Sender, @Phone,@Message,sysdate(),@IsClient)", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Request", requestId);
                cmd.Parameters.AddWithValue("@Sender", sender);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@IsClient", isClient);
                cmd.ExecuteNonQuery();
            }
        }

        public List<SmsListDto> GetSmsByRequestId(int requestId)
        {
            using (var cmd = new MySqlCommand("select id,sender,phone,message,create_date,state_desc, is_client,price*sms_count price from CallCenter.SMSRequest where request_id = @RequestId order by id", _dbConnection))
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
                            ClientOrWorker = dataReader.GetBoolean("is_client")?"Жилец":"Испол."
                        });
                    }
                    dataReader.Close();
                    return alertTypeDtos;
                }
            }

        }
        public List<RingUpHistoryDto> GetRingUpHistory(DateTime fromDate)
        {
            using (var cmd = new MySqlCommand("CALL asterisk.GetRingUpHistory2(@fromDate)", _dbConnection))
            {
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

        }public List<RingUpInfoDto> GetRingUpInfo(int id)
        {
            using (var cmd = new MySqlCommand("CALL CallCenter.GetRingUpInfo(@RingUpId)", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@RingUpId", id);
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
        public void DeleteNotAnswered()
        {

            using (var cmd =
                new MySqlCommand("delete FROM asterisk.NotAnsweredQueue where CreateTime < sysdate() - interval 1 day", _dbConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public List<AlertTypeDto> GetAlertTypes()
        {
            using (
                var cmd = new MySqlCommand("select id,name from CallCenter.AlertType where enabled = 1", _dbConnection))
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
        public List<AlertServiceTypeDto> GetAlertServiceTypes()
        {
            using (
                var cmd = new MySqlCommand("select id,name from CallCenter.AlertServiceType where enabled = 1 order by order_num", _dbConnection))
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

        public int AlertCountByHouseId(int housId)
        {
            int result = 0;
            var sqlQuery = @"SELECT count(1) alert_count FROM CallCenter.Alerts a
 where a.house_id = @HouseId and (end_date is null or a.end_date > sysdate())";
            using (
                var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@HouseId", housId);

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

        public List<AlertDto> GetAlerts(DateTime fromDate, DateTime toDate,int? houseId, bool onlyActive = true)
        {
            var sqlQuery = @"SELECT a.id alert_id,s.id street_id, s.name street_name,h.id house_id, h.building,h.corps,a.start_date,a.end_date,a.description,
 at.id alert_type_id, at.name alert_type_name, a.alert_service_type_id,ast.name alert_service_type_name,
 u.id user_id,u.SurName,u.FirstName,u.PatrName,a.create_date
 FROM CallCenter.Alerts a
 join CallCenter.Houses h on h.id = a.house_id
 join CallCenter.Streets s on s.id = h.street_id
 join CallCenter.AlertType at on at.id = a.alert_type_id
 join CallCenter.AlertServiceType ast on ast.id = a.alert_service_type_id
 join CallCenter.Users u on u.id = a.create_user_id
 where 1 = 1";
            if (onlyActive)
                sqlQuery += " and (end_date is null or a.end_date > sysdate())";
            else
            {
                sqlQuery += @" and (end_date is null or a.end_date between @FromDate and @ToDate)
 and (start_date between @FromDate and @ToDate)";
            }
            if(houseId.HasValue)
                sqlQuery += " and h.id = @HauseId";
            using (
                var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
                    return alertDtos;
                }
            }
        }
        public void SaveAlert(AlertDto alert)
        {
            if (alert.Id == 0)
            {
                using (var cmd = new MySqlCommand(@"insert into CallCenter.Alerts(house_id, alert_type_id, alert_service_type_id, create_date, create_user_id, start_date, end_date, description)
 values(@HouseId,@TypeId,@ServiceId,sysdate(),@UserId,@StartDate,@EndDate,@Desc);", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@HouseId", alert.HouseId);
                    cmd.Parameters.AddWithValue("@TypeId", alert.Type.Id);
                    cmd.Parameters.AddWithValue("@ServiceId", alert.ServiceType.Id);
                    cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                    cmd.Parameters.AddWithValue("@StartDate", alert.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", alert.EndDate);
                    cmd.Parameters.AddWithValue("@Desc", alert.Description);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (var cmd = new MySqlCommand(@"update CallCenter.Alerts set start_date = @StartDate, end_date = @EndDate, description = @Desc where id = @alertId;", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@alertId", alert.Id);
                    cmd.Parameters.AddWithValue("@StartDate", alert.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", alert.EndDate);
                    cmd.Parameters.AddWithValue("@Desc", alert.Description);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public List<UserDto> GetUsers()
        {
            using (
                 var cmd = new MySqlCommand("SELECT U.id, U.SurName, U.FirstName, U.PatrName, U.Login FROM CallCenter.Users U where ShowInForm = 1", _dbConnection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    var users = new List<UserDto>();
                    while (dataReader.Read())
                    {
                        users.Add(new UserDto
                        {
                            Id = dataReader.GetInt32("id"),
                            SurName = dataReader.GetString("SurName"),
                            FirstName = dataReader.GetNullableString("FirstName"),
                            PatrName = dataReader.GetNullableString("PatrName"),
                            Login = dataReader.GetString("Login"),
                        });
                    }
                    dataReader.Close();
                    return users;
                }
            }
        }

        public AlertTimeDto[] GetAlertTimes(bool isImmediate)
        {
            using (var cmd = new MySqlCommand("SELECT id,name,add_minutes FROM CallCenter.AlertTimes where is_immediate = @Immediate and enabled = 1 order by id", _dbConnection))
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

        public void SendAlive()
        {
            using (
                var cmd =
                    new MySqlCommand(@"call CallCenter.SendAliveAndSip(@UserId,@Sip)", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@UserId", AppSettings.CurrentUser.Id);
                cmd.Parameters.AddWithValue("@Sip", AppSettings.SipInfo.SipUser);
                cmd.ExecuteNonQuery();
            }
        }

        public void RequestChangeAddress(int requestId, int addressId)
        {
            using (
                var cmd =
                    new MySqlCommand(@"call CallCenter.ChangeAddress(@RequestId,@Address)", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                cmd.Parameters.AddWithValue("@Address", addressId);
                cmd.ExecuteNonQuery();
            }
        }

        public void SaveRingUpList(int configId, List<RingUpImportDto> records)
        {
            int newId;
            if(records.Count==0)
                return;
            using ( var cmd = new MySqlCommand(@"insert into asterisk.RingUpList(config_id,call_time,state,exten) select id,sysdate(),2,exten from asterisk.RingUpConfigs a where a.id = @Config;
    select LAST_INSERT_ID();", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Config", configId);
                newId = Convert.ToInt32(cmd.ExecuteScalar());
            }
            foreach (var item in records)
            {
                using (var cmd = new MySqlCommand(@"call asterisk.InsertDolgRingPhone(@ListId, @Phone, @Dolg);", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ListId", newId);
                    cmd.Parameters.AddWithValue("@Phone", item.Phone);
                    cmd.Parameters.AddWithValue("@Dolg", item.Dolg);
                    cmd.ExecuteNonQuery();
                }
            }
            using (var cmd = new MySqlCommand(@"update asterisk.RingUpList set state = 0 where id = @ListId;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ListId", newId);
                cmd.ExecuteNonQuery();
            }

        }

        public List<WorkerDto> GetWorkerInfoWithParrents(int workerId)
        {
            var result = new List<WorkerDto>();
            WorkerDto worker = null;
            do
            {
                if(worker==null)
                    worker = GetWorkerById(workerId);
                else if(worker.ParentWorkerId.HasValue)
                    worker = GetWorkerById(worker.ParentWorkerId.Value);
                result.Add(worker);
            } while (worker.ParentWorkerId != null);
            return result;
        }
        public void AbortRingUp(int rintUpId)
        {
            using (var cmd = new MySqlCommand(@"update asterisk.RingUpList set state = 3 where id = @ListId;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ListId", rintUpId);
                cmd.ExecuteNonQuery();
            }

        }
        public void ContinueRingUp(int rintUpId)
        {
            using (var cmd = new MySqlCommand(@"update asterisk.RingUpList set state = 1 where id = @ListId;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ListId", rintUpId);
                cmd.ExecuteNonQuery();
            }

        }

        public void SaveServiceCompanyAdvancedInfo(int id, string savedFlow)
        {
            using (var cmd = new MySqlCommand(@"update CallCenter.ServiceCompanies set advanced_info = @Info where id = @Id",
                        _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Info", savedFlow);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public string GetServiceCompanyAdvancedInfo(int id)
        {
            string result;
            using (var cmd = new MySqlCommand("SELECT id,advanced_info FROM CallCenter.ServiceCompanies where id = @Id",_dbConnection))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                using (var dataReader = cmd.ExecuteReader())
                {
                    dataReader.Read();
                    var ret_id = dataReader.GetInt32("id");
                    result = dataReader.GetNullableString("advanced_info");
                }
                return result;
            }
        }

        public void DeleteCallListRecord(int recordId)
        {
            using (var cmd = new MySqlCommand(@"delete from CallCenter.RequestCalls where id = @ID;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ID", recordId);
                cmd.ExecuteNonQuery();
            }

        }

        public List<ExecuterToServiceCompanyDto> LoadExecuterBinding(int serviceCompanyId)
        {
            using (var cmd = new MySqlCommand(@"SELECT e.id,e.service_company_id,type_id,e.executer_id,e.weigth,
 t.name type_name, w.sur_name, w.first_name, w.patr_name
 FROM CallCenter.executer_to_type e
 join CallCenter.RequestTypes t on t.id = e.type_id
 join CallCenter.Workers w on w.id = e.executer_id
 where e.service_company_id = @CompanyId order by t.name, w.sur_name, w.first_name, w.patr_name", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@CompanyId", serviceCompanyId);

                using (var dataReader = cmd.ExecuteReader())
                {
                    var binding = new List<ExecuterToServiceCompanyDto>();
                    while (dataReader.Read())
                    {
                        binding.Add(new ExecuterToServiceCompanyDto
                        {
                            Id = dataReader.GetInt32("id"),
                            TypeId = dataReader.GetInt32("type_id"),
                            Type = dataReader.GetString("type_name"),
                            ServiceCompanyId =  dataReader.GetInt32("service_company_id"),
                            Executer = new WorkerDto
                            {
                                Id = dataReader.GetInt32("executer_id"),
                                SurName = dataReader.GetString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                            },
                            Weigth = dataReader.GetInt32("weigth"),
                        });
                    }
                    dataReader.Close();
                    return binding;
                }
            }
        }
        public void AddExecuterBinding(int serviceCompanyId, int serviceTypeId, int executerId, int weigth)
        {
            using (var cmd = new MySqlCommand(@"insert into CallCenter.executer_to_type(service_company_id,type_id,executer_id,weigth)
    values(@ServiceCompanyId,@TypeId,@ExecuterId,@Weigth)",
                        _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ServiceCompanyId", serviceCompanyId);
                cmd.Parameters.AddWithValue("@TypeId", serviceTypeId);
                cmd.Parameters.AddWithValue("@ExecuterId", executerId);
                cmd.Parameters.AddWithValue("@Weigth", weigth);
                cmd.ExecuteNonQuery();
            }
        }
        public void DropExecuterBinding(int bindId)
        {
            using (var cmd = new MySqlCommand(@"delete from CallCenter.executer_to_type where id = @Id",
                        _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Id", bindId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<CityRegionDto> GetCityRegions()
        {
            var query = "SELECT id,name FROM CallCenter.CityRegions S where enabled = 1 order by S.Name";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                var regions = new List<CityRegionDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        regions.Add(new CityRegionDto
                        {
                            Id = dataReader.GetInt32("id"),
                            Name = dataReader.GetString("name")
                        });
                    }
                    dataReader.Close();
                }
                return regions.OrderBy(i => i.Name).ToList();
            }
        }
        public void AddCallToMeter(int? meterId, string callUniqueId)
        {

            if (meterId.HasValue && !string.IsNullOrEmpty(callUniqueId))
            {
                using (var cmd =
                        new MySqlCommand("insert into CallCenter.MeterCalls(meter_id,uniqueID,insert_date) values(@MeterId, @UniqueId,sysdate())", _dbConnection))
                {
                    cmd.Parameters.AddWithValue("@MeterId", meterId.Value);
                    cmd.Parameters.AddWithValue("@UniqueId", callUniqueId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

}

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using DevinoSender.Devino;
using NLog;

namespace DevinoSender
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger _logger;

        private void App_OnStartup(object app_sender, StartupEventArgs e)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Debug("Run");
            var server = ConfigurationManager.AppSettings["CallCenterIP"];
            var connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", server,
                "asterisk", "mysqlasterisk", "asterisk");
            var dbConnection = new MySqlConnection(connectionString);
            dbConnection.Open();

            var sender = new Devino.SmsServiceSoapClient();
            var sessionID = sender.GetSessionID(ConfigurationManager.AppSettings["DevinoUser"],
                ConfigurationManager.AppSettings["DevinoPassword"]);
            //Проверка баланса
            var balance = sender.GetBalance(sessionID);
            if (balance < 5) return;
            //Запрет отправки смс
            var smsList = GetSendSmsList(dbConnection);
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSend"]) == false)
            {

                foreach (var sms in smsList)
                {
                    using (
                        var cmd =
                            new MySqlCommand(
                                @"update CallCenter.SMSRequest set message_id = -1,state_id=0,sms_count=0 where id = @smsId;",
                                dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@smsId", sms.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return;
            }
            foreach (var sms in smsList)
            {
                try
                {

                    var message = new Message
                    {
                        Data = sms.Message,
                        SourceAddress = sms.Sender,
                        DestinationAddresses = new ArrayOfString() {sms.Phone}
                    };
                    var messageIds = sender.SendMessage(sessionID, message);

                    //var messageIds = sender.SendMessageByTimeZone(sessionID, sms.Sender, sms.Phone, sms.Message, DateTime.Now.AddMinutes(2), 240);
                    if (messageIds != null && messageIds.Count > 0)
                    {
                        using (
                            var cmd =
                                new MySqlCommand(
                                    @"update CallCenter.SMSRequest set message_id = @MessageId,sms_count=@SmsCount where id = @smsId;",
                                    dbConnection))
                        {
                            cmd.Parameters.AddWithValue("@MessageId", messageIds[0]);
                            cmd.Parameters.AddWithValue("@SmsCount", messageIds.Count);
                            cmd.Parameters.AddWithValue("@smsId", sms.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception exception)
                {
                    _logger.Error(exception);
                    _logger.Error($"Phone={sms.Phone}; Sender = {sms.Sender}; Message = {sms.Message}");
                }

            }
            var getStateList = GetSendSmsList(dbConnection, true);
            foreach (var sms in getStateList)
            {
                try
                {
                    var state = sender.GetMessageState(sessionID, Convert.ToInt64(sms.DevinoMessageId));
                    using (
                        var cmd =
                            new MySqlCommand(@"update CallCenter.SMSRequest set state_id = @StateId,state_desc = @StateDesc,
    date_utc = @Date, price = @Price where id = @smsId;", dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@StateId", state.State);
                        cmd.Parameters.AddWithValue("@StateDesc", state.StateDescription);
                        cmd.Parameters.AddWithValue("@Price", state.Price);
                        cmd.Parameters.AddWithValue("@Date", state.TimeStampUtc);
                        cmd.Parameters.AddWithValue("@smsId", sms.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception exception)
                {
                    _logger.Error(exception);
                    _logger.Error($"DevinoMessageId={sms.DevinoMessageId}");
                }
            }
            dbConnection.Close();

            Application.Current.Shutdown();
        }


        public static List<SmsDto> GetSendSmsList(MySqlConnection dbConnection, bool inWork = false)
        {
            var sql = @"SELECT S.id, S.request_id, S.sender, S.phone, S.message, S.state_id, S.state_desc,
    S.price, S.message_id, S.date_utc, S.create_date FROM CallCenter.SMSRequest S ";
            if (inWork)
                sql += "where message_id is not null and message_id > -1 and (state_id is null or (state_id < 0 or state_id > 100)) and create_date > sysdate() - interval 5 Day";
            else
                sql += "where message_id is null and S.create_date > sysdate() - interval 1 hour";
            using (
                var cmd = new MySqlCommand(sql, dbConnection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    var smsList = new List<SmsDto>();
                    while (dataReader.Read())
                    {
                        smsList.Add(new SmsDto
                        {
                            Id = dataReader.GetInt32("id"),
                            RequestId = dataReader.GetInt32("request_id"),
                            Sender = dataReader.GetString("sender"),
                            Phone = dataReader.GetString("phone"),
                            Message = dataReader.GetString("message"),
                            StateDescription = dataReader.GetNullableString("state_desc"),
                            StateId = dataReader.GetNullableInt("state_id"),
                            DevinoMessageId = dataReader.GetNullableString("message_id"),
                            Price = dataReader.GetNullableDecimal("price"),
                            UtcDate = dataReader.GetNullableDateTime("date_utc"),
                            CreateDate = dataReader.GetDateTime("create_date")
                        });
                    }
                    dataReader.Close();
                    return smsList;
                }
            }
        }
    }
}


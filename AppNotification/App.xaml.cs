using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using MySql.Data.MySqlClient;
using NLog;
using RestSharp;
using RestSharp.Authenticators;
using DataFormat = System.Windows.DataFormat;

namespace AppNotification
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger _logger;
        private static string _oneSignalKey;

        private void App_OnStartup(object app_sender, StartupEventArgs e)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Debug("Run");
            try
            {

                var server = ConfigurationManager.AppSettings["CallCenterIP"];
                _oneSignalKey = ConfigurationManager.AppSettings["OneSignalKey"];
                var connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", server,
                    "asterisk", "mysqlasterisk", "asterisk");
                var dbConnection = new MySqlConnection(connectionString);
                dbConnection.Open();

                var notificationList = GetNotificationList(dbConnection);
                foreach (var notification in notificationList)
                {
                    try
                    {
                        var result = SendNotification(notification.Message, notification.WorkerGuid,notification.RequestId);
                        if (result.StartsWith("{\"id\":"))
                        {
                            using (var cmd = new MySqlCommand(
                                        @"update CallCenter.App_Notifications set sended_date = sysdate(),is_sended=1 where id = @Id;",
                                        dbConnection))
                            {
                                cmd.Parameters.AddWithValue("@Id", notification.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception);
                        _logger.Error($"Error NotificationId={notification.Id};");
                    }

                }
                using (var cmd = new MySqlCommand(@"delete from CallCenter.App_Notifications where sended_date is null and client_send_date is null and web_send_date is null and insert_date < sysdate() - INTERVAL 30 MINUTE;",
                                        dbConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                dbConnection.Close();
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
            }
            Application.Current.Shutdown();
        }

        public static string SendNotification(string message,string dest, int? requestId)
        {
            var saveSampleUrl = "https://onesignal.com/api/v1/notifications";
            //var saveSampleUrl = "http://web.dispex.ru:5000";

            var client = new RestClient(saveSampleUrl);

            var request = new RestRequest(Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"Basic {_oneSignalKey}");
            //request.AddHeader("Authorization", "Basic Y2M3YjMyY2YtODUyZS00M2YyLWFjN2UtMWU4NjI0Y2Y5YjJi");
            //request.AddHeader("Authorization", "Basic M2FkNzJkMmYtZWJjNS00NDc4LTk2ZGYtNWRiZWJlNDVkMTNj");
            
            //request.AddHeader("Authorization", "Basic MmJlODRiN2ItODYxMC00MThiLWJmZjItNDIwZmRkMzgwOTMx");
            var discar = new MessageDto
            {
                app_id = "956627ad-9954-433a-be60-5814302bdbba",
                contents = new Content()
                {
                    en = message,
                },
                data = new Data()
                {
                    requestId = requestId.ToString()
                },
                filters = new Filter[] {new Filter()
                {
                    field = "tag",
                    key = "push_id",
                    relation = "=",
                    value = dest
                } }
            };
            var dataRequest = request.AddJsonBody(discar);

            var responce = client.Execute(dataRequest);
            return responce.Content;
        }

        public static List<NotificationDto> GetNotificationList(MySqlConnection dbConnection)
        {
            var sql = @"SELECT w.guid,n.* FROM CallCenter.App_Notifications n
join CallCenter.Workers w on w.id = n.worker_id
where w.send_notification = 1 and n.sended_date is null and n.insert_date > AddDate(sysdate(), interval -1 hour);";
            using (var cmd = new MySqlCommand(sql, dbConnection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    var smsList = new List<NotificationDto>();
                    while (dataReader.Read())
                    {
                        smsList.Add(new NotificationDto
                        {
                            Id = dataReader.GetInt32("id"),
                            RequestId = dataReader.GetNullableInt("request_id"),
                            Message = dataReader.GetNullableString("description"),
                            WorkerGuid = dataReader.GetNullableString("guid")
                        });
                    }
                    dataReader.Close();
                    return smsList;
                }
            }
        }
    }
}


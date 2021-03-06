﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Windows;
using MySql.Data.MySqlClient;
using NLog;
using RestSharp;

namespace AppNotificationForWeb
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
            //Set SSL/TLS ver 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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
                        var result = SendOwnNotification(notification.WorkerGuid, notification.Info, notification.MessageType, notification.RequestId);
                        if (result.StartsWith("{\"id\":") || result.Equals("OK"))
                        {
                            using (var cmd = new MySqlCommand(
                                @"update CallCenter.App_Notifications set web_send_date = sysdate(),is_sended=1 where id = @Id;",
                                dbConnection))
                            {
                                cmd.Parameters.AddWithValue("@Id", notification.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        //var result = SendNotification(notification.Message, notification.WorkerGuid,notification.RequestId);
                        //if (result.StartsWith("{\"id\":"))
                        //{
                        //    using (var cmd = new MySqlCommand(
                        //                @"update CallCenter.App_Notifications set web_send_date = sysdate(),is_sended=1 where id = @Id;",
                        //                dbConnection))
                        //    {
                        //        cmd.Parameters.AddWithValue("@Id", notification.Id);
                        //        cmd.ExecuteNonQuery();
                        //    }
                        //}
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

                app_id = "0e854521-3f11-4cb9-9b27-8585c9d94a5c",
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
        public static string SendOwnNotification(string userGuid, string message, int messageType, int? requestId)
        {
            var saveSampleUrl = "https://dispex.org:5000/v3/notification/web";
            //var saveSampleUrl = "http://web.dispex.ru:5000";

            var client = new RestClient(saveSampleUrl);

            var request = new RestRequest(Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"Basic {_oneSignalKey}");
            //request.AddHeader("Authorization", "Basic Zjk0ZDNmZjMtMDc4MC00Yjc5LWIyZGYtYzg4ZTk2MzAyYjhm");
            var discar = new NewMessageDto
            {
                pushId = userGuid,
                Id = requestId ?? 0,
                type = messageType,
                mode = "REQUEST",
                data = new NewData()
                {
                    text = message
                }
            };
            var dataRequest = request.AddJsonBody(discar);

            var responce = client.Execute(dataRequest);
            if (!responce.IsSuccessful)
            {
                _logger.Debug(responce.ErrorMessage);
                throw responce.ErrorException;
            }
            return responce.Content;
        }
        public static List<NotificationDto> GetNotificationList(MySqlConnection dbConnection)
        {
            var sql = @"SELECT w.guid,n.* FROM CallCenter.App_Notifications n
join CallCenter.Workers w on w.id = n.worker_id
where w.send_notification = 1 and n.web_send_date is null and n.insert_date > AddDate(sysdate(), interval -1 hour);";
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
                            MessageType = dataReader.GetInt32("type_id"),
                            Message = dataReader.GetNullableString("description"),
                            WorkerGuid = dataReader.GetNullableString("guid"),
                            Info = dataReader.GetNullableString("info")
                        });
                    }
                    dataReader.Close();
                    return smsList;
                }
            }
        }
    }
}


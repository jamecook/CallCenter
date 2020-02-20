using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using MySql.Data.MySqlClient;

namespace RequestSync
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = "192.168.0.130";                                                                                                                              
            var connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", server,
                "asterisk", "mysqlasterisk", "asterisk");

            var requests = GetRequests(connectionString, null);

            SaveToStatBase(requests);
            return;
            //ToGoogleSheets(requests, "1Vq3tnDt6QqnZWsOImit5y-0KLTH95GLhAls5k8lbbmI"); //Requests3  https://docs.google.com/spreadsheets/d/1Vq3tnDt6QqnZWsOImit5y-0KLTH95GLhAls5k8lbbmI/edit?usp=sharing

            ToGoogleSheets(requests, "120c2EQ6a7qZNVug--UNHL98jDx-X8w0_LN3yyZd3Vxg"); //Req20180717 https://docs.google.com/spreadsheets/d/120c2EQ6a7qZNVug--UNHL98jDx-X8w0_LN3yyZd3Vxg/edit?usp=sharing
                                                                                      //SaveToStatBase(requests);

            
            if (false)
            {
                var requestsSibEnergo = GetRequests(connectionString, "СибЭнергоСервис УК ООО");
                ToGoogleSheets(requestsSibEnergo, "1wzjPS5mFotIH8koSyxxv_uVOezRV5VNx1AtKB032WJQ"); //RequestsSibEnergo  https://docs.google.com/spreadsheets/d/1wzjPS5mFotIH8koSyxxv_uVOezRV5VNx1AtKB032WJQ/edit?usp=sharing
            }
            if (true)
            {
                var requestsSibEnergo = GetRequests(connectionString, "ЭНКО");
                ToGoogleSheets(requestsSibEnergo, "1mqht3bC129p3GbTARIcxVI0LHgwvwMclTY7emv4z1gY"); //ReqEnko  https://docs.google.com/spreadsheets/d/1mqht3bC129p3GbTARIcxVI0LHgwvwMclTY7emv4z1gY/edit?usp=sharing
            }
        }

        private static void SaveToStatBase(List<RequestForListDto> requests)
        {
            var now = DateTime.Now;
            var server = "192.168.1.124";
            var connectionString = string.Format("server={0};uid={1};pwd={2};database={3};port=3306;charset=utf8", server,
                "stat", "stat", "callcenter");
            using (var dbConnection = new MySqlConnection(connectionString))
            {
                dbConnection.Open();
                using (var transaction = dbConnection.BeginTransaction())
                {
                    using (var cmd = new MySqlCommand("delete from callcenter.requeststat", dbConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    foreach (var request in requests)
                    {
                        using (
                            var cmd = new MySqlCommand(@"insert into callcenter.requeststat
(id,has_attach,create_time,prefix_name,street_name,building,corps,address_type,flat,worker_id,sur_name,first_name,patr_name,create_user_id,surname,
firstname,patrname,execute_date,Period_Name,description,service_name,parent_name,client_phones,clinet_fio,Rating,RatingDesc,Req_Status,to_time,
from_time,spend_time,bad_work,garanty,retry,recordId,alert_time,service_company_name,executer_id,exec_sur_name,exec_first_name,exec_patr_name,
address_id,house_id,close_date,done_date,is_immediate,is_chargeable,create_by_client)
values
(@id,@has_attach,@create_time,@prefix_name,@street_name,@building,@corps,@address_type,@flat,@worker_id,@sur_name,@first_name,@patr_name,
@create_user_id,@surname,@firstname,@patrname,@execute_date,@Period_Name,@description,@service_name,@parent_name,@client_phones,@clinet_fio,
@Rating,@RatingDesc,@Req_Status,@to_time,@from_time,@spend_time,@bad_work,@garanty,@retry,@recordId,@alert_time,@service_company_name,
@executer_id,@exec_sur_name,@exec_first_name,@exec_patr_name,@address_id,@house_id,@close_date,@done_date,@isImmediate,@isChargeable,@createByClient)",
                                    dbConnection))
                        {
                            cmd.Parameters.AddWithValue("@id", request.Id);
                            cmd.Parameters.AddWithValue("@has_attach", request.HasAttachment);
                            cmd.Parameters.AddWithValue("@create_time", request.CreateTime);
                            cmd.Parameters.AddWithValue("@prefix_name", request.StreetPrefix);
                            cmd.Parameters.AddWithValue("@street_name", request.StreetName);
                            cmd.Parameters.AddWithValue("@building", request.Building);
                            cmd.Parameters.AddWithValue("@corps", request.Corpus);
                            cmd.Parameters.AddWithValue("@address_type", request.AddressType);
                            cmd.Parameters.AddWithValue("@flat", request.Flat);
                            cmd.Parameters.AddWithValue("@worker_id", request.Worker?.Id);
                            cmd.Parameters.AddWithValue("@sur_name", request.Worker?.SurName);
                            cmd.Parameters.AddWithValue("@first_name", request.Worker?.FirstName);
                            cmd.Parameters.AddWithValue("@patr_name", request.Worker?.PatrName);
                            cmd.Parameters.AddWithValue("@create_user_id", request.CreateUser?.Id);
                            cmd.Parameters.AddWithValue("@surname", request.CreateUser?.SurName);
                            cmd.Parameters.AddWithValue("@firstname", request.CreateUser?.FirstName);
                            cmd.Parameters.AddWithValue("@patrname", request.CreateUser?.PatrName);
                            cmd.Parameters.AddWithValue("@execute_date", request.ExecuteTime);
                            cmd.Parameters.AddWithValue("@Period_Name", request.ExecutePeriod);
                            cmd.Parameters.AddWithValue("@description", request.Description);
                            cmd.Parameters.AddWithValue("@service_name", request.Service);
                            cmd.Parameters.AddWithValue("@parent_name", request.ParentService);
                            cmd.Parameters.AddWithValue("@client_phones", request.ContactPhones);
                            cmd.Parameters.AddWithValue("@clinet_fio", request.MainFio);
                            cmd.Parameters.AddWithValue("@Rating", request.Rating);
                            cmd.Parameters.AddWithValue("@RatingDesc", request.RatingDescription);
                            cmd.Parameters.AddWithValue("@Req_Status", request.Status);
                            cmd.Parameters.AddWithValue("@to_time", request.ToTime);
                            cmd.Parameters.AddWithValue("@from_time", request.FromTime);
                            cmd.Parameters.AddWithValue("@spend_time", request.SpendTime);
                            cmd.Parameters.AddWithValue("@bad_work", request.IsBadWork);
                            cmd.Parameters.AddWithValue("@garanty", request.Garanty);
                            cmd.Parameters.AddWithValue("@retry", request.IsRetry);
                            cmd.Parameters.AddWithValue("@recordId", request.RecordUniqueId);
                            cmd.Parameters.AddWithValue("@alert_time", request.AlertTime);
                            cmd.Parameters.AddWithValue("@service_company_name", request.ServiceCompany);
                            cmd.Parameters.AddWithValue("@executer_id", request.Executer?.Id);
                            cmd.Parameters.AddWithValue("@exec_sur_name", request.Executer?.SurName);
                            cmd.Parameters.AddWithValue("@exec_first_name", request.Executer?.FirstName);
                            cmd.Parameters.AddWithValue("@exec_patr_name", request.Executer?.PatrName);
                            cmd.Parameters.AddWithValue("@address_id", request.AddressId);
                            cmd.Parameters.AddWithValue("@house_id", request.HouseId);
                            cmd.Parameters.AddWithValue("@close_date", request.CloseDate);
                            cmd.Parameters.AddWithValue("@done_date", request.DoneDate);
                            cmd.Parameters.AddWithValue("@isImmediate", request.IsImmediate);
                            cmd.Parameters.AddWithValue("@isChargeable", request.IsChargeable);
                            cmd.Parameters.AddWithValue("@createByClient", request.CreateByClient);


                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
            var spendTime = (DateTime.Now - now).TotalSeconds;
            Console.WriteLine($"SpendedTime:{0}",spendTime);
        }

        private static void ToGoogleSheets(List<RequestForListDto> requests, string spreadsheetId)
        {
            string[] Scopes = {SheetsService.Scope.Spreadsheets};
            UserCredential credential;
            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = credPath + ".credentials/sheets.googleapis.com-dotnet-quickstart.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "DispexRobo",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }


            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DispexRobo",
            });
            //String spreadsheetId = "1JFJge2LVNWhNuumIDMDFgdJE8jMB4dk2rjiXn3UMbnA";
            //var spreadsheetId = "1Vq3tnDt6QqnZWsOImit5y-0KLTH95GLhAls5k8lbbmI"; //Requests3  https://docs.google.com/spreadsheets/d/1Vq3tnDt6QqnZWsOImit5y-0KLTH95GLhAls5k8lbbmI/edit?usp=sharing
            //var spreadsheetId = "1wzjPS5mFotIH8koSyxxv_uVOezRV5VNx1AtKB032WJQ"; //RequestsSibEnergo  https://docs.google.com/spreadsheets/d/1wzjPS5mFotIH8koSyxxv_uVOezRV5VNx1AtKB032WJQ/edit?usp=sharing
            //var spreadsheetId = "1J4OL1khbYn4ywFyUBOLQnug2yTrNAoOwwMOTWwcQaHs"; //Requests2
            //var spreadsheetId = "1wpq3B9_-JGdt6ypoRC2szfV4OQxgfY-FryjVN_t8xcg"; //Requests

            //String range3 = "address!A2";
            int index = 1;
            var startRange = $"requests!A2";
            var valueRange = new ValueRange();
            var values = new List<IList<object>>();
            foreach (var request in requests)
            {
                index++;
                if (index % 5000 == 0)
                {
                    valueRange.Values = values;
                    SpreadsheetsResource.ValuesResource.UpdateRequest update = service.Spreadsheets.Values.Update(valueRange,
                        spreadsheetId, startRange);
                    update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

                    UpdateValuesResponse result2 = update.Execute();

                    startRange = $"requests!A{index}";
                    values.Clear();
                }

                var objRequest = new List<object>()
                {
                    request.Id,
                    request.ServiceCompany ?? "",
                    request.Status,
                    request.CreateTime,
                    //request.CreateTime.ToString("dd.MM.yyyy"),
                    //request.CreateTime.ToString("HH:mm:ss"),
                    request.CreateUser?.ShortName ?? "",
                    request.StreetName ?? "",
                    request.Building ?? "",
                    request.Corpus ?? "",
                    request.Flat ?? "",
                    request.ParentService ?? "",
                    request.Service ?? "",
                    request.ExecuteTime,
                    //request.ExecuteTime?.ToString("dd.MM.yyyy") ?? "",
                    request.ExecutePeriod ?? "",
                    request.Worker?.ShortName ?? "",
                    request.FromTime?.ToString("HH:mm:ss") ?? "",
                    request.ToTime?.ToString("HH:mm:ss") ?? "",
                    request.SpendTime ?? "",
                    request.Garanty ? "Да" : "Нет",
                    request.Rating ?? "",
                    request.RatingDescription ?? "",
                    request.IsRetry ? "Да" : "Нет",
                    request.Description,
                    request.Executer?.ShortName ?? ""
                };
                values.Add(objRequest);
            }
            valueRange.Values = values;
            SpreadsheetsResource.ValuesResource.UpdateRequest update2 = service.Spreadsheets.Values.Update(valueRange,
                spreadsheetId, startRange);
            update2.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            UpdateValuesResponse result = update2.Execute();

        }

        static List<RequestForListDto> GetRequests(string connectionString,string serviceCompany)
        {
            var requests = new List<RequestForListDto>();
            using (var dbConnection = new MySqlConnection(connectionString))
            {
                dbConnection.Open();
                var sqlQuery =
                    @"SELECT R.id,case when count(ra.id)=0 then false else true end has_attach,R.create_time,sp.name as prefix_name,s.name as street_name,h.building,h.corps,at.Name address_type, a.flat,
    R.worker_id, w.sur_name,w.first_name,w.patr_name, create_user_id,u.surname,u.firstname,u.patrname,
    R.execute_date,p.Name Period_Name, R.description,rt.name service_name, rt2.name parent_name, group_concat(distinct cp.Number order by rc.IsMain desc separator ', ') client_phones,
    (SELECT concat(sur_name,' ',case when first_name is null then '' else first_name end,' ', case when patr_name is null then '' else patr_name end) from CallCenter.RequestContacts rc2
    join CallCenter.ClientPhones cp2 on cp2.id = rc2.ClientPhone_id
    where rc2.request_id = R.id
    order by IsMain desc limit 1) clinet_fio,    
    rating.Name Rating, rtype.Description RatingDesc,
    RS.Description Req_Status,R.to_time, R.from_time, TIMEDIFF(R.to_time,R.from_time) spend_time,R.bad_work,R.garanty,R.retry,
    min(rcalls.uniqueID) recordId, R.alert_time, sc.Name service_company_name,
    R.executer_id,execw.sur_name exec_sur_name, execw.first_name exec_first_name, execw.patr_name exec_patr_name,a.id address_id,h.id house_id,
    R.close_date, R.done_date,  R.is_immediate, R.is_chargeable, if(R.create_client_id is not null,1,0) by_client

    FROM CallCenter.Requests R
    left join CallCenter.ServiceCompanies  sc on sc.id = R.service_company_id
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
    left join CallCenter.Workers execw on execw.id = R.executer_id
    left join CallCenter.RequestContacts rc on rc.request_id = R.id
    left join CallCenter.ClientPhones cp on cp.id = rc.clientPhone_id
    join CallCenter.Users u on u.id = create_user_id
    left join CallCenter.PeriodTimes p on p.id = R.period_time_id
    left join CallCenter.RequestRating rtype on rtype.request_id = R.id
    left join CallCenter.RatingTypes rating on rtype.rating_id = rating.id
    left join CallCenter.RequestCalls rcalls on rcalls.request_id = R.id
    where 1=1 ";// where R.create_time > 20180101";
                if (serviceCompany != null)
                {
                    if (serviceCompany == "ЭНКО")
                    {
                        sqlQuery += " and h.street_id in (67,107,109) or (h.street_id = 70 and building in (7,9)) or (h.street_id = 47 and building in (16,18))";
                    }
                    else
                    {
                        sqlQuery += " and sc.Name = @ServiceCompany";
                        
                    }
                }
                sqlQuery += " group by R.id order by R.id";
                using (var cmd = new MySqlCommand(sqlQuery, dbConnection))
                {
                    if (serviceCompany != null && serviceCompany != "ЭНКО")
                    {
                        cmd.Parameters.AddWithValue("@ServiceCompany", serviceCompany);
                    }
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var recordUniqueId = dataReader.GetNullableString("recordId");
                            requests.Add(new RequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                AddressId = dataReader.GetInt32("address_id"),
                                HouseId = dataReader.GetInt32("house_id"),
                                HasAttachment = dataReader.GetBoolean("has_attach"),
                                IsBadWork = dataReader.GetBoolean("bad_work"),
                                IsRetry = dataReader.GetBoolean("retry"),
                                IsImmediate = dataReader.GetBoolean("is_immediate"),
                                IsChargeable = dataReader.GetBoolean("is_chargeable"),
                                CreateByClient = dataReader.GetBoolean("by_client"),
                                Garanty = dataReader.GetBoolean("garanty"),
                                HasRecord = !string.IsNullOrEmpty(recordUniqueId),
                                RecordUniqueId = recordUniqueId,
                                StreetPrefix = dataReader.GetString("prefix_name"),
                                StreetName = dataReader.GetString("street_name"),
                                AddressType = dataReader.GetString("address_type"),
                                Flat = dataReader.GetString("flat"),
                                Building = dataReader.GetString("building"),
                                ServiceCompany = dataReader.GetNullableString("service_company_name"),
                                Corpus = dataReader.GetNullableString("corps"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                Worker = dataReader.GetNullableInt("worker_id") != null
                                    ? new RequestUserDto
                                    {
                                        Id = dataReader.GetInt32("worker_id"),
                                        SurName = dataReader.GetNullableString("sur_name"),
                                        FirstName = dataReader.GetNullableString("first_name"),
                                        PatrName = dataReader.GetNullableString("patr_name"),
                                    }
                                    : null,
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
                                ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                                Rating = dataReader.GetNullableString("Rating"),
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                Status = dataReader.GetNullableString("Req_Status"),
                                SpendTime = dataReader.GetNullableString("spend_time"),
                                FromTime = dataReader.GetNullableDateTime("from_time"),
                                ToTime = dataReader.GetNullableDateTime("to_time"),
                                AlertTime = dataReader.GetNullableDateTime("alert_time"),
                                CloseDate = dataReader.GetNullableDateTime("close_date"),
                                DoneDate = dataReader.GetNullableDateTime("done_date"),
                                MainFio = dataReader.GetNullableString("clinet_fio")
                            });
                        }
                        dataReader.Close();
                    }
                }
                dbConnection.Close();
            }
            return requests;
        }
    }
}
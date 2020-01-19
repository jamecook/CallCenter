using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RequestServiceImpl.Dto;
using RestSharp;

namespace ClientPhone.Services
{
    public static class RestRequestService
    {
        private static readonly string ApiKey  = "qwertyuiop987654321";
        //private static readonly string ApiUrl  = "http://127.0.0.1:5000/Client";
        private static readonly string ApiUrl  = "http://192.168.1.124:32180/Client";
        public static ActiveChannelsDto[] GetActiveChannels(int userId)
        {
            var restUrl = $"{ApiUrl}/activeCalls?userId={userId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<ActiveChannelsDto[]>(responce.Content);
        }
        public static NotAnsweredDto[] GetNotAnswered(int userId)
        {
            var restUrl = $"{ApiUrl}/getNotAnswered?userId={userId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<NotAnsweredDto[]>(responce.Content);
        }

        public static SipDto GetSipInfoByIp(string ipAddr)
        {
            var restUrl = $"{ApiUrl}/getSipInfoByIp?ipAddr={ipAddr}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<SipDto>(responce.Content);
        }

        public static UserDto[] GetDispatchers(int companyId)
        {
            var restUrl = $"{ApiUrl}/getDispatchers?companyId={companyId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<UserDto[]>(responce.Content);
        }

        public static RequestUserDto[] GetFilterDispatchers(int userId)
        {
            var restUrl = $"{ApiUrl}/getFilterDispatchers?userId={userId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<RequestUserDto[]>(responce.Content);
        }

        public static ServiceCompanyDto[] GetFilterServiceCompanies(int userId)
        {
            var restUrl = $"{ApiUrl}/getFilterCompanies?userId={userId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<ServiceCompanyDto[]>(responce.Content);
        }

        public static CallsListDto[] GetCallList(int userId, DateTime fromDate, DateTime toDate, string requestId, int? operatorId, int? serviceCompanyId, string phoneNumber)
        {
            var restUrl = $"{ApiUrl}/getCallList?userId={userId}&fromDate={fromDate.ToString("yyyy-MM-dd")}&toDate={toDate.ToString("yyyy-MM-dd")}";
            if (!string.IsNullOrEmpty(requestId))
            {
                restUrl += $"&requestId={requestId}";
            }
            if (operatorId.HasValue)
            {
                restUrl += $"&operatorId={operatorId}";
            }
            if (serviceCompanyId.HasValue)
            {
                restUrl += $"&serviceCompanyId={serviceCompanyId}";
            }
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                restUrl += $"&phoneNumber={phoneNumber}";
            }

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<CallsListDto[]>(responce.Content);
        }


        public static ServiceCompanyDto[] GetCompaniesForCall(int userId)
        {
            var restUrl = $"{ApiUrl}/getCompaniesForCall?userId={userId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<ServiceCompanyDto[]>(responce.Content);
        }
        public static TransferIntoDto[] GetTransferList(int userId)
        {
            var restUrl = $"{ApiUrl}/getTransferList?userId={userId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<TransferIntoDto[]>(responce.Content);
        }

        public static RequestForListShortDto[] GetRequestByPhone(int userId,string phoneNumber)
        {
            var restUrl = $"{ApiUrl}/getRequestByPhone?userId={userId}&phoneNumber={phoneNumber}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<RequestForListShortDto[]>(responce.Content);
        }

        public static DispatcherStatDto[] GetDispatcherStat(int userId)
        {
            var restUrl = $"{ApiUrl}/getDispatcherStat?userId={userId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<DispatcherStatDto[]>(responce.Content);
        }

        public static void SendAlive(int userId,string sipUser)
        {
            var restUrl = $"{ApiUrl}/sendAlive?userId={userId}&sipUser={sipUser}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            client.Execute(request);
        }
        public static void DeleteCallFromNotAnsweredListByTryCount(int userId,string callId)
        {
            var restUrl = $"{ApiUrl}/deleteByRingCount?userId={userId}&callId={callId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            client.Execute(request);
        }

        public static void IncreaseRingCount(int userId,string callId)
        {
            var restUrl = $"{ApiUrl}/increaseRingCount?userId={userId}&callId={callId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            client.Execute(request);
        }

        public static void AddCallToRequest(int userId, int requestId, string callId)
        {
            var restUrl = $"{ApiUrl}/attachCall?userId={userId}&requestId={requestId}&callId={callId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            client.Execute(request);
        }

        public static void Logout(int userId)
        {
            var restUrl = $"{ApiUrl}/logout?userId={userId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            client.Execute(request);
        }

        public static DateTime GetCurrentDate()
        {
            var restUrl = $"{ApiUrl}/currentDate";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<DateTime>(responce.Content);
        }

        public static UserDto Login(string login, string password, string sipUser)
        {
            var restUrl = $"{ApiUrl}/login?login={login}&password={password}&sipUSer={sipUser}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<UserDto>(responce.Content);
        }

        public static byte[] GetRecordById(int userId, string path)
        {
            var restUrl = $"{ApiUrl}/GetRecordById?userId={userId}&path={path}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<byte[]>(responce.Content);
        }

        public static string GetUniqueIdByCallId(int userId, string callId)
        {
            var restUrl = $"{ApiUrl}/getCallUniqueId?userId={userId}&callId={callId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<string>(responce.Content);

        }
    }
}

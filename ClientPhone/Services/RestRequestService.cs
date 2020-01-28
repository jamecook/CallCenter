using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RequestServiceImpl;
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

        public static MeterListDto[] GetMetersByDate(int userId, int? serviceCompanyId, DateTime fromDate, DateTime toDate)
        {
            var restUrl = $"{ApiUrl}/getMeters?userId={userId}&fromDate={fromDate.ToString("yyyy-MM-dd")}&toDate={toDate.ToString("yyyy-MM-dd")}";
            if (serviceCompanyId.HasValue)
            {
                restUrl += $"&companyId={serviceCompanyId}";
            }

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<MeterListDto[]>(responce.Content);
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
        public static RequestForListShortDto[] GetAlertRequests(int userId)
        {
            var restUrl = $"{ApiUrl}/getAlertRequests?userId={userId}";

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

        public static StreetDto[] GetStreets(int userId,int cityId,int? serviceCompanyId)
        {
            var restUrl = $"{ApiUrl}/getStreets?userId={userId}&cityId={cityId}";
            if(serviceCompanyId.HasValue)
                restUrl += $"&companyId={serviceCompanyId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<StreetDto[]>(responce.Content);
        }
        public static HouseDto[] GetHouses(int userId,int streetId,int? serviceCompanyId)
        {
            var restUrl = $"{ApiUrl}/getHouses?userId={userId}&streetId={streetId}";
            if (serviceCompanyId.HasValue)
                restUrl += $"&companyId={serviceCompanyId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<HouseDto[]>(responce.Content);
        }
        public static HouseDto GetHouseById(int userId,int houseId)
        {
            var restUrl = $"{ApiUrl}/getHouseById?userId={userId}&houseId={houseId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<HouseDto>(responce.Content);
        }
        public static string GetActiveCallUniqueIdByCallId(int userId, string callId)
        {
            var restUrl = $"{ApiUrl}/getActiveCallUniqueIdByCallId?userId={userId}&callId={callId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<string>(responce.Content);
        }
        public static StatusDto[] GetStatuses(int userId)
        {
            var restUrl = $"{ApiUrl}/getStatuses?userId={userId}";
          var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<StatusDto[]>(responce.Content);
        }
        public static AddressTypeDto[] GetAddressTypes(int userId)
        {
            var restUrl = $"{ApiUrl}/getAddressTypes?userId={userId}";
          var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<AddressTypeDto[]>(responce.Content);
        }
        public static CityDto[] GetCities(int userId)
        {
            var restUrl = $"{ApiUrl}/getCities?userId={userId}";
          var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<CityDto[]>(responce.Content);
        }
        public static WorkerDto[] GetMasters(int userId, int? serviceCompanyId, bool showOnlyExecutors)
        {
            var restUrl = $"{ApiUrl}/getMasters?userId={userId}&showOnlyExecutors={showOnlyExecutors}";
            if (serviceCompanyId.HasValue)
                restUrl += $"&companyId={serviceCompanyId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<WorkerDto[]>(responce.Content);
        }
        public static RequestInfoDto GetRequest(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/getRequest?userId={userId}&requestId={requestId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<RequestInfoDto>(responce.Content);
        }
        public static ServiceDto GetServiceById(int userId, int serviceId)
        {
            var restUrl = $"{ApiUrl}/getRequest?userId={userId}&serviceId={serviceId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<ServiceDto>(responce.Content);
        }
        public static WorkerDto[] GetExecutors(int userId, int? serviceCompanyId, bool showOnlyExecutors)
        {
            var restUrl = $"{ApiUrl}/getExecutors?userId={userId}&showOnlyExecutors={showOnlyExecutors}";
            if (serviceCompanyId.HasValue)
                restUrl += $"&companyId={serviceCompanyId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<WorkerDto[]>(responce.Content);
        }
        public static WorkerDto GetWorkerById(int userId, int workerId)
        {
            var restUrl = $"{ApiUrl}/getExecutors?userId={userId}&workerId={workerId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<WorkerDto>(responce.Content);
        }
        public static ScheduleTaskDto GetScheduleTaskByRequestId(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/getScheduleTaskByRequestId?userId={userId}&requestId={requestId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<ScheduleTaskDto>(responce.Content);
        }

        public static ServiceDto[] GetServices(int userId,int? parentId, int? houseId)
        {
            var restUrl = $"{ApiUrl}/getServices?userId={userId}";
            if (parentId.HasValue)
                restUrl += $"&parentId={parentId}";
            if (houseId.HasValue)
                restUrl += $"&houseId={houseId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<ServiceDto[]>(responce.Content);
        }
        public static FlatDto[] GetFlats(int userId, int houseId)
        {
            var restUrl = $"{ApiUrl}/getFlats?userId={userId}&houseId={houseId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<FlatDto[]>(responce.Content);
        }
        public static int? GetServiceCompanyIdByHouseId(int userId, int houseId)
        {
            var restUrl = $"{ApiUrl}/getServiceCompanyIdByHouseId?userId={userId}&houseId={houseId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<int?>(responce.Content);
        }
        public static int AlertCountByHouseId(int userId, int houseId)
        {
            var restUrl = $"{ApiUrl}/alertCountByHouseId?userId={userId}&houseId={houseId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<int>(responce.Content);
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

        public static RequestForListDto[] GetRequestList(int userId, string requestId, bool filterByCreateDate, DateTime fromDate, DateTime toDate,
            DateTime executeFromDate, DateTime executeToDate, int[] streetsId, int? houseId, int? addressId, int[] parentServicesId, int? serviceId,
            int[] statusesId, int[] mastersId, int[] executorsId, int[] serviceCompaniesId, int[] usersId, int[] ratingsId, int? payment, bool onlyBadWork,
            bool onlyRetry, string clientPhone, bool onlyGaranty, bool onlyImmediate, bool onlyByClient)
        {
            var restUrl = $"{ApiUrl}/getRequests?userId={userId}&filterByCreateDate={filterByCreateDate.ToString()}&fromDate={fromDate.ToString("yyyy-MM-dd")}&toDate={toDate.ToString("yyyy-MM-dd")}" +
                          $"&executeFromDate={fromDate.ToString("yyyy-MM-dd")}&executeToDate={toDate.ToString("yyyy-MM-dd")}&badWork={onlyBadWork.ToString()}" +
                          $"&onlyRetry={onlyRetry.ToString()}&garanty={onlyGaranty.ToString()}&immediate={onlyImmediate.ToString()}&onlyByClient={onlyByClient.ToString()}";
            if(!string.IsNullOrEmpty(clientPhone))
                restUrl += $"&clientPhone={clientPhone}";
            if(!string.IsNullOrEmpty(requestId))
                restUrl += $"&requestId={requestId}";
            if (streetsId.Length > 0)
                restUrl += $"&streets={streetsId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if(houseId.HasValue)
                restUrl += $"&houseId={houseId}";
            if (addressId.HasValue)
                restUrl += $"&addressId={addressId}";
            if (parentServicesId.Length > 0)
                restUrl += $"&parentServices={parentServicesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (serviceId.HasValue)
                restUrl += $"&serviceId={serviceId}";
            if (statusesId.Length > 0)
                restUrl += $"&statuses={statusesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (mastersId.Length > 0)
                restUrl += $"&workers={mastersId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (executorsId.Length > 0)
                restUrl += $"&executors={executorsId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (serviceCompaniesId.Length > 0)
                restUrl += $"&companies={serviceCompaniesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (usersId.Length > 0)
                restUrl += $"&users={usersId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (ratingsId.Length > 0)
                restUrl += $"&ratings={ratingsId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (payment.HasValue)
                restUrl += $"&chargeable={payment}";
            
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<RequestForListDto[]>(responce.Content);

        }
        /*
        public IActionResult GetRequests([FromQuery]int userId, [FromQuery]string requestId, [FromQuery] bool? filterByCreateDate,
            [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] streets,
            [FromQuery]int? houseId,
            [FromQuery]int? addressId,
            [FromQuery]int? serviceId,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] parentServices,
            //[ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] services,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] statuses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] workers,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] executors,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] ratings,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] companies,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] users,
            [FromQuery] bool? badWork,
            [FromQuery] bool? garanty,
            [FromQuery] bool? onlyRetry,
            [FromQuery] int? chargeable,
            [FromQuery] bool? onlyExpired,
            [FromQuery] bool? onlyByClient,
            [FromQuery] bool? immediate,
            [FromQuery] string clientPhone)
            */
    }
}

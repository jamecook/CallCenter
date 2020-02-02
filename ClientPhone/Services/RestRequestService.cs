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
        private static readonly string ApiUrl = "http://192.168.1.124:32180/Client";
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

        public static StreetDto[] GetStreets(int userId,int cityId,int? serviceCompanyId = null)
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
        public static AlertDto[] GetAlerts(int userId, DateTime fromDate, DateTime toDate, int? houseId, bool onlyActive = true)
        {
            var restUrl = $"{ApiUrl}/GetAlerts?userId={userId}&fromDate={fromDate.ToString("yyyy-MM-dd")}&toDate={toDate.ToString("yyyy-MM-dd")}&onlyActive={onlyActive}";
            if(houseId.HasValue)
                restUrl += $"&houseId={houseId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<AlertDto[]>(responce.Content);
        }
        public static HouseDto[] GetHouses(int userId,int streetId,int? serviceCompanyId = null)
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
        public static WorkerDto[] GetWorkerInfoWithParrents(int userId,int workerId)
        {
            var result = new List<WorkerDto>();
            WorkerDto worker = null;
            do
            {
                if (worker == null)
                    worker = GetWorkerById(userId, workerId);
                else if (worker.ParentWorkerId.HasValue)
                    worker = GetWorkerById(userId, worker.ParentWorkerId.Value);
                result.Add(worker);
            } while (worker.ParentWorkerId != null);
            return result.ToArray();
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
        public static ClientAddressInfoDto GetLastAddressByClientPhone(int userId, string phone)
        {
            var restUrl = $"{ApiUrl}/GetLastAddressByClientPhone?userId={userId}&phone={phone}";
          var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<ClientAddressInfoDto>(responce.Content);
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
        public static StatusDto[] GetRequestStatuses(int userId)
        {
            var restUrl = $"{ApiUrl}/GetRequestStatuses?userId={userId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<StatusDto[]>(responce.Content);
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
            var restUrl = $"{ApiUrl}/getWorkerById?userId={userId}&workerId={workerId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<WorkerDto>(responce.Content);
        }
        public static void AddCallHistory(int requestId, string callUniqueId, int userId, string callId, string methodName)
        {
            var value = new AddCallHistoryDto
            {
                UserId = userId,
                CallUniqueId = callUniqueId,
                RequestId = requestId,
                CallId = callId,
                MethodName = methodName
            };
            var restUrl = $"{ApiUrl}/RequestChangeAddress";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }

        public static void ChangeDescription(int userId, int requestId, string description)
        {
            var value = new ChangeDescriptionDto
            {
                UserId = userId,
                RequestId = requestId,
                Description = description
            };
            var restUrl = $"{ApiUrl}/ChangeDescription";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static int? SaveNewRequest(int userId, string lastCallId, int addressId, int requestTypeId, ContactDto[] contactList, string requestMessage,
            bool chargeable, bool immediate, string callUniqueId, string entrance, string floor, DateTime? alertTime, bool isRetry, bool isBedWork, int? equipmentId, int warranty)

        {
            var value = new NewRequestDto()
            {
                UserId = userId,
                LastCallId= lastCallId,
                AddressId = addressId,
                RequestMessage = requestMessage,
                RequestTypeId = requestTypeId,
                IsRetry = isRetry,
                CallUniqueId = callUniqueId,
                Warranty =  warranty,
                AlertTime = alertTime,
                Chargeable = chargeable,
                ContactList = contactList,
                Entrance = entrance,
                EquipmentId = equipmentId,
                Floor = floor,
                Immediate = immediate,
                IsBedWork = isBedWork
            };
            var restUrl = $"{ApiUrl}/SaveNewRequest";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);

            return JsonConvert.DeserializeObject<int?>(responce.Content);
        }

        public static void RequestChangeAddress(int userId, int requestId, int addressId)
        {
            var value = new ChangeAddressDto
            {
                UserId = userId,
                AddressId = addressId,
                RequestId = requestId
            };
            var restUrl = $"{ApiUrl}/RequestChangeAddress";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void SendSms(int userId, int requestId, string sender, string phone, string message, bool isClient)
        {
            var value = new SendSmsDto
            {
                UserId = userId,
                RequestId = requestId,
                Sender = sender,
                IsClient = isClient,
                Message = message,
                Phone = phone
            };
            var restUrl = $"{ApiUrl}/RequestChangeAddress";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void AddNewMaster(int userId, int requestId, int? workerId)
        {
            var value = new AddNewWorkerDto()
            {
                UserId = userId,
                RequestId = requestId,
                WorkerId = workerId
            };
            var restUrl = $"{ApiUrl}/AddNewMaster";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void AddNewExecutor(int userId, int requestId, int? workerId)
        {
            var value = new AddNewWorkerDto
            {
                UserId = userId,
                RequestId = requestId,
                WorkerId = workerId
            };
            var restUrl = $"{ApiUrl}/AddNewExecutor";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void AddNewExecuteDate(int userId, int requestId, DateTime executeDate, PeriodDto period, string note)
        {
            var value = new NewExecuteDateDto
            {
                UserId = userId,
                RequestId = requestId,
                Period = period,
                Note = note,
                ExecuteDate = executeDate
            };
            var restUrl = $"{ApiUrl}/AddNewExecuteDate";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void AddNewTermOfExecution(int userId, int requestId, DateTime termOfExecution, string note)
        {
            var value = new NewTermOfExecutionDto
            {
                UserId = userId,
                RequestId = requestId,
                Note = note,
                TermOfExecution = termOfExecution
            };
            var restUrl = $"{ApiUrl}/AddNewExecuteDate";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void EditRequest(int userId, int requestId, int requestTypeId, string requestMessage, bool immediate, bool chargeable, bool isBadWork, int garanty, bool isRetry, DateTime? alertTime, DateTime? termOfExecution)
        {
            var value = new EditRequestDto
            {
                UserId = userId,
                RequestId = requestId,
                RequestTypeId = requestTypeId,
                IsRetry = isRetry,
                RequestMessage = requestMessage,
                Chargeable = chargeable,
                TermOfExecution = termOfExecution,
                Warranty = garanty,
                Immediate = immediate,
                AlertTime = alertTime,
                IsBadWork = isBadWork
            };
            var restUrl = $"{ApiUrl}/EditRequest";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void AddScheduleTask(int userId, int workerId, int? requestId, DateTime fromDate, DateTime toDate, string eventDescription)
        {
            var value = new AddScheduleTaskDto
            {
                UserId = userId,
                RequestId = requestId,
                WorkerId = workerId,
                ToDate = toDate,
                FromDate = fromDate,
                EventDescription = eventDescription
            };
            var restUrl = $"{ApiUrl}/AddScheduleTask";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void SetRequestWorkingTimes(int userId, int requestId, DateTime fromTime, DateTime toTime)
        {
            var value = new SetRequestWorkingTimesDto
            {
                UserId = userId,
                RequestId = requestId,
                ToDate = toTime,
                FromDate = fromTime,
            };
            var restUrl = $"{ApiUrl}/SetRequestWorkingTimes";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void SetRating(int userId, int requestId, int ratingId, string description)
        {
            var value = new SetRatingDto
            {
                UserId = userId,
                RequestId = requestId,
                Description = description,
                RatingId = ratingId
            };
            var restUrl = $"{ApiUrl}/SetRating";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void AddNewState(int userId, int requestId, int stateId)
        {
            var value = new NewStateDto
            {
                UserId = userId,
                RequestId = requestId,
                StateId = stateId
            };
            var restUrl = $"{ApiUrl}/AddNewState";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }
        public static void AddNewNote(int userId, int requestId, string note)
        {
            var value = new NewNoteDto
            {
                UserId = userId,
                RequestId = requestId,
                Note = note
            };
            var restUrl = $"{ApiUrl}/AddNewNote";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            request.AddJsonBody(value);
            var responce = client.Execute(request);
        }

        public static void DeleteScheduleTask(int userId, int taskId)
        {

            var restUrl = $"{ApiUrl}/DeleteScheduleTask?userId={userId}&taskId={taskId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.DELETE) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            var responce = client.Execute(request);
        }
        public static void DeleteRequestRatingById(int userId, int itemId)
        {

            var restUrl = $"{ApiUrl}/DeleteRequestRatingById?userId={userId}&itemId={itemId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.DELETE) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            var responce = client.Execute(request);
        }
        public static void DeleteAttachment(int userId, int attachmentId)
        {

            var restUrl = $"{ApiUrl}/DeleteAttachment?userId={userId}&attachmentId={attachmentId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.DELETE) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            var responce = client.Execute(request);
        }
       public static void DeleteCallListRecord(int userId, int itemId)
        {

            var restUrl = $"{ApiUrl}/DeleteCallListRecord?userId={userId}&itemId={itemId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.DELETE) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");
            var responce = client.Execute(request);
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
       public static StatusHistoryDto[] GetStatusHistoryByRequest(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/GetStatusHistoryByRequest?userId={userId}&requestId={requestId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<StatusHistoryDto[]>(responce.Content);
        }
       public static WorkerHistoryDto[] GetExecutorHistoryByRequest(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/GetExecutorHistoryByRequest?userId={userId}&requestId={requestId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<WorkerHistoryDto[]>(responce.Content);
        }
       public static WorkerHistoryDto[] GetMasterHistoryByRequest(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/GetMasterHistoryByRequest?userId={userId}&requestId={requestId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<WorkerHistoryDto[]>(responce.Content);
        }
       public static NoteDto[] GetNotes(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/GetNotes?userId={userId}&requestId={requestId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<NoteDto[]>(responce.Content);
        }
        public static ExecuteDateHistoryDto[] GetExecuteDateHistoryByRequest(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/GetExecuteDateHistoryByRequest?userId={userId}&requestId={requestId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<ExecuteDateHistoryDto[]>(responce.Content);
        }
          public static RequestRatingListDto[] GetRequestRatings(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/GetRequestRatings?userId={userId}&requestId={requestId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<RequestRatingListDto[]>(responce.Content);
        }
          public static AttachmentDto[] GetAttachments(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/GetAttachments?userId={userId}&requestId={requestId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<AttachmentDto[]>(responce.Content);
        }
          public static RequestRatingDto[] GetRequestRating(int userId)
        {
            var restUrl = $"{ApiUrl}/GetRequestRating?userId={userId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<RequestRatingDto[]>(responce.Content);
        }
        public static SmsSettingDto GetSmsSettingsForServiceCompany(int userId, int? serviceCompanyId)
        {
            var restUrl = $"{ApiUrl}/GetSmsSettingsForServiceCompany?userId={userId}";
            if (serviceCompanyId.HasValue)
                restUrl += $"&serviceCompanyId={serviceCompanyId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<SmsSettingDto>(responce.Content);
        }
        public static ScheduleTaskDto[] GetScheduleTasks(int userId, int workerId, DateTime fromDate, DateTime toDate)
        {
            var restUrl = $"{ApiUrl}/getScheduleTasks?userId={userId}&workerId={workerId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<ScheduleTaskDto[]>(responce.Content);
        }
        public static PeriodDto[] GetPeriods(int userId)
        {
            var restUrl = $"{ApiUrl}/GetPeriods?userId={userId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<PeriodDto[]>(responce.Content);
        }
        public static EquipmentDto[] GetEquipments(int userId)
        {
            var restUrl = $"{ApiUrl}/GetPeriods?userId={userId}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<EquipmentDto[]>(responce.Content);
        }
        public static WorkerDto[] GetWorkersByHouseAndService(int userId, int houseId, int parentServiceTypeId, bool showMasters = true)
        {
            var restUrl = $"{ApiUrl}/GetWorkersByHouseAndService?userId={userId}&houseId={houseId}&parentServiceTypeId={parentServiceTypeId}&showMasters={showMasters}";
            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<WorkerDto[]>(responce.Content);
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
        public static AlertTimeDto[] GetAlertTimes(int userId, bool isImmediate)
        {
            var restUrl = $"{ApiUrl}/GetAlertTimes?userId={userId}&isImmediate={isImmediate}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<AlertTimeDto[]>(responce.Content);
        }

       public static CallsListDto[] GetCallListByRequestId(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/GetCallListByRequestId?userId={userId}&requestId={requestId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<CallsListDto[]>(responce.Content);
        }
       public static SmsListDto[] GetSmsByRequestId(int userId, int requestId)
        {
            var restUrl = $"{ApiUrl}/GetSmsByRequestId?userId={userId}&requestId={requestId}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"{ApiKey}");

            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<SmsListDto[]>(responce.Content);
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
        public static byte[] GetFile(int userId,int requestId, string fileName)
        {
            var restUrl = $"{ApiUrl}/GetFile?userId={userId}&requestId={requestId}&fileName={fileName}";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            //request.AddHeader("Content-Type", "application/json; charset=utf-8");
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
        public static string GetOnlyActiveCallUniqueIdByCallId(int userId, string callId)
        {
            var restUrl = $"{ApiUrl}/getOnlyActiveCallUniqueIdByCallId?userId={userId}&callId={callId}";

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
            if (streetsId != null && streetsId.Length > 0)
                restUrl += $"&streets={streetsId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if(houseId.HasValue)
                restUrl += $"&houseId={houseId}";
            if (addressId.HasValue)
                restUrl += $"&addressId={addressId}";
            if (streetsId != null && parentServicesId.Length > 0)
                restUrl += $"&parentServices={parentServicesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (serviceId.HasValue)
                restUrl += $"&serviceId={serviceId}";
            if (statusesId != null && statusesId.Length > 0)
                restUrl += $"&statuses={statusesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (mastersId != null && mastersId.Length > 0)
                restUrl += $"&workers={mastersId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (executorsId != null && executorsId.Length > 0)
                restUrl += $"&executors={executorsId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (serviceCompaniesId != null && serviceCompaniesId.Length > 0)
                restUrl += $"&companies={serviceCompaniesId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (usersId != null && usersId.Length > 0)
                restUrl += $"&users={usersId.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)}";
            if (ratingsId != null && ratingsId.Length > 0)
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

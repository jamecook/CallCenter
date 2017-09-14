using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using RequestServiceImpl.Dto;

namespace RequestWcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IRequestWebService
    {
        [OperationContract]
        CityDto[] GetData();

        [OperationContract]
        WebUserDto Login(string login, string password);

        [OperationContract]
        RequestForListDto[] RequestList(int workerId, DateTime fromDate, DateTime toDate, int? FirlerWorkerId, int? FilterStreetId, int? FilterHouseId, int? FilterAddressId, int? FilterStatusId, int? FilterParrentServiceId, int? FilterServiceId);

        [OperationContract]
        RequestForListDto GetRequestById(int requestId);

        [OperationContract]
        WorkerDto[] GetWorkers(int workerId);
        [OperationContract]
        StreetDto[] GetStreetListByWorker(int workerId);
        [OperationContract]
        WebStatusDto[] GetWebStatuses();
        [OperationContract]
        WebHouseDto[] GetHousesByStreetAndWorkerId(int streetId, int workerId);
        [OperationContract]
        WebFlatDto[] GetFlatByHouseId(int houseId);
        [OperationContract]
        ServiceDto[] GetServices(int? parentId);
        [OperationContract]
        byte[] GetMediaByRequestId(int requestId);

        [OperationContract]
        StatInfoDto[] GetRequestByUsersInto();

        [OperationContract]
        WebCallsDto[] GetWebCallsByRequestId(int requestId);

        [OperationContract]
        StatInfoDto[] GetRequestByWorkersInto();

        [OperationContract]
        StatusDto[] GetStatusesAllowedInWeb();

        [OperationContract]
        void ChangeState(int requestId, int stateId, int userId);

        [OperationContract]
        byte[] GetRequestActs(int workerId, DateTime fromDate, DateTime toDate, int? FirlerWorkerId, int? FilterStreetId, int? FilterHouseId, int? FilterAddressId, int? FilterStatusId, int? FilterParrentServiceId, int? FilterServiceId);
    }
}

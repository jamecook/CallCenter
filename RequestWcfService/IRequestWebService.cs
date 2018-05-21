using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
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
        DateTime GetCurrentDate();

        [OperationContract]
        WebUserDto Login(string login, string password);

        [OperationContract]
        RequestForListDto[] RequestList(int workerId, DateTime fromDate, DateTime toDate, int? firlerWorkerId, int? filterStreetId, int? filterHouseId, int? filterAddressId, int? filterStatusId, int? filterParrentServiceId, int? filterServiceId, bool badWork, bool garanty, string clientPhone, int? rating, bool filterByCreateDate);

        [OperationContract]
        byte[] ExportToExcel(int workerId, int? requestId, DateTime fromDate, DateTime toDate, int[] filterWorkerIds,int[] filterExecuterIds, int[] filterStreetIds, int[] filterHouseIds, int[] filterAddressIds,int[] filterStatusIds, int[] filterParrentServiceIds, int[] filterServiceIds, bool badWork, bool garanty,string clientPhone, int[] ratingIds, bool filterByCreateDate);

        [OperationContract]
        RequestForListDto[] RequestListArrayParams(int workerId, int? requestId, DateTime fromDate, DateTime toDate, int[] filterWorkerIds, int[] filterExecuterIds, int[] filterStreetIds, int[] filterHouseIds, int[] filterAddressIds, int[] filterStatusIds, int[] filterParrentServiceIds, int[] filterServiceIds, bool badWork, bool garanty, string clientPhone, int[] ratingIds, bool filterByCreateDate);

        [OperationContract]
        RequestForListDto GetRequestById(int requestId);
        [OperationContract]
        RequestForListDto GetRequestByWorkerAndId(int workerId, int requestId);

        [OperationContract]
        WorkerDto[] GetWorkers(int workerId);

        [OperationContract]
        WorkerDto[] GetWorkersByPeriod(bool filterByCreateDate, DateTime fromDate, DateTime toDate,
            DateTime executeFromDate, DateTime executeToDate, int workerId);

        [OperationContract]
        WorkerDto[] GetExecutersByPeriod(bool filterByCreateDate, DateTime fromDate, DateTime toDate,
            DateTime executeFromDate, DateTime executeToDate, int workerId);

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
        byte[] GetRecordById(int recordId);

        [OperationContract]
        StatInfoDto[] GetRequestByUsersInfo();

        [OperationContract]
        WebCallsDto[] GetWebCallsByRequestId(int requestId);

        [OperationContract]
        StatInfoDto[] GetRequestByWorkersInfo();

        [OperationContract]
        StatusDto[] GetStatusesAllowedInWeb();

        [OperationContract]
        byte[] DownloadFile(int requestId, string fileName);

        [OperationContract]
        AttachmentDto[] GetAttachmentList(int requestId);

        [OperationContract]
        void AddNote(int requestId, string note, int userId);

        [OperationContract]
        NoteDto[] GetNotes(int requestId);

        [OperationContract]
        void ChangeState(int requestId, int stateId, int userId);

        [OperationContract]
        void SaveRedirectPhone(string secret, string phoneNumber);

        [OperationContract]
        [WebGet]
        AppRequestDto[] GetRequestsByPhone(string phone, string code);

        [OperationContract]
        [WebGet]
        AppTypeDto[] GetTypesByPhone(string phone, string code);

        [OperationContract]
        [WebGet]
        AppAddressDto[] GetAddressesByPhone(string phone, string code);

        [OperationContract]
        void CreateRequestFromPhone(string phone, string code, int addressId, int typeId, string description);

        [OperationContract]
        string CreateRequest(int workerId, string phone, string fio, int addressId, int typeId, int? masterId,
            int? executerId, string description);

        [OperationContract]
        string GetRedirectPhone();

        [OperationContract]
        StatInfoDto[] GetWorkerStat(int currentWorkerId, DateTime fromDate, DateTime toDate);

        [OperationContract]
        byte[] GetRequestActs(int workerId, DateTime fromDate, DateTime toDate, int? FirlerWorkerId, int? FilterStreetId, int? FilterHouseId, int? FilterAddressId, int? FilterStatusId, int? FilterParrentServiceId, int? FilterServiceId);
        [OperationContract]
        FileUploadResponse UploadFile(FileUploadRequest input);
    }
    [MessageContract]
    public class FileUploadRequest
    {
        [MessageHeader(MustUnderstand = true)]
        public int UserId;
        [MessageHeader(MustUnderstand = true)]
        public int RequestId;
        [MessageHeader(MustUnderstand = true)]
        public string FileName;
        [MessageBodyMember]
        public Stream FileStream;

    }

    [MessageContract]
    public class FileUploadResponse
    {
        [MessageBodyMember]
        public string RetFileName { get; set; }

    }

}

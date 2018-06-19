using System;
using System.ServiceModel;
using RequestServiceImpl.Dto;

namespace AppWebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRequestServiceToken" in both code and config file together.
    [ServiceContract]
    public interface IRequestTokenService
    {
        [OperationContract]
        LoginDto Login(string login, string password);
        [OperationContract]
        ServiceDto[] GetServices(Guid token, int? parentId);
    }
}

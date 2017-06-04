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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace SaveWcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISaveService" in both code and config file together.
    [ServiceContract]
    public interface ISaveService
    {

        [OperationContract]
        string SaveFile(int requestId, string fileExtension, byte[] fileArray);

        [OperationContract]
        byte[] GetFile(int requestId, string fileName);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
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
        byte[] DownloadFile(int requestId, string fileName);

        [OperationContract]
        FileUploadResponse UploadFile(FileUploadRequest input);
    }

    [MessageContract]
    public class FileUploadRequest
    {
        [MessageHeader(MustUnderstand = true)]
        public long RequestId;
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

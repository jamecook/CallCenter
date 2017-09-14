using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace SaveWcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SaveService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select SaveService.svc or SaveService.svc.cs at the Solution Explorer and start debugging.
    public class SaveService : ISaveService
    {
        public byte[] DownloadFile(int requestId, string fileName)
        {
            var rootDir = ConfigurationManager.AppSettings["rootFolder"].TrimEnd('\\');
            if (string.IsNullOrEmpty(rootDir))
                throw new ConfigurationErrorsException("rootFolder is not set!");
            if (Directory.Exists($"{rootDir}\\{requestId}"))
            {
                return File.ReadAllBytes($"{rootDir}\\{requestId}\\{fileName}");
            }
            return null;
        }

        //public FileUploadResponse AddAttachment(FileUploadRequest input)
        //{
        //    var path = ConfigurationManager.AppSettings["storage_path"];
        //    if (string.IsNullOrEmpty(path))
        //        return new FileUploadResponse() { fileId = -1 };
        //    path = path.TrimEnd('\\');
        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }
        //    var extention = Path.GetExtension(input.FileName);
        //    var guidFileName = Guid.NewGuid() + extention;
        //    long newId;
        //    using (var writer = new FileStream($"{path}\\{guidFileName}", FileMode.Create))
        //    {
        //        int readCount;
        //        var buffer = new byte[8192];
        //        while ((readCount = input.FileStream.Read(buffer, 0, buffer.Length)) != 0)
        //        {
        //            writer.Write(buffer, 0, readCount);
        //        }
        //    }
        //   return new FileUploadResponse() { fileId = newId };
        //}

        public FileUploadResponse UploadFile(FileUploadRequest input)
        {
            var rootDir = ConfigurationManager.AppSettings["rootFolder"].TrimEnd('\\');
            if (string.IsNullOrEmpty(rootDir))
                throw new ConfigurationErrorsException("rootFolder is not set!");
            if (!Directory.Exists($"{rootDir}\\{input.RequestId}"))
            {
                Directory.CreateDirectory($"{rootDir}\\{input.RequestId}");
            }
            var fileExtension = Path.GetExtension(input.FileName);
            var fileName = Guid.NewGuid() + fileExtension;
            using (var writer = new FileStream($"{rootDir}\\{input.RequestId}\\{fileName}", FileMode.Create))
            {
                int readCount;
                var buffer = new byte[8192];
                while ((readCount = input.FileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    writer.Write(buffer, 0, readCount);
                }
            }
            
            return new FileUploadResponse() {RetFileName = fileName};
            }
    }
}

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
        public string SaveFile(int requestId, string fileExtension, byte[] fileArray)
        {
            var rootDir = ConfigurationManager.AppSettings["rootFolder"].TrimEnd('\\');
            if(string.IsNullOrEmpty(rootDir))
                throw new ConfigurationErrorsException("rootFolder is not set!");
            if (!Directory.Exists($"{rootDir}\\{requestId}"))
            {
                Directory.CreateDirectory($"{rootDir}\\{requestId}");
            }
            var fileName = Guid.NewGuid() + "." + fileExtension;
            var fs = new FileStream($"{rootDir}\\{requestId}\\{fileName}", FileMode.CreateNew);
            fs.Write(fileArray, 0, fileArray.Length);
            fs.Close();
            return fileName;
        }

        public byte[] GetFile(int requestId, string fileName)
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
    }
}

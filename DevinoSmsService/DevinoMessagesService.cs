using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DevinoSmsService.Devino;

namespace DevinoSmsService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DevinoMessagesService" in both code and config file together.
    public class DevinoMessagesService : IDevinoMessagesService
    {
        public string GetData(int value)
        {
            //return string.Format("You entered: {0}", value);
            var sender = new Devino.SmsServiceSoapClient();
            var sessionID = sender.GetSessionID("cmscms", "p57w(.O|8P");
            var t = sender.GetBalance(sessionID);
            //var eee = sender.GetStatistics(sessionID, DateTime.Today, DateTime.Now);
            var ttt = sender.SendMessageByTimeZone(sessionID, "CMS24", "79323232177", "Проверка связи! Видешь какое у нас имя интересное", DateTime.Now.AddMinutes(2), 240);
            var e = 659352979031195648;
            var ееее = sender.GetMessageState(sessionID, e);
            return sessionID;

        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
    }
}

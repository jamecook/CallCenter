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
            var phones = new ArrayOfString();
            phones.Add("79323232177");
            var sessionID = sender.GetSessionID("ccms24", "y7q@uRx^to");
            var mess = new Devino.Message
            {
                DelayUntilUtc = DateTime.Now.AddMinutes(1),
                Data = "Test",
                DestinationAddresses = phones,
                SourceAddress = "DTSMS",
                ReceiptRequested = false
            };
            var t = sender.GetBalance(sessionID);
            sender.SendMessage(sessionID, mess);
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

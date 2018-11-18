using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RudiGrobler.Calendar.Common
{
    public class Appointment : BindableObject
    {
        private string subject;
        public string Subject 
        {
            get { return subject; }
            set
            {
                if (subject != value)
                {
                    subject = value;
                    RaisePropertyChanged("Subject");
                }
            }
        }

        private string location;
        public string Location
        {
            get { return location; }
            set
            {
                if (location != value)
                {
                    location = value;
                    RaisePropertyChanged("Location");
                }
            }
        }
        public int Id { get; set; }
        public int? RequestId { get; set; }
        public string WorkerInfo { get; set; }
        private DateTime startTime;
        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                if (startTime != value)
                {
                    startTime = value;
                    RaisePropertyChanged("StartTime");
                }
            }
        }

        private DateTime endTime;
        public DateTime EndTime
        {
            get { return endTime; }
            set
            {
                if (endTime != value)
                {
                    endTime = value;
                    RaisePropertyChanged("EndTime");
                }
            }
        }

        private string body;
        public string Body
        {
            get { return body; }
            set
            {
                if (body != value)
                {
                    body = value;
                    RaisePropertyChanged("Body");
                }
            }
        }
        
        public override string ToString()
        {
            return string.Format("{0}-{1}",StartTime,EndTime);
        }
    }
}

namespace AppNotificationForWeb
{
    public class NotificationDto
    {
        public int Id;
        public string WorkerGuid;
        public int? RequestId;
        public string Message;
    }
    public class Content
    {
        public string en;
    }
    public class Data
    {
        public string requestId;
    }
    public class Filter
    {
        public string field;
        public string key;
        public string relation;
        public string value;
    }

    public class MessageDto
    {
        public string app_id;
        public Content contents;
        public Data data;
        public Filter[] filters;
    }
}
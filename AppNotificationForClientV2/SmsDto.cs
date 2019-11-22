namespace AppNotificationForClient
{
    public class NotificationDto
    {
        public int Id;
        public string WorkerGuid;
        public int? RequestId;
        public int MessageType;
        public string Message;
        public string Info;
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
    public class NewData
    {
        public string text;
    }

    public class NewMessageDto
    {
        public string pushId;
        public string mode;
        public int type;
        public int Id;
        public NewData data;
    }

    public class MessageDto
    {
        public string app_id;
        public Content contents;
        public Data data;
        public Filter[] filters;
    }
}
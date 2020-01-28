namespace ClientPhone.Services
{
    public class ChangeAddressDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public int AddressId { get; set; }
    }
    public class AddCallHistoryDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public string CallUniqueId { get; set; }
        public string CallId { get; set; }
        public string MethodName { get; set; }
    }
    public class AddCallToRequestDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public string CallUniqueId { get; set; }
    }
    public class ChangeDescriptionDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public string Description { get; set; }
    }

}
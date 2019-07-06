namespace WebApi.Models
{
    public class WebUserDto
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public int WorkerId { get; set; }
        public int ServiceCompanyId { get; set; }
        public int SpecialityId { get; set; }
        public bool CanCreateRequestInWeb { get; set; }
        public bool AllowStatistics { get; set; }
        public bool AllowCalendar { get; set; }
        public bool OnlyImmediate { get; set; }
        public bool CanSetRating { get; set; }
        public bool CanCloseRequest { get; set; }
        public bool CanChangeExecutors { get; set; }
        public bool CanChangeStatus { get; set; }
        public bool CanChangeImmediate { get; set; }
        public bool CanChangeChargeable { get; set; }
        public bool CanChangeAddress { get; set; }
        public bool CanChangeServiceType { get; set; }
        public bool CanChangeExecuteDate { get; set; }
        public bool ServiceCompanyFilter { get; set; }
        public bool EnableAdminPage { get; set; }
        public string PushId { get; set; }
        public string FullName => SurName + " " + FirstName + " " + PatrName;
    }
}
namespace RequestServiceImpl.Dto
{
    public class WorkerDto
    {
        public int Id { get; set; }
        public int? ServiceCompanyId { get; set; }
        public string ServiceCompanyName { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public int? SpecialityId { get; set; }
        public string SpecialityName { get; set; }
        public string FullName => SurName + (string.IsNullOrEmpty(FirstName)? "" : " " + FirstName) + (string.IsNullOrEmpty(PatrName) ? "" : " " + PatrName);
        public string Phone { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool CanAssign { get; set; }
        public bool IsMaster { get; set; }
        public bool Enabled { get; set; }
        public string IsBlockedText => Enabled ? "" : "блокир.";
        public bool IsExecuter { get; set; }
        public bool IsDispetcher { get; set; }
        public bool SendSms { get; set; }
        public bool AppNotification { get; set; }
        public int? ParentWorkerId { get; set; }
        public bool CanCreateRequest { get; set; }
        public bool ShowAllRequest { get; set; }
        public bool CanCloseRequest { get; set; }
        public bool CanSetRating { get; set; }
        public bool CanShowStatistic { get; set; }
        public bool CanChangeExecutor { get; set; }
        public bool ShowOnlyGaranty { get; set; }
        public bool FilterByHouses { get; set; }
        public bool ShowOnlyMy { get; set; }
    }
}
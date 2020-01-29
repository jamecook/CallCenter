using System;
using RequestServiceImpl.Dto;

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
    public class SendSmsDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public string Sender { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public bool IsClient { get; set; }
    }
    public class AddNewMasterDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public int? MasterId { get; set; }
    }
    public class NewExecuteDateDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public DateTime ExecuteDate { get; set; }
        public PeriodDto Period { get; set; }
        public string Note { get; set; }
    }
    public class NewTermOfExecutionDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public DateTime TermOfExecution { get; set; }
        public string Note { get; set; }
    }
    public class AddScheduleTaskDto
    {
        public int UserId { get; set; }
        public int? RequestId { get; set; }
        public int WorkerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string EventDescription { get; set; }
    }
    public class SetRequestWorkingTimesDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class EditRequestDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public int RequestTypeId { get; set; }
        public string RequestMessage { get; set; }
        public bool Immediate { get; set; }
        public bool Chargeable { get; set; }
        public bool IsBadWork { get; set; }
        public int Warranty { get; set; }
        public bool IsRetry { get; set; }
        public DateTime? AlertTime { get; set; }
        public DateTime? TermOfExecution { get; set; }
    }
    public class SetRatingDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public int RatingId { get; set; }
        public string Description { get; set; }
    }
}
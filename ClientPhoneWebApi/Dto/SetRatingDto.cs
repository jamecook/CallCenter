namespace ClientPhoneWebApi.Dto
{
    public class SetRatingDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public int RatingId { get; set; }
        public string Description { get; set; }
    }
}
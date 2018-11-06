using System;

namespace RequestServiceImpl.Dto
{
    public class RequestRatingListDto
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string Rating { get; set; }
        public string Description { get; set; }
        public RequestUserDto CreateUser { get; set; }
    }
}
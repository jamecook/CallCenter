namespace CRMPhone.Dto
{
    public class RequestTypeDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public string Name { get; set; }
    }
}
namespace ClientPhoneWebApi.Dto
{
    public class RingUpConfigDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string FullName => $"{Name} ({Phone})";
    }
}
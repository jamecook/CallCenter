namespace WebApi.Models
{
    public class StreetDto
    {
        public int Id { get; set; }
        public StreetPrefixDto Prefix { get; set; }
        public string Name { get; set; }
        public int CityId { get; set; }
        public string NameWithPrefix => $"{Name}, {Prefix.ShortName}";
    }
}
namespace WebApi.Models
{
    public class ShortAddressDto
    {
        public int Id { get; set; }
        public int StreetId { get; set; }
        public int HouseId { get; set; }
        public string StreetPrefix { get; set; }
        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string Flat { get; set; }
        public string FullAddress
        {
            get
            {
                return string.IsNullOrEmpty(Corpus) ? $"{StreetName}, {Building}, {Flat}"
                    : $"{StreetName}, {Building} �.{Corpus}, {Flat}";
            }
        }

    }
}
namespace CRMPhone.Dto
{
    public class AddressDto
    {
        public int Id { get; set; }
        public ServiceCompanyDto ServiceCompany { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; }
        public int HouseId { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string Flat { get; set; }
        public int StreetId { get; set; }
        public string StreetName { get; set; }
        public int StreetPrefixId { get; set; }
        public string StreetPrefix { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
    }
}
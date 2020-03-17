namespace RequestServiceImpl.Dto
{
    public class AdditionInfoDto
    {
        public int Id { get; set; }
        public InfoType Type { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string StreetName { get; set; }
        public string ParentType { get; set; }
        public string ServiceType { get; set; }
        public int InfoId { get; set; }
        public int CompanyId { get; set; }
        public int? StreetId { get; set; }
        public int? HouseId { get; set; }
        public int ParentId { get; set; }
        public int TypeId { get; set; }
        public string HouseName
        {
            get
            {
                if (string.IsNullOrEmpty(Corpus))
                    return Building;
                return Building + "ê." + Corpus;
            }
        }
    }
}
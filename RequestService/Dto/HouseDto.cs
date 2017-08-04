namespace RequestServiceImpl.Dto
{
    public class HouseDto
    {
        public int Id { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public int StreetId { get; set; }
        public int? ServiceCompanyId { get; set; }
        public string ServiceCompanyName { get; set; }
        public int? EntranceCount { get; set; }
        public int? FlatCount { get; set; }
        public int? FloorCount { get; set; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Corpus))
                    return Building;
                return Building + "/" + Corpus;
            }
        }
    }
    }
namespace ClientPhoneWebApi.Dto
{
    public class SaveMeterCodesDto
    {
        public int UserId { get; set; }
        public int SelectedFlatId { get; set; }
        public string PersonalAccount { get; set; }
        public string Electro1Code { get; set; }
        public string Electro2Code { get; set; }
        public string HotWater1Code { get; set; }
        public string ColdWater1Code { get; set; }
        public string HotWater2Code { get; set; }
        public string ColdWater2Code { get; set; }
        public string HotWater3Code { get; set; }
        public string ColdWater3Code { get; set; }
        public string HeatingCode { get; set; }
        public string Heating2Code { get; set; }
        public string Heating3Code { get; set; }
        public string Heating4Code { get; set; }
    }
}
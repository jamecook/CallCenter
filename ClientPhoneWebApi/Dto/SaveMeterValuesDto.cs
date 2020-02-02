namespace ClientPhoneWebApi.Dto
{
    public class SaveMeterValuesDto
    {
        public int UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string PersonalAccount { get; set; }
        public int? MeterId { get; set; }
        public int AddressId { get; set; }
        public double Electro1 { get; set; }
        public double Electro2 { get; set; }
        public double HotWater1 { get; set; }
        public double ColdWater1 { get; set; }
        public double HotWater2 { get; set; }
        public double ColdWater2 { get; set; }
        public double HotWater3 { get; set; }
        public double ColdWater3 { get; set; }
        public double Heating { get; set; }
        public double Heating2 { get; set; }
        public double Heating3 { get; set; }
        public double Heating4 { get; set; }
    }
}
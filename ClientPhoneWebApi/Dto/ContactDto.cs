namespace ClientPhoneWebApi.Dto
{
    public class ContactDto 
    {
        public int Id { get; set; }

        public bool IsMain { get; set; }

        public bool IsOwner { get; set; }

        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string AdditionInfo { get; set; }
        public bool CanEditPhone { get { return Id == 0; } }
        public bool ReadOnlyPhone => !CanEditPhone;


    }
}
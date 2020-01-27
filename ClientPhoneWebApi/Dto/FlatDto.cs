namespace ClientPhoneWebApi.Dto
{
    public class FlatDto
    {
        public int Id { get; set; }
        public string Flat { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }

        public string Name
        {
            get
            {
                //if (TypeId == 1 || TypeId == 2)
                //    return Flat;
                //if (TypeId == 2)
                //    return TypeName + " ¹" + Flat;
                return Flat;
            }
        }
    }
}
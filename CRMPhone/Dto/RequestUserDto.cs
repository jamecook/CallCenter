using System.Windows.Markup;

namespace CRMPhone.Dto
{
    public class RequestUserDto
    {
        public int Id { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public string FullName => string.Format($"{SurName} {FirstName} {PatrName}");
        public string ShortName {
            get
            {
                var firstShortName = string.IsNullOrEmpty(FirstName) ? "" : FirstName.Substring(0, 1)+".";
                var partShortName = string.IsNullOrEmpty(PatrName) ? "" : PatrName.Substring(0, 1)+".";
                return string.Format($"{SurName} {firstShortName} {partShortName}").TrimEnd();
            }
        }
}
}
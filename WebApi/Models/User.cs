namespace WebApi.Models
{
    public class User
    {
        public virtual long Id { get; set; }
        public virtual string Login { get; set; }
        public virtual string PasswordHash { get; set; }
    }
}
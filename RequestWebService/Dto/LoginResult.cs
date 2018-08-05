namespace RequestWebService.Dto
{
    public class LoginResult
    {
        public int ResultCode { get; set; }
        public string ResultDescription { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
    }
}
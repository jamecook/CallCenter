using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace TrizTest
{
    /*
     * Тема: Логин в papi.itpc.ru
sibiteks : LT5vFxKSyh4UBy1BbaXl
     */
    public class Login
    {
        // ReSharper disable once InconsistentNaming
        public string username { get; set; }
        // ReSharper disable once InconsistentNaming
        public string password { get; set; }
    }
   public class LoginResponse
    {
        // ReSharper disable once InconsistentNaming
        public string token { get; set; }
    }

    class Program
    {
        private static readonly string ApiUrl = "https://papi.itpc.ru/v1";
        static void Main(string[] args)
        {
            var time = DateTime.UtcNow.ToString("yyyyMMddHH");
            var secret = "LT5vFxKSyh4UBy1BbaXl" + time;
            var hash = Hash(secret);
            var token = GetToken("sibiteks", hash);
            var result = GetReader(token.token, "11827699");
            var tt =
                "\u0425\u043e\u043b\u043e\u0434\u043d\u043e\u0435 \u0432\u043e\u0434\u043e\u0441\u043d\u0430\u0431\u0436\u0435\u043d\u0438\u0435";
        }

        static LoginResponse GetToken(string username, string secret)
        {
            var restUrl = $"{ApiUrl}/token/";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            var login = new Login
            {
                username = username,
                password = secret
            };
            request.AddBody(login);
            var responce = client.Execute(request);
            return JsonConvert.DeserializeObject<LoginResponse>(responce.Content);
        }

        static string GetReader(string token, string contract)
        {
            var restUrl = $"{ApiUrl}/counter/reading/{contract}/";

            var client = new RestClient(restUrl);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            //request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Authorization", $"Bearer {token}");

            var responce = client.Execute(request);
            return responce.Content;
        }

        static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}

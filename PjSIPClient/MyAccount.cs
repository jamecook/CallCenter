using System;
using Newtonsoft.Json;
using PJSip.Interop;

namespace PjSIPClient
{
    public class MyAccount : Account
    {
        public override void onRegState(OnRegStateParam prm)
        {
            AccountInfo ai = getInfo();
            var tt = prm;
            Console.WriteLine("prm:");
            Console.WriteLine(JsonConvert.SerializeObject(prm));
            Console.WriteLine("getInfo:");
            Console.WriteLine(JsonConvert.SerializeObject(ai));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BridgeAMI
{
    class Program
    {
        private static string _amiUserName = "zerg";
        private static string _amiSecret = "asteriskzerg";

        static void Main(string[] args)
        {
            StartClient();
        }

        public static void StartClient()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                var addresses = Dns.GetHostAddresses("192.168.1.130");
                var ipAddress = addresses[0];
                var remoteEP = new IPEndPoint(ipAddress, 5038);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(remoteEP);
//Считываем все данные из буфера
                var bytesRec = sender.Receive(bytes);
                Console.WriteLine("{0}",Encoding.ASCII.GetString(bytes, 0, bytesRec));

                // Encode the data string into a byte array.  
                byte[] loginMsg = Encoding.ASCII.GetBytes($@"Action: login
Username: {_amiUserName}
Secret: {_amiSecret}

");
                var bytesSent = sender.Send(loginMsg);
                // Receive the response from the remote device.  
                bytesRec = sender.Receive(bytes);
                var result = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine($"{GetResult(result)}");

                byte[] logoffMsg = Encoding.ASCII.GetBytes(@"Action: logoff

");
                bytesSent = sender.Send(logoffMsg);
                // Receive the response from the remote device.  
                bytesRec = sender.Receive(bytes);
                result = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine($"{GetResult(result)}");

                // Release the socket.  
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Console.ReadKey();
            }
        }

        public static string GetResult(string message)
        {
            var lines = message.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Response:"))
                {
                    var items = line.Split(':');
                    return items[1]?.Trim();
                }
            }
            return String.Empty;
        }
    }
}

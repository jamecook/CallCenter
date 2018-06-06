using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CRMPhone
{
    public class AmiService :IDisposable

    {
        private Socket _clientSocket;
        private string _ipAddress;
        private int _port;

        public AmiService(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }
        public void Connect()
        {
            var addresses = Dns.GetHostAddresses(_ipAddress);
            var address = addresses[0];
            var remoteEndPoint = new IPEndPoint(address, _port);

            _clientSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket.Connect(remoteEndPoint);
        }

        public void Disconnect()
        {
            _clientSocket.Close();
        }

        public string ReadAllFromSocket()
        {
            byte[] bytes = new byte[16*1024];
            var bytesRec = _clientSocket.Receive(bytes);
            return Encoding.ASCII.GetString(bytes, 0, bytesRec);
        }

        public string Login(string login, string secret)
        {
            byte[] bytes = new byte[1024];
            byte[] loginMsg = Encoding.ASCII.GetBytes($@"Action: login
Username: {login}
Secret: {secret}

");
            var bytesSent = _clientSocket.Send(loginMsg);
            // Receive the response from the remote device.  
            var bytesRec = _clientSocket.Receive(bytes);
            var result = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            return GetResult(result);
        }

        public string Logout()
        {
            byte[] bytes = new byte[1024];
            byte[] logoffMsg = Encoding.ASCII.GetBytes(@"Action: logoff

");
            var bytesSent = _clientSocket.Send(logoffMsg);
            // Receive the response from the remote device.  
            var bytesRec = _clientSocket.Receive(bytes);
            var result = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            return GetResult(result);
        }

        //Action: Bridge
        //Channel1: sip/trunk-500415-000038b8
        //Channel2: sip/115-000038b5
        public string Bridge(string channel1,string channel2) 
        {
            byte[] bytes = new byte[16*1024];
            byte[] msg = Encoding.ASCII.GetBytes($@"Action: Bridge
Channel1: {channel1}
Channel2: {channel2}

");
            var bytesSent = _clientSocket.Send(msg);
            // Receive the response from the remote device.  
            var bytesRec = _clientSocket.Receive(bytes);
            var result = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            return GetResult(result);
        }

        public string QueuePause(string sipNumber, bool paused)
        {
            byte[] bytes = new byte[16 * 1024];
            var pauseStr = paused ? "true" : "false";
            byte[] msg = Encoding.ASCII.GetBytes($@"Action: QueuePause
Interface: sip/{sipNumber}
Paused: {pauseStr}

");
            var bytesSent = _clientSocket.Send(msg);
            // Receive the response from the remote device.  
            var bytesRec = _clientSocket.Receive(bytes);
            var result = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            return GetResult(result);
        }

        public bool LoginAndQueuePause(string login, string secret, string sipNumber, bool paused)
        {
            var ret = false;
            Connect();
            var buff = ReadAllFromSocket();
            var result = Login(login, secret);
            if (result != "Success")
                return false;
            result = QueuePause(sipNumber, paused);
            if (string.IsNullOrEmpty(result))
            {
                result = ReadAllFromSocket();
            }
            if (result.Contains("Success"))
                ret = true;
            result = Logout();
            return ret;
        }
        public bool LoginAndBridge(string login, string secret, string channel1, string channel2)
        {
            Connect();
            var buff = ReadAllFromSocket();
            var result = Login(login, secret);
            if (result != "Success")
                return false;
            result = Bridge(channel1, channel2);
            result = Logout();
            return true;
        }

        private string GetResult(string message)
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

        public void Dispose()
        {
            Disconnect();
        }
    }
}
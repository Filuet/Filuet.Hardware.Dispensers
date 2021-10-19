using Filuet.Hardware.Dispensers.Common.Interfaces;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Filuet.Hardware.Dispensers.Common
{
    public class TcpChannel : ICommunicationChannel
    {
        private string _ip;
        private int _port;

        public TcpChannel(string ip, int port = 5000)
        {
            _ip = ip;
            _port = port;
        }

        public byte[] SendCommand(byte[] data)
        {
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse(_ip), _port));

            NetworkStream stream = client.GetStream();
            
            stream.Write(data, 0, data.Length);
            data = new byte[32];
            int bytes = stream.Read(data, 0, data.Length);

            stream.Close();
            client.Close();
            return data.Take(bytes).ToArray();
        }
    }
}
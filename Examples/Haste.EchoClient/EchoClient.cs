using System;
using System.Net;
using System.Threading;
using Haste.Data;
using Haste.Messages;

namespace Haste.EchoClient
{
    class EchoClient
    {
        private const int MESSAGE_CODE = 0;
        private const int MESSAGE_PARAM_CODE = 0;

        private NetworkConnection _connection;
        private ConnectionConfig _config;

        public EchoClient(ConnectionConfig config)
        {
            _connection = new NetworkConnection();
            _config = config;
        }

        public void Start()
        {
            Logger.Current = LogLevel.Developer;
            _connection.LogMessageReceived += (level, s) =>
            {
                Console.WriteLine("[{0}] {1}", level, s);
            };

            _connection.Configure(_config);
            _connection.ResponseReceived += OnResponseReceived;
            _connection.StatusChanged += OnStatusChanged;


            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5056);
            _connection.Connect(remoteEndPoint, new Version(0, 1, 0), null);

            Thread receiveThread = new Thread(Receive);
            receiveThread.Start();
        }

        public void Send(string input)
        {
            DataObject data = new DataObject();
            data.SetString(MESSAGE_PARAM_CODE, input);
            _connection.SendRequestMessage(MESSAGE_CODE, data, SendOptions.ReliableSend);
        }
        
        private void Receive()
        {
            while (true)
            {
                _connection.NetworkUpdate();
            }
        }

        private void OnStatusChanged(StatusCode statusCode, string s)
        {
            Console.WriteLine("[OnStatusChanged] {0}, {1}", statusCode, s);
        }
        
        private void OnResponseReceived(ResponseMessage response)
        {
            if (response.Code == MESSAGE_CODE)
            {
                string message = string.Empty;
                if (response.Data.GetValue(MESSAGE_PARAM_CODE, out message))
                {
                    Console.WriteLine("[OnResponseReceived] Server message is \"{0}\"", message);
                }
            }
        }
    }
}

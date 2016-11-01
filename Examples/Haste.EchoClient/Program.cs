using System;

namespace Haste.EchoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            EchoClient client = new EchoClient(new ConnectionConfig
            {
                ChannelCount = 5,
                DisconnectionTimeout = 3000,
                IsCrcEnabled = true,
                MaxUnreliableCommands = 0,
                MtuSize = 1350,
                PingInterval = 1000,
                SentCountAllowance = 3,
            });
            client.Start();

            while (true)
            {
                string input = Console.ReadLine();
                if (input.ToLower() == "exit" || input.ToLower() == "quit")
                    break;

                client.Send(input);
            }
        }

    }
}

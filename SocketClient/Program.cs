using System;
using System.Threading;

namespace SocketClient
{
    public class Program
    {
        static void Main()
        {
            Client client = new Client("192.168.1.36", 3000);
            client.Connect();
            
            Console.WriteLine("");
            Console.WriteLine("## HELP");
            Console.WriteLine(" - time: get the time");
            Console.WriteLine(" - exit: properly disconnect");
            Console.WriteLine(" - ping: simple ping pong");
            
            while (client.IsConnected())
            {
                string msg = Console.ReadLine();
                if (!string.IsNullOrEmpty(msg))
                {
                    client.SendMessage(msg);
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
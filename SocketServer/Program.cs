using System;

namespace SocketServer
{
    class Program
    {
        static void Main()
        {
            Server server = new Server("192.168.1.36", 3000);
            server.Start();
            
            // Example :
            
            Console.WriteLine("");
            Console.WriteLine("## HELP");
            Console.WriteLine(" - ls: list clients connected");
            
            while (server.IsRunning())
            {
                string msg = Console.ReadLine();
                if (!string.IsNullOrEmpty(msg))
                {
                    switch (msg)
                    {
                        case "ls":
                            Console.WriteLine("Clients connected:");
                            server.GetClients().ForEach((s) => { Console.WriteLine(" - " + s.RemoteEndPoint); });
                            break;
                    }
                }
            }
        }
    }
}
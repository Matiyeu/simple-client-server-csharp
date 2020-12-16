using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServer
{
    public class Server
    {
        public Socket serverSocket;
        public short port = 3000;
        private IPAddress ipAddress;
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        private bool running = false;

        public Server(string ip, short port)
        {
            IPHostEntry ipHostEntry = Dns.Resolve(ip);
            ipAddress = ipHostEntry.AddressList[0];
            this.port = port;
            Console.WriteLine("IP=" + ipAddress);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            try
            {
                serverSocket.Bind(new IPEndPoint(ipAddress, port));

                // Listen 10 requests at a time
                serverSocket.Listen(10);

                serverSocket.BeginAccept(AcceptCallback, null);
                running = true;
            }
            catch (SocketException ex)
            {
                running = false;
                Console.WriteLine(ex.Message);
            }
        }

        private void Stop()
        {
            running = false;
            CloseAllSockets();
            serverSocket.Close();
        }

        public bool IsRunning()
        {
            return running;
        }

        public List<Socket> GetClients()
        {
            return clientSockets;
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            try
            {
                Socket socket = serverSocket.EndAccept(AR);
                clientSockets.Add(socket);
                socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
                Console.WriteLine("Client connected, waiting for request...");
                serverSocket.BeginAccept(AcceptCallback, null);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket) AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string textReceived = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Text: " + textReceived);

            switch (textReceived.ToLower())
            {
                case "time":
                    SendMessage(DateTime.Now.ToLongTimeString(), current);
                    break;
                case "ping":
                    SendMessage("pong", current);
                    break;
                case "exit":
                    SendMessage("Bye bye!", current);
                    current.Shutdown(SocketShutdown.Both);
                    current.Close();
                    clientSockets.Remove(current);
                    break;
                default:
                    SendMessage("Invalid request!", current);
                    break;
            }

            if (current.Connected)
                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }

        private void SendCallback(IAsyncResult AR)
        {
            Socket current = (Socket) AR.AsyncState;
            try
            {
                current.EndSend(AR);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                SendMessage("Bye, we close all connections!", socket);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }

        public void SendMessage(string text, Socket socketClient)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            socketClient.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, socketClient);
        }
    }
}

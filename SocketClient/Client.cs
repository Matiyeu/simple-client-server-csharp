using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketClient
{
    public class Client
    {
        private Socket clientSocket;
        private string ip;
        private short port;
        private readonly int maxConnectAttempts = 5;
        private byte[] buffer;

        public Client(string ip, short port)
        {
            this.ip = ip;
            this.port = port;
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool IsConnected()
        {
            return clientSocket.Connected;
        }

        public void Connect()
        {
            int attempts = 0;
            while (!clientSocket.Connected && attempts < maxConnectAttempts)
            {
                try
                {
                    Console.WriteLine($"Connection attempt {attempts}");
                    var ipAdress = IPAddress.Parse(ip);
                    clientSocket.BeginConnect(new IPEndPoint(ipAdress, port), ConnectCallback, null);
                    Console.WriteLine($"Successfully connected to {ip}:{port}");

                    // Wait 1 seconds between connection attempts
                    Thread.Sleep(1000);
                }
                catch (SocketException ex)
                {
                    attempts++;
                    Console.WriteLine(ex);
                }
            }
        }

        public void Close()
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        public void SendMessage(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, null);
        }

        private void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket.EndConnect(AR);
                buffer = new byte[clientSocket.ReceiveBufferSize];
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SendCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket.EndSend(AR);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = clientSocket.EndReceive(AR);

                if (received == 0)
                {
                    return;
                }

                byte[] recBuf = new byte[received];
                Array.Copy(buffer, recBuf, received);
                string textReceived = Encoding.ASCII.GetString(recBuf);

                switch (textReceived)
                {
                    case "bye":
                        Close();
                        break;
                    default:
                        Console.WriteLine(textReceived);
                        break;
                }

                // Start receiving data again.
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            // Avoid Pokemon exception handling in cases like these.
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
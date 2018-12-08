using System;
using HenkTcp;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace HenkChat
{
    class NameServer
    {
        const int MAXCONNECTIONS = 100;
        private HenkTcpServer _Server = new HenkTcpServer();
        public void Start(string Ip, int Port)
        {
            Console.WriteLine($"[Server {DateTime.Now.ToString("h:mm:ss")}] Starting NameServer...");
            _Server.Start(Ip,Port,MAXCONNECTIONS);
            _Server.DataReceived += DataReceived;
            if(Program.NameServerLogs)
            {
                _Server.ClientConnected += (object sender, TcpClient Client) => _Log($"{((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString()} connected");
                _Server.ClientDisconnected += (object sender, TcpClient Client) => _Log($"{((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString()} disconnected");
                _Server.DataReceived += (object sender, Message e) => _Log($"{((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Address.ToString()} sends {e.Data.Length} bytes");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[Server {DateTime.Now.ToString("h:mm:ss")}] NameServer is online");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void DataReceived(object sender, Message e)
        {
            if (Program.Servers.ContainsKey(e.MessageString)) e.Reply(BitConverter.GetBytes(Program.Servers[e.MessageString]));
            else e.Reply(new byte[] { 0 });
        }

        private void _Log(string Text)=> File.AppendAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NameServerLog.txt"), $"[{DateTime.Now.ToString("yyyy-MM h:mm:ss")}] {Text}{Environment.NewLine}");
    }
}

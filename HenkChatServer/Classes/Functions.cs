using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;

namespace HenkChat
{
    class Functions
    {
        public static void Print(string Text, HenkChatServer Server, ConsoleColor Color = ConsoleColor.Green)
        {
            Console.ForegroundColor = Color;
            Console.WriteLine($"[{Server.S_Name} {DateTime.Now.ToString("h:mm:ss")}] {Text}");
            Console.ForegroundColor = ConsoleColor.White;

            Log(Text,Server);
        }

        public static void Error(Exception Error, HenkChatServer Server, bool Critical = false)
        {           
            string ErrorMessage = $"[{Server.S_Name} {DateTime.Now.ToString("yyyy-MM h:mm:ss")}] {Error.Message}";
            Log("[Error]"+Error.Message,Server);
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLog.txt"), Environment.NewLine + ErrorMessage);

            if (Critical)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(ErrorMessage);
                Console.ForegroundColor = ConsoleColor.White;               

                Server.Server.OnError -= Server.OnError;
                try { Program.Servers.Remove(Server.S_Name); } catch { }
                try { Server.Server.Stop(); } catch { }
                try { Server.Server = null; } catch { }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ErrorMessage);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void Broadcast(Dictionary<int, User> UserList, byte[] Message, HenkChatServer Server)
        {
            foreach (var User in UserList.Values.Where(x => x.Login))
            {
                try { User.TcpClient.GetStream().Write(Message, 0, Message.Length); }
                catch { Error(new Exception("Error while sending message to tcpclient"), Server); }
            }
        }

        public static void Kick(TcpClient Client) { Client.Client.Shutdown(SocketShutdown.Both); Client = null; }

        public static void Ban(TcpClient Client, HenkChatServer Server)
        {
            string IP = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();

            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BannedIps.txt")))    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BannedIps.txt"), IP);
            else File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BannedIps.txt"), Environment.NewLine + IP);

            Server.Server.BannedIps.Add(IP);
            Kick(Client);
            Print($"Banned {IP}", Server, ConsoleColor.Blue);
        }

        public static void Log(string Text,HenkChatServer Server)=> File.AppendAllTextAsync(Path.Combine(Server.ServerFolder,"Log.txt"), $"[{DateTime.Now.ToString("yyyy-MM h:mm:ss")}] {Text}{Environment.NewLine}");
    }
}

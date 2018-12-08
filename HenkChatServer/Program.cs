using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HenkChat
{
    class Program
    {
        static string IP = "0.0.0.0";
        public const int NameServerPort = 52525;
        public static bool NameServerLogs = false;
        public static Dictionary<string, int> Servers = new Dictionary<string, int>();

        static void Main()
        {
            Console.Clear();
            Console.Title = "HenkChatServer";
            Console.ForegroundColor = ConsoleColor.White;

            LoadConfig();
         
            List<string> BannedIps = new List<string>();          
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BannedIps.txt"))) foreach (var Line in File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BannedIps.txt"))) BannedIps.Add(Line);
          
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Servers"))) { Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Servers")); }
            if (Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Servers")).Length == 0) { Console.ForegroundColor = ConsoleColor.DarkRed; Console.WriteLine("No servers detected, create a new folder in the server folder to create one."); Console.ReadKey(); return; }

            foreach (string Server in Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Servers"))) { new HenkChatServer(Server).Start(BannedIps,IP); }

            new NameServer().Start(IP, NameServerPort);

            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"[Server {DateTime.Now.ToString("h:mm:ss")}] All services are online");
            Console.ForegroundColor = ConsoleColor.White;
            Task.Delay(-1).Wait();
        }

        private static void LoadConfig()
        {
            if(!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerConfig.conf")))
            {
                File.WriteAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerConfig.conf"),new string[] {
                    "IP=0.0.0.0",
                    "NameServerLogs=false"
                });
            }
            else
            {
                string[] Lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerConfig.conf"));

                foreach(var Line in Lines)
                {
                    if (Line[0].Equals('#')) continue;
                    else if (Line.StartsWith("IP=")&&!string.IsNullOrEmpty(Line.Remove(0, 3)))
                    {
                        IP = Line.Remove(0,3);
                    }
                    else if (Line.StartsWith("NameServerLogs=") && !string.IsNullOrEmpty(Line.Remove(0, 15)))
                    {
                        if (Line.Remove(0, 15).Equals("true")) NameServerLogs = true;
                        else NameServerLogs = false;
                    }
                }
            }
        }
    }
}

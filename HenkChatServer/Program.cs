using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HenkChat
{
    class Program
    {
        static string IP = "0.0.0.0";
        public const int NameServerPort = 52525;
        public static bool NameServerLogs = false;
        public static bool NameServerIsEnabled = true;
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

            foreach (string Server in Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Servers"))) { new HenkChatServer(Server).Start(BannedIps, IP); }

            if (NameServerIsEnabled)
            {
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NameServer.txt")))
                {
                    Console.WriteLine("Adding servers to NameServer from NameServer.txt...");
                    foreach (var Line in File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NameServer.txt"))) Servers.Add(Encoding.UTF8.GetString(new Rfc2898DeriveBytes(Line.Split(':')[0].ToLower(), new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 10000).GetBytes(20)), int.Parse(Line.Split(':')[1]));
                }
                new NameServer().Start(IP, NameServerPort);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Server {DateTime.Now.ToString("h:mm:ss")}] All services are online");
            Console.ForegroundColor = ConsoleColor.White;
            Task.Delay(-1).Wait();
        }

        private static void LoadConfig()
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerConfig.conf")))
            {
                File.WriteAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerConfig.conf"), new string[] {
                    "IP=0.0.0.0",
                    "NameServer=enabled",
                    "NameServerLogs=false"
                });
            }
            else
            {
                string[] Lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerConfig.conf"));

                foreach (var Line in Lines)
                {
                    if (Line[0].Equals('#')) continue;
                    else if (Line.StartsWith("IP=") && !string.IsNullOrEmpty(Line.Remove(0, 3))) IP = Line.Remove(0, 3);
                    else if (Line.StartsWith("NameServer="))
                    {
                        if (Line.Remove(0, 11).Equals("enabled")) NameServerIsEnabled = true;
                        else NameServerIsEnabled = false;
                    }
                    else if (Line.StartsWith("NameServerLogs="))
                    {
                        if (Line.Remove(0, 15).Equals("true")) NameServerLogs = true;
                        else NameServerLogs = false;
                    }
                }
            }
        }
    }
}

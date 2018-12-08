using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HenkChat
{
    class ConfigLoader
    {
        const string DEFAULT_NAME = "HenkChatServer";
        const int DEFAULT_PORT = 52530;
        const int DEFAULT_MAXCONNECTIONS = 5000;
        const bool DEFAULT_ADVANCEDLOG = false;

        public ConfigLoader(string ServerFolder, HenkChatServer Server, out int Port, out int MaxConnections)
        {
            if (File.Exists(Path.Combine(ServerFolder, "Config.conf"))) _LoadConfig(Path.Combine(ServerFolder, "Config.conf"), Server, out Port, out MaxConnections);
            else { _CreateNewConfig(Path.Combine(ServerFolder, "Config.conf")); _LoadConfig(Path.Combine(ServerFolder, "Config.conf"), Server, out Port, out MaxConnections); }
        }

        private void _LoadConfig(string Path, HenkChatServer Server, out int Port, out int MaxConnections)
        {
            Port = 0; MaxConnections = 0;

            foreach (var Line in File.ReadAllLines(Path))
            {
                if (Line[0].Equals('#')) continue;
                else if (Line.StartsWith("Name="))
                {
                    string Name = Line.Remove(0, 5);
                    if (Name.Length < 1 || Name.Length > 50) { Functions.Error(new Exception("You entered a invalid name, the name must be between 1 and 50 characters."), Server, false); Name = DEFAULT_NAME; }

                    Server.S_Name = Name;
                    Server.S_NameBytes = Encoding.UTF8.GetBytes(Name);
                }
                else if (Line.StartsWith("Port="))
                {
                    try
                    {
                        Port = int.Parse(Line.Remove(0, 5));
                        if (Port > 65535 || Port <= 0) { Functions.Error(new Exception("You entered a invalid port."), Server, false); Port = DEFAULT_PORT; }
                        if (Port.Equals(Program.NameServerPort)) { Functions.Error(new Exception("Port already used by nameserver"), Server, false); Port = DEFAULT_PORT; }
                    }
                    catch { Functions.Error(new Exception("You entered a invalid port."), Server, false); Port = DEFAULT_PORT; }
                }
                else if (Line.StartsWith("MaxConnections="))
                {
                    try
                    {
                        MaxConnections = int.Parse(Line.Remove(0, 15));
                        if (MaxConnections < 0) MaxConnections = DEFAULT_MAXCONNECTIONS;
                    }
                    catch { Functions.Error(new Exception("You entered a invalid MaxConnections count."), Server, false); Port = DEFAULT_PORT; }
                }
                else if (Line.StartsWith("AdvancedLog="))
                {
                    if (Line.Remove(0, 12).Equals("true")) Server.AdvancedLog = true;
                    else Server.AdvancedLog = false;
                }
                else if (Line.StartsWith("Password=")) Server.S_Password = Convert.FromBase64String(Line.Remove(0, 9));
                else if (Line.StartsWith("AdminPassword=")) Server.S_AdminPassword = Line.Remove(0, 14);
                else if (Line.StartsWith("Salt=")) Server.S_Salt = Convert.FromBase64String(Line.Remove(0, 5));
            }

            if (string.IsNullOrEmpty(Server.S_Name)) Server.S_Name = DEFAULT_NAME;
            if (Port == 0) Port = DEFAULT_PORT;
            if (MaxConnections == 0) MaxConnections = DEFAULT_MAXCONNECTIONS;
            if (Server.S_Password == null) Functions.Error(new Exception("Could not find the password in the config file"), Server, true);
            if (string.IsNullOrEmpty(Server.S_AdminPassword)) Functions.Error(new Exception("Could not find the admin password in the config file"), Server, true);
            if (Server.S_Salt == null) Functions.Error(new Exception("Could not find the salt in the config file"), Server, true);
        }

        private void _CreateNewConfig(string FolderPath)
        {
            byte[] Salt = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(Salt);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No config file detected");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Enter a new password for this server:");
            byte[] Password = new Rfc2898DeriveBytes(Console.ReadLine(), Salt, 50000).GetBytes(20);

            Console.WriteLine("Enter a new admin password for this server:");
            byte[] AdminPassword = new Rfc2898DeriveBytes(Console.ReadLine(), Salt, 50000).GetBytes(20);

            int Port = 0;
            while (Port == 0)
            {
                try
                {
                    Console.WriteLine("Enter a port for this server:");
                    Port = int.Parse(Console.ReadLine());

                    if (Port > 65535 || Port <= 0) { Console.WriteLine("Invalid port, enter a new one"); Port = 0; }
                    else break;
                }
                catch { Console.WriteLine("Invalid port, enter a new one"); }
            }


            File.CreateText(FolderPath).Close();
            string[] config = new string[] {
                "#HenkChatServer config file",
                "Name="+Path.GetFileName( Path.GetDirectoryName( FolderPath ) ),
                "Port="+Port,
                "MaxConnections="+DEFAULT_MAXCONNECTIONS,
                "AdvancedLog="+(DEFAULT_ADVANCEDLOG?"true":"false"),
                "#WARNING# #DO NOT CHANGE THE FOLLOWING LINES OR THE SERVER WILL BE BROKEN#",
                "Password="+Convert.ToBase64String(Password),
                "AdminPassword="+Convert.ToBase64String(AdminPassword),
                "Salt="+Convert.ToBase64String(Salt),
            };
            File.WriteAllLines(FolderPath, config);

            Console.WriteLine();
        }
    }
}

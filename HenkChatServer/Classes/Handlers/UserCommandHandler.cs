using HenkTcp;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System;
using System.Net.Sockets;
using System.Net;

namespace HenkChat
{
    class UserCommandHandler
    {
        public static void Handle(Message e, HenkChatServer Server, Database Database)
        {
            if (e.Data[2].Equals(1))//Users
            {
                if (e.Data.Length > 3)
                {
                    byte[] UserBytes = new byte[e.Data.Length - 3];
                    Buffer.BlockCopy(e.Data, 3, UserBytes, 0, UserBytes.Length);

                    int User = BitConverter.ToInt32(UserBytes);
                    if (User > Server.UserList.Count) return;

                    e.Reply(Server.UserList.ElementAt(User).Value.Name);
                }
                else e.Reply(_Encrypt(BitConverter.GetBytes(Server.UserList.Where(x => x.Value.Name != null).Count()), e.TcpClient, Server));
            }
            else if (e.Data[2].Equals(2))//kick
            {
                if (!Server.UserList[e.TcpClient.GetHashCode()].Admin) { e.Reply(_Encrypt(Encoding.UTF8.GetBytes("You need to be admin to do this"), e.TcpClient, Server)); return; }
                byte[] UserName = new byte[e.Data.Length - 3];
                Buffer.BlockCopy(e.Data, 3, UserName, 0, UserName.Length);

                byte[] AdminName = Server.UserList.Values.First(x => x.TcpClient == e.TcpClient).Name;
                if (_BytesEquals(AdminName, UserName)) { e.Reply(_Encrypt(Encoding.UTF8.GetBytes("You could not kick yourself"), e.TcpClient, Server)); return; }

                if (Server.UserList.Values.Any(x => _BytesEquals(x.Name, UserName))) { Functions.Kick(Server.UserList.Values.First(x => _BytesEquals(x.Name, UserName)).TcpClient); e.Reply(_Encrypt(Encoding.UTF8.GetBytes("Kicked user"), e.TcpClient, Server)); }
                else e.Reply(_Encrypt(Encoding.UTF8.GetBytes("Invalid user"), e.TcpClient, Server));
            }
            else if (e.Data[2].Equals(3))//ban
            {
                if (!Server.UserList[e.TcpClient.GetHashCode()].Admin) { e.Reply(_Encrypt(Encoding.UTF8.GetBytes("You need to be admin to do this"), e.TcpClient, Server)); return; }
                byte[] UserName = new byte[e.Data.Length - 3];
                Buffer.BlockCopy(e.Data, 3, UserName, 0, UserName.Length);

                byte[] AdminName = Server.UserList.Values.First(x => x.TcpClient == e.TcpClient).Name;
                if (_BytesEquals(AdminName, UserName)) { e.Reply(_Encrypt(Encoding.UTF8.GetBytes("You could not ban yourself"), e.TcpClient, Server)); return; }

                if (Server.UserList.Values.Any(x => _BytesEquals(x.Name, UserName))) { Functions.Kick(Server.UserList.Values.First(x => _BytesEquals(x.Name, UserName)).TcpClient); Server.Server.BannedIps.Add(((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Address.ToString()); e.Reply(_Encrypt(Encoding.UTF8.GetBytes("Banned user"), e.TcpClient, Server)); }
                else e.Reply(_Encrypt(Encoding.UTF8.GetBytes("Invalid user"), e.TcpClient, Server));
            }
            else
            {
                byte[] Command = new byte[e.Data.Length - 2];
                Buffer.BlockCopy(e.Data, 2, Command, 0, Command.Length);

                e.Reply(_Encrypt(Encoding.UTF8.GetBytes(_Handle(e, Encoding.UTF8.GetString(_Decrypt(Command, e.TcpClient, Server)), Server, Database)), e.TcpClient, Server));
            }
        }

        private static string _Handle(Message e, string Command, HenkChatServer Server, Database Database)
        {
            if (Command.Equals("!help"))
            {
                if (Server.UserList[e.TcpClient.GetHashCode()].Admin) return "!users                                         displays the online users\n!admin {admin password}          become admin\n!kick {username}                         kick a user\n!ban {username}                         give a user an ipban";
                else return "!users                                         displays the online users\n!admin {admin password}          become admin";
            }
            if (Command.Equals("!admin")) return Server.UserList[e.TcpClient.GetHashCode()].Admin ? "You are an admin" : "You are not an admin";
            else if (Command.StartsWith("!admin "))
            {
                if (Server.S_AdminPassword.Equals(Command.Remove(0, 7)))
                {
                    User x = Server.UserList[e.TcpClient.GetHashCode()];
                    x.Admin = true;
                    Server.UserList[e.TcpClient.GetHashCode()] = x;
                    return "You are now admin";
                }
                else return "Invalid password";
            }
            else if (Command.Equals("!cleardata"))
            {
                if (!Server.UserList[e.TcpClient.GetHashCode()].Admin) return "You need to be admin to do this";
                Database.Clear();
                return "All messages are deleted from database";
            }
            else return "Error, invalid command.";
        }

        private static byte[] _Encrypt(byte[] Data, TcpClient Client, HenkChatServer Server) => HenkTcp.Encryption.Encrypt(Aes.Create(), Data, Server.UserList[Client.GetHashCode()].Key);

        private static byte[] _Decrypt(byte[] Data, TcpClient Client, HenkChatServer Server) => HenkTcp.Encryption.Decrypt(Aes.Create(), Data, Server.UserList[Client.GetHashCode()].Key);

        private static bool _IsLoggedIn(int ID, HenkChatServer Server) => Server.UserList[ID].Login;

        private static bool _BytesEquals(byte[] B1, byte[] B2)
        {
            if (B1 == null || B2 == null) return false;

            if (B1.Length != B2.Length) return false;
            for (int i = 0; i < B1.Length; i++) if (B1[i] != B2[i]) return false;
            return true;
        }
    }
}

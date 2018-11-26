using System.Net.Sockets;

namespace HenkChat
{
    class ClientHandler
    {
        HenkChatServer Server;
        public ClientHandler(HenkChatServer Server) => this.Server = Server;

        public void ClientConnected(object sender, TcpClient e) { try { Server.UserList.Add(e.GetHashCode(), new User() { TcpClient = e, Login = false, Admin = false }); } catch { Functions.Kick(e); } }

        public void ClientDisconnected(object sender, TcpClient e) { try { Server.UserList.Remove(e.GetHashCode()); } catch { } }
    }
}

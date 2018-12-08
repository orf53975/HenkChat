using System.Net.Sockets;

namespace HenkChat
{
    struct User
    {
        public byte[] Name;
        public TcpClient TcpClient;
        public byte[] Key;

        public bool Login;
        public bool Admin;
    }
}

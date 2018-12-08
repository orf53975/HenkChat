using System;
using HenkTcp;
using Encryption;
using System.Security.Cryptography;
using System.Text;

namespace HenkChat
{
    class Connection
    {
        const int TIMESPAN = 1;
        const string IP = "127.0.0.1";
        const int NAME_SERVER_PORT = 52525;

        public static int Connect(string ServerName, HenkTcpClient Client)
        {
            if (!Client.Connect(IP, NAME_SERVER_PORT, TimeSpan.FromSeconds(TIMESPAN))) return 0;//could not connect to name server

            var Reply = Client.WriteAndGetReply(new Rfc2898DeriveBytes(ServerName.ToLower(), new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 10000).GetBytes(20), TimeSpan.FromSeconds(TIMESPAN));
            if (Reply == null) return 0;
            else if (Reply.Data[0].Equals(0)) return 1;
            int ServerPort = BitConverter.ToInt32(Reply.Data, 0);

            return Client.Connect(IP, ServerPort, TimeSpan.FromSeconds(TIMESPAN)) ? 3 : 2;//2 = could not connect to server,3= ok
        }

        public static int Establish(ref byte[] EncryptionKeyServer, ref byte[] Salt, HenkTcpClient Client, string Password, string UserName, RSAKey RSAKey)
        {
            try
            {
                EncryptionKeyServer = Encryption.RSA.Decrypt(Client.WriteAndGetReply(CombineBytes(new byte[] { 42, 1 }, RSAKey.PublicKey), TimeSpan.FromSeconds(TIMESPAN)).Data, RSAKey.PrivateKey);
                Client.SetEncryption(Aes.Create(), EncryptionKeyServer);
                Salt = Client.WriteAndGetReply(new byte[] { 42, 2 }, TimeSpan.FromSeconds(1)).DecryptedData;

                Rfc2898DeriveBytes HashedPassword = new Rfc2898DeriveBytes(Password, Salt, 50000);
                byte ValidPassword = Client.WriteAndGetReply(CombineBytes(new byte[] { 42, 3 }, HenkTcp.Encryption.Encrypt(Aes.Create(), HashedPassword.GetBytes(20), EncryptionKeyServer)), TimeSpan.FromSeconds(TIMESPAN)).Data[0];

                if (ValidPassword.Equals(1))
                {
                    byte ValidUserName = Client.WriteAndGetReply(CombineBytes(new byte[] { 42, 4 }, AES256.encrypt(UserName, Password, Salt)), TimeSpan.FromSeconds(TIMESPAN)).Data[0];
                    if (ValidUserName.Equals(1)) return 3;//evrything ok
                    else { Client.Disconnect(); return 2; }//UserName already taken
                }
                else { Client.Disconnect(); return 1; }//Wrong password
            }
            catch { return 0; }//server did not reply on a message
        }

        public static string SendCommand(string Text, HenkTcpClient Client, byte[] EncryptionKeyServer, string Password, byte[] Salt)
        {
            if (Text.Equals("!users"))
            {
                int Online = BitConverter.ToInt32(Client.WriteAndGetReply(new byte[] { 42, 6, 1 }, TimeSpan.FromSeconds(1)).DecryptedData, 0);

                string Users = string.Empty;
                for (int x = 0; x < Online; x++)
                {
                    Users += ", " + AES256.decrypt(Client.WriteAndGetReply(CombineBytes(new byte[] { 42, 6, 1 }, BitConverter.GetBytes(x)), TimeSpan.FromSeconds(1)).Data, Password, Salt);
                }
                return $"({Online}) {Users.Remove(0, 2)}";
            }
            if (Text.StartsWith("!admin "))
            {
                Rfc2898DeriveBytes HashedAdminPassword = new Rfc2898DeriveBytes(Text.Remove(0, 7), Salt, 50000);
                return _SendCommand(CombineBytes(new byte[] { 42, 6 }, HenkTcp.Encryption.Encrypt(Aes.Create(), Encoding.UTF8.GetBytes("!admin " + Convert.ToBase64String(HashedAdminPassword.GetBytes(20))), EncryptionKeyServer)), Client);
            }
            else if (Text.StartsWith("!kick ")) return _SendCommand(CombineBytes(new byte[] { 42, 6, 2 }, AES256.encrypt(Text.Remove(0, 6), Password, Salt)), Client);
            else if (Text.StartsWith("!ban ")) return _SendCommand(CombineBytes(new byte[] { 42, 6, 3 }, AES256.encrypt(Text.Remove(0, 5), Password, Salt)), Client);
            else return _SendCommand(CombineBytes(new byte[] { 42, 6 }, HenkTcp.Encryption.Encrypt(Aes.Create(), Encoding.UTF8.GetBytes(Text), EncryptionKeyServer)), Client);
        }

        public static byte[] CombineBytes(byte[] B1, byte[] B2)
        {
            byte[] B12 = new byte[B1.Length + B2.Length];
            Buffer.BlockCopy(B1, 0, B12, 0, B1.Length);
            Buffer.BlockCopy(B2, 0, B12, B1.Length, B2.Length);
            return B12;
        }

        private static string _SendCommand(byte[] Data, HenkTcpClient Client) => Encoding.UTF8.GetString(Client.WriteAndGetReply(Data, TimeSpan.FromSeconds(1)).DecryptedData);
    }
}

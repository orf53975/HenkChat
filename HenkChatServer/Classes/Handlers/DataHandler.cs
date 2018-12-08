using HenkTcp;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System;

namespace HenkChat
{
    class DataHandler
    {        
        private HenkChatServer _Server;
        public DataHandler(HenkChatServer _Server, string ServerFolder) { this._Server = _Server; _Database.Open(ServerFolder); }
        private Database _Database = new Database();

        public void DataReceived(object sender, Message e)
        {
            if (e.Data[0].Equals(42)) _CommandHandler(e);//42 = *
            else if (_IsLoggedIn(e.TcpClient.GetHashCode()))
            {
                Functions.Broadcast(_Server.UserList, e.Data, _Server);
                _Database.Save(e.Data);
            }
            else Functions.Ban(e.TcpClient, _Server);
        }

        private void _CommandHandler(Message e)
        {
            try
            {
                if (e.Data[1].Equals(1))
                {
                    if (_Server.UserList[e.TcpClient.GetHashCode()].Key == null)
                    {
                        byte[] Key = new byte[32];
                        new RNGCryptoServiceProvider().GetBytes(Key);

                        User x = _Server.UserList[e.TcpClient.GetHashCode()];
                        x.Key = Key;
                        _Server.UserList[e.TcpClient.GetHashCode()] = x;

                        byte[] PublicKey = new byte[e.Data.Length - 2];
                        Buffer.BlockCopy(e.Data, 2, PublicKey, 0, PublicKey.Length);
                        e.Reply(Encryption.RSA.Encrypt(Key, PublicKey));
                    }
                }
                else if (e.Data[1].Equals(2)) e.Reply(_Encrypt(_Server.S_Salt, e.TcpClient));//return salt
                else if (e.Data[1].Equals(3))//CheckPassword
                {
                    byte[] _EncryptedHashFromClient = new byte[e.Data.Length - 2];
                    Buffer.BlockCopy(e.Data, 2, _EncryptedHashFromClient, 0, _EncryptedHashFromClient.Length);

                    byte[] HashFromClient = _Decrypt(_EncryptedHashFromClient, e.TcpClient);                

                    if (_BytesEquals(HashFromClient, _Server.S_Password))
                    {
                        e.Reply(new byte[] { 1 });
                        User x = _Server.UserList[e.TcpClient.GetHashCode()];
                        x.Login = true;
                        _Server.UserList[e.TcpClient.GetHashCode()] = x;
                    }
                    else { e.Reply(new byte[] { 0 }); Functions.Kick(e.TcpClient); }
                }
                else if (_IsLoggedIn(e.TcpClient.GetHashCode()))//commands for logged in connections:
                {
                    if (e.Data[1].Equals(4))//CheckUserName
                    {
                        byte[] UserName = new byte[e.Data.Length - 2];
                        Buffer.BlockCopy(e.Data, 2, UserName, 0, UserName.Length);

                        if (_Server.UserList.Values.Any(a => _BytesEquals(a.Name, UserName)))//username already used
                        {
                            e.Reply(new byte[] { 0 });
                            Functions.Kick(e.TcpClient);
                        }
                        else
                        {
                            e.Reply(new byte[] { 1 });
                            User x = _Server.UserList[e.TcpClient.GetHashCode()];
                            x.Name = UserName;
                            _Server.UserList[e.TcpClient.GetHashCode()] = x;
                        }
                    }
                    else if (e.Data[1].Equals(5))//GetMessages
                    {
                        try
                        {
                            if (e.Data.Length > 2)
                            {
                                byte[] Message = new byte[e.Data.Length - 2];
                                Buffer.BlockCopy(e.Data, 2, Message, 0, Message.Length);

                                e.Reply(_Database.Read(BitConverter.ToInt32(Message)));
                            }
                            else e.Reply(_Encrypt(BitConverter.GetBytes(_Database.GetMessagesCount()), e.TcpClient));
                        }
                        catch (Exception ex) { Functions.Error(ex, _Server); }
                    }                                                   
                    else if (e.Data[1].Equals(6)) UserCommandHandler.Handle(e,_Server,_Database);//UserCommands
                }
                else Functions.Ban(e.TcpClient, _Server);
            }
            catch (Exception ex) { Functions.Error(ex, _Server); }
        }

        private bool _BytesEquals(byte[] B1, byte[] B2)
        {
            if (B1 == null || B2 == null) return false;

            if (B1.Length != B2.Length) return false;
            for (int i = 0; i < B1.Length; i++) if (B1[i] != B2[i]) return false;
            return true;
        }

        private byte[] _Encrypt(byte[] Data, TcpClient Client) => HenkTcp.Encryption.Encrypt(Aes.Create(), Data, _Server.UserList[Client.GetHashCode()].Key);
        private byte[] _Decrypt(byte[] Data, TcpClient Client) => HenkTcp.Encryption.Decrypt(Aes.Create(), Data, _Server.UserList[Client.GetHashCode()].Key);

        private bool _IsLoggedIn(int ID) => _Server.UserList[ID].Login;
    }
}

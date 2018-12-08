using System.Text;
using System.Security.Cryptography;

namespace Encryption
{
    class RSA
    {
        public static byte[] Encrypt(string Text, byte[] PublicKey) => Encrypt(Encoding.UTF8.GetBytes(Text), PublicKey);
        public static byte[] Encrypt(byte[] Data, byte[] PublicKey)
        {
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportCspBlob(PublicKey);

            return rsaProvider.Encrypt(Data, false);
        }

        public static string Decrypt(string Text, byte[] PrivateKey)=> Decrypt(Encoding.UTF8.GetBytes(Text), PrivateKey);
        public static string Decrypt(byte[] Data, byte[] PrivateKey)
        {
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportCspBlob(PrivateKey);

            return Encoding.UTF8.GetString(rsaProvider.Decrypt(Data, false));
        }
    }

    public struct RSAKey
    {
        public string PrivateKey;
        public string PublicKey;
    }
}

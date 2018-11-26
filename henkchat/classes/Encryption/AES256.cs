using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Encryption
{
    class AES256
    {
        public static byte[] encrypt(string Text, string Password, byte[] Salt)
        {
            try
            {
                DeriveBytes rgb = new Rfc2898DeriveBytes(Password, Salt);
                AesCryptoServiceProvider algorithm = new AesCryptoServiceProvider();

                using (MemoryStream buffer = new MemoryStream())
                {
                    using (CryptoStream stream = new CryptoStream(buffer, algorithm.CreateEncryptor(rgb.GetBytes(algorithm.KeySize >> 3), rgb.GetBytes(algorithm.BlockSize >> 3)), CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
                        {
                            writer.Write(Text);
                        }
                    }

                    return buffer.ToArray();
                }
            }
            catch { return null; }
        }
        public static string decrypt(byte[] Data, string Password, byte[] Salt)
        {
            try
            {
                DeriveBytes rgb = new Rfc2898DeriveBytes(Password, Salt);
                AesCryptoServiceProvider algorithm = new AesCryptoServiceProvider();

                using (MemoryStream buffer = new MemoryStream(Data))
                {
                    using (CryptoStream stream = new CryptoStream(buffer, algorithm.CreateDecryptor(rgb.GetBytes(algorithm.KeySize >> 3), rgb.GetBytes(algorithm.BlockSize >> 3)), CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch { return null; }
        }
    }
}

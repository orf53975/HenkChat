using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Encryption
{
    class RSA
    {
        public static RSAKey GenerateKeys()
        {
            CryptographicKey key = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1).CreateKeyPair(2048);

            byte[] privateKey;
            byte[] publicKey;

            CryptographicBuffer.CopyToByteArray(key.Export(CryptographicPrivateKeyBlobType.Capi1PrivateKey), out privateKey);
            CryptographicBuffer.CopyToByteArray(key.ExportPublicKey(CryptographicPublicKeyBlobType.Capi1PublicKey), out publicKey);

            RSAKey RSAKey = new RSAKey();
            RSAKey.PrivateKey = privateKey;
            RSAKey.PublicKey = publicKey;

            return RSAKey;
        }

        public static byte[] Encrypt(string Data, byte[] PublicKey)
        {
            CryptographicKey key = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1).ImportPublicKey(PublicKey.AsBuffer(), CryptographicPublicKeyBlobType.Capi1PublicKey);

            byte[] Encrypted;
            CryptographicBuffer.CopyToByteArray(CryptographicEngine.Encrypt(key, CryptographicBuffer.ConvertStringToBinary(Data, BinaryStringEncoding.Utf8), null), out Encrypted);

            return Encrypted;
        }

        public static byte[] Decrypt(byte[] Data, byte[] PrivateKey)
        {
            CryptographicKey key = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1).ImportKeyPair(PrivateKey.AsBuffer(), CryptographicPrivateKeyBlobType.Capi1PrivateKey);

            byte[] PlainData;
            CryptographicBuffer.CopyToByteArray(CryptographicEngine.Decrypt(key, Data.AsBuffer(), null), out PlainData);

            return PlainData;
        }
    }

    public struct RSAKey
    {
        public byte[] PrivateKey;
        public byte[] PublicKey;
    }
}
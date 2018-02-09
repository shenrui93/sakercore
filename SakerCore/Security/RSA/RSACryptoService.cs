/***************************************************************************
 * 
 * 创建时间：   2017/2/28 14:25:22
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SakerCore.Security.RSA
{

    /// <summary>
    /// 
    /// </summary>
    public class RSACryptoService
    {

        private int MaxDecryptLength = 128;   //RSA最大解密长度
        private int MaxEncryptSize = 117;     //RSA最大加密长度

        RSACryptoServiceProvider _privateKeyRsaProvider;
        RSACryptoServiceProvider _publicKeyRsaProvider;

        /// <summary>
        /// 初始化RSA加密对象
        /// </summary>
        /// <param name="privateKey">密钥对的私钥</param>
        /// <param name="publicKey">密钥对的公钥</param>
        /// <param name="certBits">密钥长度目前可选 1024，2048</param>
        public RSACryptoService(string privateKey, string publicKey = null, int certBits = 1024)
        {
            MaxDecryptLength = certBits / 8;
            MaxEncryptSize = MaxDecryptLength - 11;

            if (!string.IsNullOrEmpty(privateKey))
            {
                var privateKeyBits = Convert.FromBase64String(privateKey);
                _publicKeyRsaProvider = _privateKeyRsaProvider = RSACryptoServiceProviderExtension.LoadRSAPrivateKeyPkcs1(privateKeyBits, certBits);
                return;
            }
            if (!string.IsNullOrEmpty(publicKey))
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.PersistKeyInCsp = false;
                RSACryptoServiceProviderExtension.LoadPublicKeyDER(rsa, Convert.FromBase64String(publicKey));
                _publicKeyRsaProvider = rsa;
            }
        }

        /// <summary>
        /// 使用当前的RSA对象进行解密数据
        /// </summary>
        /// <param name="cipherText">待解密的加密数据</param>
        /// <returns>返回解密的数据</returns>
        public string Decrypt(string cipherText)
        {
            return Encoding.UTF8.GetString(Decrypt(SakerCore.Serialization.Base64Serialzier.FromBase64String(cipherText)));
        }
        /// <summary>
        /// 对数据进行加密处理
        /// </summary>
        /// <param name="text">需要加密的文本数据</param>
        /// <returns>返回加密后的base64字符串数据</returns>
        public string Encrypt(string text)
        {
            return Serialization.Base64Serialzier.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(text)));
        }

        /// <summary>
        /// 对数据进行加密处理
        /// </summary>
        /// <param name="encryptBytes">需要加密的字节数据</param>
        /// <returns>返回加密后的base64字符串数据</returns>
        public byte[] Encrypt(byte[] encryptBytes)
        {
            if (_publicKeyRsaProvider == null)
            {
                throw new System.Exception("没有正确的初始化公钥信息，无法完成加密操作");
            }
            //由于RSA 1024 位 加密的特性，一次解密最大处理 128 个字节
            int length = encryptBytes.Length;
            int offset = 0;
            byte[] cache;
            //一个输出的缓存流
            MemoryStream outStream = new MemoryStream();
            byte[] buffer1 = new byte[MaxEncryptSize];
            while (offset < length)
            {
                var len = length - offset;
                len = len <= MaxEncryptSize ? len : MaxEncryptSize;
                byte[] buffer = new byte[len];
                System.Buffer.BlockCopy(encryptBytes, offset, buffer, 0, len);
                cache = _publicKeyRsaProvider.Encrypt(buffer, false);
                outStream.Write(cache, 0, cache.Length);
                offset += len;
            }
            //返回解密后的数据
            return outStream.ToArray();

        }
        /// <summary>
        /// 使用当前的RSA对象进行解密数据
        /// </summary>
        /// <param name="decryptBytes">待解密的加密数据</param>
        /// <returns>返回解密的数据</returns>
        public byte[] Decrypt(byte[] decryptBytes)
        {
            if (_privateKeyRsaProvider == null)
            {
                throw new System.Exception("没有正确的初始化私钥信息，无法完成解密操作");
            }

            int offset = 0;
            var stream = new MemoryStream();
            while (offset < decryptBytes.Length)
            {
                var len = decryptBytes.Length - offset;
                len = len <= MaxDecryptLength ? len : MaxDecryptLength;
                var tmp = new byte[len];
                System.Buffer.BlockCopy(decryptBytes, offset, tmp, 0, len);
                offset += len;
                var s = _privateKeyRsaProvider.Decrypt(tmp, false);
                stream.Write(s, 0, s.Length);
            }
            return stream.ToArray();

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public byte[] SignDataSHA1(byte[] bytes)
        {
            return _privateKeyRsaProvider.SignData(bytes, typeof(SHA1));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public bool VerifyDataSHA1(byte[] bytes, byte[] signature)
        {
            return _publicKeyRsaProvider.VerifyData(bytes, typeof(SHA1), signature);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public byte[] SignDataSHA256(byte[] bytes)
        {
            return _privateKeyRsaProvider.SignData(bytes, typeof(SHA256));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public bool VerifyDataSHA256(byte[] bytes, byte[] signature)
        {
            return _privateKeyRsaProvider.VerifyData(bytes, typeof(SHA256), signature);
        }

        private RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = System.Convert.FromBase64String(privateKey);

            var RSA = new RSACryptoServiceProvider();
            var RSAparams = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new System.Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new System.Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new System.Exception("Unexpected value read binr.ReadByte()");

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            RSA.ImportParameters(RSAparams);
            return RSA;
        }
        private int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }
        private RSACryptoServiceProvider CreateRsaProviderFromPublicKey(string publicKeyString)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] x509key;
            byte[] seq = new byte[15];
            // int x509size;

            x509key = Convert.FromBase64String(publicKeyString);
            // x509size = x509key.Length;

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            using (MemoryStream mem = new MemoryStream(x509key))
            {
                using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    seq = binr.ReadBytes(15);       //read the Sequence OID
                    if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x00)     //expect null byte next
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                        lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {   //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte();    //skip this null byte
                        modsize -= 1;   //reduce modulus buffer size by 1
                    }

                    byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                    if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                        return null;
                    int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                    byte[] exponent = binr.ReadBytes(expbytes);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                    RSAParameters RSAKeyInfo = new RSAParameters();
                    RSAKeyInfo.Modulus = modulus;
                    RSAKeyInfo.Exponent = exponent;
                    RSA.ImportParameters(RSAKeyInfo);

                    return RSA;
                }

            }
        }
        private bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }



    }
}




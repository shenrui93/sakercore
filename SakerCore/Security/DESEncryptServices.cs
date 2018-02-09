/***************************************************************************
 * 
 * 创建时间：   2017/4/6 10:11:12
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   DESEncryptServices
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SakerCore.Security
{

    #region DESEncryptServices
    /// <summary>
    /// DESEncryptServices
    /// </summary>
    public class DESEncryptServices
    {
        //默认密钥向量 
        private byte[] _key;
        private byte[] _kiv;

        TripleDES des3 = new TripleDESCryptoServiceProvider();


        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param> 
        /// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
        public string EncryptDES(string encryptString)
        {
            des3.Mode = CipherMode.CBC;
            byte[] plainText = Encoding.ASCII.GetBytes(encryptString);
            MemoryStream memStreamEncryptedData = new MemoryStream();
            CryptoStream encStream = new CryptoStream(memStreamEncryptedData, des3.CreateEncryptor(_key, _kiv), CryptoStreamMode.Write);
            try
            {
                encStream.Write(plainText, 0, plainText.Length);
                encStream.FlushFinalBlock();
                encryptString = Convert.ToBase64String(memStreamEncryptedData.ToArray());
            }
            catch //(Exception ex)
            {
                return encryptString;
            }
            finally
            {
                encStream.Close();
            }
            return encryptString;
        }
        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">输入需要解密的字符串</param>
        /// <returns>返回字符串信息</returns>
        public string DecryptDES(string decryptString)
        {
            des3.Mode = CipherMode.CBC;
            byte[] plainText = Convert.FromBase64String(decryptString);
            MemoryStream memStreamDecryptedData = new MemoryStream();
            CryptoStream decStream = new CryptoStream(memStreamDecryptedData, des3.CreateDecryptor(_key, _kiv), CryptoStreamMode.Write);
            try
            {
                decStream.Write(plainText, 0, plainText.Length);
                decStream.FlushFinalBlock();
                decryptString = Encoding.ASCII.GetString(memStreamDecryptedData.ToArray());
            }
            catch //(Exception ex)
            {
                return decryptString;
            }
            finally
            {
                decStream.Close();
            }
            return decryptString;
        }
        /// <summary>
        /// DES解密字节数组
        /// </summary>
        /// <param name="inputByteArray"></param>
        /// <returns></returns>
        public byte[] DecryptDES(byte[] inputByteArray)
        {
            des3.Mode = CipherMode.CBC;
            MemoryStream mStream = new MemoryStream();
            using (CryptoStream cStream = new CryptoStream(mStream, des3.CreateDecryptor(_key, _kiv), CryptoStreamMode.Write))
            {
                try
                {
                    cStream.Write(inputByteArray, 0, inputByteArray.Length);
                    cStream.FlushFinalBlock();
                    inputByteArray = mStream.ToArray();
                }
                catch
                {
                    inputByteArray = new byte[0];
                }
            }
            return inputByteArray;
        }
        /// <summary>
        /// DES加密字节数组
        /// </summary>
        /// <param name="inputByteArray">待加密的字节数组</param> 
        /// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
        public byte[] EncryptDES(byte[] inputByteArray)
        {
            des3.Mode = CipherMode.CBC;
            MemoryStream mStream = new MemoryStream();
            using (CryptoStream cStream = new CryptoStream(mStream, des3.CreateEncryptor(_key, _kiv), CryptoStreamMode.Write))
            {
                try
                {
                    cStream.Write(inputByteArray, 0, inputByteArray.Length);
                    cStream.FlushFinalBlock();
                    inputByteArray = mStream.ToArray();
                }
                catch
                {
                    inputByteArray = new byte[0];
                }
            }
            return inputByteArray;
        }





        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param> 
        /// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
        public string SimpleBase64EncryptDES(string encryptString)
        {
            des3.Mode = CipherMode.CBC;
            byte[] plainText = Encoding.UTF8.GetBytes(encryptString);
            return Serialization.Base64Serialzier.ToBase64String(this.EncryptDES(plainText));
        }
        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">输入需要解密的字符串</param>
        /// <returns>返回字符串信息</returns>
        public string SimpleBase64DecryptDES(string decryptString)
        {
            byte[] plainText = Serialization.Base64Serialzier.FromBase64String(decryptString);
            return Encoding.UTF8.GetString(this.DecryptDES(plainText));
        }


        /// <summary>
        /// 
        /// </summary>
        public DESEncryptServices()
        {
            DefaultInit();
        }

        private void DefaultInit()
        {
            _key = Encoding.ASCII.GetBytes("WWW_8hb_ccHFhcwl");
            _kiv = Encoding.ASCII.GetBytes("HCwl2016");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretKey">加密字符串长度固定为24个字符</param>
        public DESEncryptServices(string secretKey)
        {
            if (string.IsNullOrEmpty(secretKey))
            {
                DefaultInit();
                return;
            }
            var bytes = Encoding.UTF8.GetBytes(secretKey);
            if (bytes.Length < 24)
            {
                DefaultInit();
                return;
            }

            var key = new byte[16];
            var kiv = new byte[8];

            System.Buffer.BlockCopy(bytes, 0, key, 0, 16);
            System.Buffer.BlockCopy(bytes, 16, kiv, 0, 8);


            _key = key;
            _kiv = kiv;
        }

    }

    #endregion

}

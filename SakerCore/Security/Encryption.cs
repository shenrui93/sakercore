using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SakerCore.Security
{
    /// <summary>
    /// 为加密处理的操作提供服务模块
    /// </summary>
    public static class Encryption
    {
        /// <summary>
        /// 获取一个空的字节数组
        /// </summary>
        public static readonly byte[] EmptyBytes = new byte[0];

        //MD5加密服务对象
        private static MD5 md5Instance { get { return MD5.Create(); } }
        //sha1加密服务对象
        private static SHA1 sha1Instance { get { return SHA1.Create(); } }
        //sha1加密服务对象
        private static HMAC HMACsha1Instance { get { return HMACSHA1.Create(); } }
        /// <summary>
        /// 
        /// </summary>
        private static HMAC HMACsha512Instance { get { return new HMACSHA512(); } }
        //SHA256 加密服务对象
        private static SHA256 sha256Instance { get { return SHA256.Create(); } }
        //SHA384 加密服务对象
        private static SHA384 sha384Instance { get { return SHA384.Create(); } }
        //SHA512 加密服务对象
        private static SHA512 sha512Instance { get { return SHA512.Create(); } }
        //des加密服务对象
        private static readonly DESEncryptServices desInstance;
        //加解密数据提供的基础编码信息
        private static readonly Encoding baseEncoding = Encoding.UTF8;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static Encryption()
        {
            desInstance = new DESEncryptServices();
        }

        #region MD5加密模块 


        /// <summary>
        /// 采用MD5加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string MD5Encrypt(string context)
        {
            return MD5Encrypt(context, baseEncoding);
        }
        /// <summary>
        /// 采用MD5加密协议加密字符串
        /// </summary>
        /// <param name="context">待加密的文本</param>
        /// <param name="encoding">加密文本的编码格式</param>
        /// <returns></returns>
        public static string MD5Encrypt(string context, Encoding encoding)
        {
            //计算加密结果
            var resultData = MD5EncryptBytes(context, encoding);
            return System.BitConverter.ToString(resultData).Replace("-", "").ToLower();
        }
        /// <summary>
        /// 采用MD5加密协议加密字符串 返回Base64编码的字符串的加密结果
        /// </summary>
        /// <param name="context"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string MD5EncryptBase64(string context, Encoding encoding)
        {
            //计算加密结果
            var resultData = MD5EncryptBytes(context, encoding);
            return System.Convert.ToBase64String(resultData).ToString();

        }
        /// <summary>
        /// 采用MD5加密协议加密字符串 返回Base64编码的字符串的加密结果
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string MD5EncryptBase64(string context)
        {
            return MD5EncryptBase64(context, baseEncoding);
        }
        /// <summary>
        /// 采用MD5加密协议加密字符串 返回字节数组的加密结果
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static byte[] MD5EncryptBytes(string context)
        {
            return MD5EncryptBytes(context, baseEncoding);
        }
        /// <summary>
        /// 采用MD5加密协议加密字符串 返回字节数组的加密结果
        /// </summary>
        /// <param name="context"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] MD5EncryptBytes(string context, Encoding encoding)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            return md5Instance.ComputeHash(encoding.GetBytes(context));
        }


        #endregion

        #region SHA加密模块

        /// <summary>
        /// 采用Sha1加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha1Encrypt(string context)
        {
            return Sha1Encrypt(context, baseEncoding);
        }
        /// <summary>
        /// 采用Sha1加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha1Base64String(string context)
        {
            return Sha1Base64String(context, baseEncoding);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha1ShortBase64String(string context)
        {
            return Sha1ShortBase64String(context, baseEncoding);
        }

        /// <summary>
        /// 采用Sha1加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Sha1Encrypt(string context, Encoding encoding)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = sha1Instance.ComputeHash(encoding.GetBytes(context));
            return BitConverter.ToString(resultData).Replace("-", "").ToLower();
        }
        /// <summary>
        /// 采用Sha1加密协议加密字符串
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha1Base64String(string context, Encoding encoding)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = sha1Instance.ComputeHash(encoding.GetBytes(context));
            return ToBase64String(resultData);
        }
        public static string Sha1ShortBase64String(string context, Encoding encoding)
        {

            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = sha1Instance.ComputeHash(encoding.GetBytes(context));
            return SakerCore.Serialization.Base64Serialzier.ToBase64String(resultData);
        }



        /// <summary>
        /// 采用 SHA256 加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha256Encrypt(string context)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = sha256Instance.ComputeHash(baseEncoding.GetBytes(context));
            return BitConverter.ToString(resultData).Replace("-", "").ToLower();
        }
        /// <summary>
        /// 采用 SHA256 加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha256Base64String(string context)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = sha256Instance.ComputeHash(baseEncoding.GetBytes(context));
            return ToBase64String(resultData);
        }
        /// <summary>
        /// 采用 SHA384 加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha384Encrypt(string context)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = sha384Instance.ComputeHash(baseEncoding.GetBytes(context));
            return BitConverter.ToString(resultData).Replace("-", "").ToLower();
        }
        /// <summary>
        /// 采用 SHA384 加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha384Base64String(string context)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = sha384Instance.ComputeHash(baseEncoding.GetBytes(context));
            return ToBase64String(resultData);
        }
        /// <summary>
        /// 采用 SHA512 加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha512Encrypt(string context)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = sha512Instance.ComputeHash(baseEncoding.GetBytes(context));
            return BitConverter.ToString(resultData).Replace("-", "").ToLower();
        }
        /// <summary>
        /// 采用 SHA512 加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Sha512Base64String(string context)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = sha512Instance.ComputeHash(baseEncoding.GetBytes(context));
            return ToBase64String(resultData);
        }
        /// <summary>
        /// 表示基于哈希的消息验证代码 (HMAC) 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string HMACSha1Encrypt(string context, byte[] key)
        { 
            return BitConverter.ToString(HMACSha1EncryptBytes(context, key)).Replace("-", "");
        }
        /// <summary>
        /// 表示基于哈希的消息验证代码 (HMAC) 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string HMACSha1EncryptBase64(string context, byte[] key)
        { 
            return Convert.ToBase64String(HMACSha1EncryptBytes(context,key));
        }
        /// <summary>
        /// 表示基于哈希的消息验证代码 (HMAC) 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] HMACSha1EncryptBytes(string context, byte[] key)
        {
            var bytes = baseEncoding.GetBytes(context);
            var sec_instance = HMACsha1Instance;
            sec_instance.Key = key;
            var outdata = sec_instance.ComputeHash(bytes);
            return  outdata;
        }
        /// <summary>
        /// 表示基于哈希的消息验证代码 (HMAC) 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string HMACSha1Base64String(string context, byte[] key)
        {
            var bytes = baseEncoding.GetBytes(context);
            var sec_instance = HMACsha1Instance;
            sec_instance.Key = key;
            var outdata = sec_instance.ComputeHash(bytes);
            return Convert.ToBase64String(outdata);
        }

        #endregion

        /// <summary>
        /// 采用DES加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string DESEncrypt(string context)
        {
            return desInstance.EncryptDES(context);
        }
        /// <summary>
        /// 采用DES加密协议解密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string DESDecrypt(string context)
        {
            return desInstance.DecryptDES(context);
        }


        /// <summary>
        /// 采用DES加密协议加密字节数组
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static byte[] DESEncrypt(byte[] context)
        {
            return desInstance.EncryptDES(context);
        }
        /// <summary>
        /// 采用DES加密协议解密字节数组
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static byte[] DESDecrypt(byte[] context)
        {
            return desInstance.DecryptDES(context);
        }
        /// <summary>
        /// 采用Base64加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Base64Encrypt(string context)
        {
            return System.Convert.ToBase64String(baseEncoding.GetBytes(context));
        }
        /// <summary>
        /// 采用Base64加密协议解密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Base64Decrypt(string context)
        {
            if (string.IsNullOrEmpty(context)) return "";
            if (string.IsNullOrEmpty(context.Trim())) return "";
            if (context.Trim().Length % 4 != 0) return context;

            return baseEncoding.GetString(Convert.FromBase64String(context));
        }
        /// <summary>
        /// 将字节数组转换为Base64String的表示形式
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToBase64String(byte[] data)
        {
            return Convert.ToBase64String(data);
        }
        /// <summary>
        /// 从base64字符串中还原字节数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] ToBase64Bytes(string data)
        {
            if (string.IsNullOrEmpty(data)) return new byte[0];
            if (string.IsNullOrEmpty(data.Trim())) return new byte[0];
            if (data.Trim().Length % 4 != 0) return new byte[0];

            return Convert.FromBase64String(data);
        }


        /// <summary>
        /// 将字节数组转换成一个简单的Base64格式
        /// </summary>
        /// <param name="data">需要转换的字节数组</param>
        /// <returns></returns>
        public unsafe static string ToSimpleBase64String(byte[] data)
        {
            return Serialization.Base64Serialzier.ToBase64String(data);
        }





        /// <summary>
        /// 将字节数组转换成一个简单的Base64格式
        /// </summary>
        /// <param name="str">需要转换的字节数组</param>
        /// <returns></returns>
        public static byte[] FromSimpleBase64String(string str)
        {
            try
            {
                var base64Builder = new StringBuilder(str);
                base64Builder.Replace("-", "+");
                base64Builder.Replace("_", "/");
                var len = base64Builder.Length;

                var addlen = (4 - (len % 4)) % 4;
                switch (addlen)
                {
                    case 1: { base64Builder.Append("="); break; }
                    case 2: { base64Builder.Append("=="); break; }
                    case 3: { base64Builder.Append("==="); break; }
                }
                return Convert.FromBase64String(base64Builder.ToString());
            }
            catch
            {
                return EmptyBytes;
            }
        }
        /***************************以下定义字符串加解密服务类************************************/

        #region EncryptionHelper

        /// <summary>
        /// 平台数据加密服务的帮助类，提供一些加密验证服务的帮助方法
        /// </summary>
        public static class EncryptionHelper
        {
            /// <summary>
            /// 获取当前时间的时间戳的字符串表示形式
            /// </summary>
            /// <returns></returns>
            public static string GetTimeStamp()
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                string ret = string.Empty;
                ret = Convert.ToInt64(ts.TotalSeconds).ToString();

                return ret;
            }
            /// <summary>
            /// 将时间戳转换为本地时间
            /// </summary>
            /// <param name="timestampStr"></param>
            /// <returns></returns>
            public static DateTime GetDateTimeFromTimeStamp(string timestampStr)
            {

                DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                long time;
                long.TryParse(timestampStr, out time);
                //获取当前时间戳表示的时间信息
                return dtStart.AddSeconds(time);

            }
            /// <summary>
            /// 获取当前时间的时间戳的字符串表示形式
            /// </summary>
            /// <returns></returns>
            public static int GetTimeStampInt32()
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                //string ret = string.Empty;
                return Convert.ToInt32(ts.TotalSeconds);//.ToString();

            }
            /// <summary>
            /// 获取当前时间的时间戳的字符串表示形式
            /// </summary>
            /// <returns></returns>
            public static long GetTimeStampInt64()
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                //string ret = string.Empty;
                return Convert.ToInt64(ts.TotalSeconds);//.ToString();

            }
            /// <summary>
            /// 将时间戳转换为本地时间
            /// </summary>
            /// <param name="timestampStr"></param>
            /// <returns></returns>
            public static DateTime GetDateTimeFromTimeStamp(int timestampStr)
            {
                DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                //获取当前时间戳表示的时间信息
                return dtStart.AddSeconds(timestampStr);
            }



            /// <summary>
            /// 将url字符串进行一次编码处理
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public static string UrlEncode(string context)
            {
                return UrlEncode(context, baseEncoding);
            }
            /// <summary>
            /// 将url字符串进行一次解码处理
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public static string UrlDecode(string context)
            {
                return UrlDecode(context, baseEncoding);
            }
            /// <summary>
            /// 将url字符串进行一次编码处理
            /// </summary>
            /// <param name="context"></param>
            /// <param name="encoder"></param>
            /// <returns></returns>
            public static string UrlEncode(string context, Encoding encoder)
            {
                return HttpUtility.UrlEncode(context, encoder);
            }
            /// <summary>
            /// 将url字符串进行一次解码处理
            /// </summary>
            /// <param name="context"></param>
            /// <param name="encoder"></param>
            /// <returns></returns>
            public static string UrlDecode(string context, Encoding encoder)
            {
                return HttpUtility.UrlDecode(context, encoder);
            }





        }

        #endregion

        #region RSAEncryptServices

        /// <summary>
        /// 服务器通用RSA非对称加密算法
        /// </summary>
        public class RSAEncryptServices
        {
            RSACryptoServiceProvider rsa_pro = new RSACryptoServiceProvider();

            const string _RSA_KEY_XML = "<RSAKeyValue><Modulus>phf9DTc91Sv+4ghHcldmWjv7aPw+AWAzu73B9o2XoCJm+vBAmJXuYM1Y2nVJaO7Okrjueyj4fFwwf4u1iQelDZvKuzMVGlAS1v4NX7fsVyg0/vjvc7x5JG8z2d/bdu361dW4OLJBKndCGlbENtxGbpM9pEESpGTCuSOg3Iac8Gc=</Modulus><Exponent>AQAB</Exponent><P>2FPIZcZ0Qolny5ylIxYQagLsnB24/dWn8WeYOkLsBPtfoYHdWWOhmzv+V+Xdx6yXXrqmhR+Sozda9t93L86Z1w==</P><Q>xI3SBiSOqHhR/Uku2DY/yRMIjlsMWcRr/ithAdilMAYaQBp74y67IzcM+fBPf+Z/vZYkGjZW3XN7g/4M9hEr8Q==</Q><DP>NXvJYPhGyCiGo/2PinQrDLq6WwKyOPe49ONC7KydA8JOa3TbD/2k9+dGQ98ODQ7rwbzt7J3YuEe4Uq7/Ha+7sw==</DP><DQ>EVmXdXVapmJgkmQYX3uCa+RjN/WvhGkDQ19e48PU6QVQ4eG0l9wzJqugWJuu2NJm6jxLmYi8aDXebEtLp5jicQ==</DQ><InverseQ>ZyOQL62C5zW3UwQagJJ+Vphu1ItDSe1MfOEeZb/ZbY4PWyeSGId9T1xvwZ3COfRvhINsx7qDxP4gQ43JTTM+fg==</InverseQ><D>fGqTwAaVZs2iDCcvfdNCdG1iEm6A8/7gQc4PMWU3I1kh0u+NM7975T9tQ5d/+f6I9xdYbSFvoZhdK+23eoIjfCzi7Q7D1Op8O2jJyr7tEMklCu+Br3kYLv+rvxpqcOZyxBLDnkxFOQ0q7ZE/ziV5ghcvrIafHVAnJD5OtjEhAoE=</D></RSAKeyValue>";

            const int MaxDecryptLength = 128;   //RSA最大解密长度
            const int MaxEncryptSize = 117;		//RSA最大加密长度



            /// <summary>
            /// 
            /// </summary>
            public string RSA_KEY_XML { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public RSAEncryptServices()
            {
                RSA_KEY_XML = _RSA_KEY_XML;
                rsa_pro.FromXmlString(_RSA_KEY_XML);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="rsaKeyXml"></param>
            public RSAEncryptServices(string rsaKeyXml)
            {
                RSA_KEY_XML = rsaKeyXml;
                rsa_pro.FromXmlString(rsaKeyXml);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="rsaParam"></param>
            public RSAEncryptServices(RSAParameters rsaParam)
            {
                rsa_pro.ImportParameters(rsaParam);
                if (rsa_pro.PublicOnly)
                {
                    RSA_KEY_XML = rsa_pro.ToXmlString(false);
                }
                else
                {
                    RSA_KEY_XML = rsa_pro.ToXmlString(true);
                }
            }


            /// <summary>
            ///  RSA加密数据
            /// </summary>
            /// <param name="encryptBytes">待加密的字符串</param> 
            /// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
            public byte[] EncryptDES(byte[] encryptBytes)
            {
                int length = encryptBytes.Length;
                int offset = 0;
                byte[] cache;
                MemoryStream outStream = new MemoryStream();
                byte[] buffer1 = new byte[MaxEncryptSize];
                while (true)
                {
                    var len = length - offset;
                    if (len <= 0) break;

                    if (len >= MaxEncryptSize)
                    {
                        System.Buffer.BlockCopy(encryptBytes, offset, buffer1, 0, MaxEncryptSize);
                        len = MaxDecryptLength;
                        cache = rsa_pro.Encrypt(buffer1, false);
                    }
                    else
                    {
                        byte[] buffer2 = new byte[len];
                        System.Buffer.BlockCopy(encryptBytes, offset, buffer2, 0, len);
                        cache = rsa_pro.Encrypt(buffer2, false);
                    }
                    outStream.Write(cache, 0, cache.Length);
                    offset += len;
                }
                return outStream.ToArray();

            }
            /// <summary>
            /// DES解密数据
            /// </summary>
            /// <param name="encryptBytes">待解密的字节数据</param>  
            public byte[] DecryptDES(byte[] encryptBytes)
            {
                int offset = 0;
                var stream = new System.IO.MemoryStream();
                while (offset < encryptBytes.Length)
                {
                    var len = encryptBytes.Length - offset;
                    len = len <= 128 ? len : 128;
                    var tmp = new byte[len];
                    System.Buffer.BlockCopy(encryptBytes, offset, tmp, 0, len);
                    offset += len;
                    var s = rsa_pro.Decrypt(tmp, false);
                    stream.Write(s, 0, s.Length);
                }
                return stream.ToArray();
            }
            /// <summary>
            ///  使用指定的哈希算法计算指定字节数组的哈希值，并对计算所得的哈希值签名。
            /// </summary>
            /// <param name="content"></param>
            /// <param name="ha"></param>
            /// <returns></returns>
            public byte[] SignData(byte[] content, object ha)
            {
                return rsa_pro.SignData(content, ha);
            }
            /// <summary>
            ///  通过用私钥对其进行加密来计算指定哈希值的签名。
            /// </summary>
            /// <param name="content"></param>
            /// <param name="str"></param>
            /// <returns></returns>
            public byte[] SignHash(byte[] content, string str)
            {
                return rsa_pro.SignHash(content, str);
            }
            /// <summary>
            /// 通过将指定的签名数据与为指定数据计算的签名进行比较来验证指定的签名数据。
            /// </summary> 
            public bool VerifyData(byte[] buffer, object halg, byte[] signature)
            {
                return rsa_pro.VerifyData(buffer, halg, signature);
            }

            #region 解析.net 生成的Pem
            /// <summary>
            /// 解析.net 生成的Pem
            /// </summary>
            /// <param name="pemFileConent"></param>
            /// <returns></returns>
            public static RSAParameters ConvertFromPublicKey(string pemFileConent)
            {

                byte[] keyData = Convert.FromBase64String(pemFileConent);
                if (keyData.Length < 162)
                {
                    throw new ArgumentException("pem file content is incorrect.");
                }
                byte[] pemModulus = new byte[128];
                byte[] pemPublicExponent = new byte[3];
                System.Buffer.BlockCopy(keyData, 29, pemModulus, 0, 128);
                System.Buffer.BlockCopy(keyData, 159, pemPublicExponent, 0, 3);
                RSAParameters para = new RSAParameters();
                para.Modulus = pemModulus;
                para.Exponent = pemPublicExponent;
                return para;
            }
            /// <summary>
            /// 解析.net 生成的Pem
            /// </summary>
            /// <param name="pemFileConent"></param>
            /// <returns></returns>
            public static RSAParameters ConvertFromPrivateKey(string pemFileConent)
            {
                byte[] keyData = Convert.FromBase64String(pemFileConent);
                if (keyData.Length < 609)
                {
                    throw new ArgumentException("pem file content is incorrect.");
                }

                int index = 11;
                byte[] pemModulus = new byte[128];
                System.Buffer.BlockCopy(keyData, index, pemModulus, 0, 128);

                index += 128;
                index += 2;//141
                byte[] pemPublicExponent = new byte[3];
                System.Buffer.BlockCopy(keyData, index, pemPublicExponent, 0, 3);

                index += 3;
                index += 4;//148
                byte[] pemPrivateExponent = new byte[128];
                System.Buffer.BlockCopy(keyData, index, pemPrivateExponent, 0, 128);

                index += 128;
                index += ((int)keyData[index + 1] == 64 ? 2 : 3);//279
                byte[] pemPrime1 = new byte[64];
                System.Buffer.BlockCopy(keyData, index, pemPrime1, 0, 64);

                index += 64;
                index += ((int)keyData[index + 1] == 64 ? 2 : 3);//346
                byte[] pemPrime2 = new byte[64];
                System.Buffer.BlockCopy(keyData, index, pemPrime2, 0, 64);

                index += 64;
                index += ((int)keyData[index + 1] == 64 ? 2 : 3);//412/413
                byte[] pemExponent1 = new byte[64];
                System.Buffer.BlockCopy(keyData, index, pemExponent1, 0, 64);

                index += 64;
                index += ((int)keyData[index + 1] == 64 ? 2 : 3);//479/480
                byte[] pemExponent2 = new byte[64];
                System.Buffer.BlockCopy(keyData, index, pemExponent2, 0, 64);

                index += 64;
                index += ((int)keyData[index + 1] == 64 ? 2 : 3);//545/546
                byte[] pemCoefficient = new byte[64];
                System.Buffer.BlockCopy(keyData, index, pemCoefficient, 0, 64);

                RSAParameters para = new RSAParameters();
                para.Modulus = pemModulus;
                para.Exponent = pemPublicExponent;
                para.D = pemPrivateExponent;
                para.P = pemPrime1;
                para.Q = pemPrime2;
                para.DP = pemExponent1;
                para.DQ = pemExponent2;
                para.InverseQ = pemCoefficient;
                return para;
            }
            #endregion
            #region 解析java生成的pem文件私钥


            /// <summary>
            /// 解析java生成的pem文件私钥
            /// </summary>
            /// <param name="pemstr"></param>
            /// <returns></returns>
            public static RSAParameters DecodePemPrivateKey(string pemstr)
            {
                byte[] pkcs8privatekey;
                pkcs8privatekey = Convert.FromBase64String(pemstr);
                if (pkcs8privatekey != null)
                {

                    RSAParameters rsa = DecodePrivateKeyInfo(pkcs8privatekey);
                    return rsa;
                }
                else

                    throw new ArgumentException("pem file content is incorrect.");
            }
            private static RSAParameters DecodePrivateKeyInfo(byte[] pkcs8)
            {
                byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
                byte[] seq = new byte[15];

                MemoryStream mem = new MemoryStream(pkcs8);
                int lenstream = (int)mem.Length;
                BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
                byte bt = 0;
                ushort twobytes = 0;

                try
                {

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        throw new ArgumentException("pem file content is incorrect.");


                    bt = binr.ReadByte();
                    if (bt != 0x02)
                        throw new ArgumentException("pem file content is incorrect.");

                    twobytes = binr.ReadUInt16();

                    if (twobytes != 0x0001)
                        throw new ArgumentException("pem file content is incorrect.");

                    seq = binr.ReadBytes(15);       //read the Sequence OID
                    if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct
                        throw new ArgumentException("pem file content is incorrect.");

                    bt = binr.ReadByte();
                    if (bt != 0x04) //expect an Octet string 
                        throw new ArgumentException("pem file content is incorrect.");


                    bt = binr.ReadByte();       //read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count
                    if (bt == 0x81)
                        binr.ReadByte();
                    else
                        if (bt == 0x82)
                        binr.ReadUInt16();
                    //------ at this stage, the remaining sequence should be the RSA private key

                    byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
                    RSAParameters rsacsp = DecodeRSAPrivateKey(rsaprivkey);
                    return rsacsp;
                }

                catch (System.Exception)
                {
                    throw;
                }

                finally { binr.Close(); }

            }
            private static RSAParameters DecodeRSAPrivateKey(byte[] privkey)
            {
                byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

                // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
                MemoryStream mem = new MemoryStream(privkey);
                BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
                byte bt = 0;
                ushort twobytes = 0;
                int elems = 0;
                try
                {
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        throw new ArgumentException("pem file content is incorrect.");

                    twobytes = binr.ReadUInt16();
                    if (twobytes != 0x0102) //version number
                        throw new ArgumentException("pem file content is incorrect.");
                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        throw new ArgumentException("pem file content is incorrect.");


                    //------  all private key components are Integer sequences ----
                    elems = GetIntegerSize(binr);
                    MODULUS = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    E = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    D = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    P = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    Q = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    DP = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    DQ = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    IQ = binr.ReadBytes(elems);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key ----- 
                    RSAParameters RSAparams = new RSAParameters();
                    RSAparams.Modulus = MODULUS;
                    RSAparams.Exponent = E;
                    RSAparams.D = D;
                    RSAparams.P = P;
                    RSAparams.Q = Q;
                    RSAparams.DP = DP;
                    RSAparams.DQ = DQ;
                    RSAparams.InverseQ = IQ;
                    return RSAparams;
                }
                catch (System.Exception)
                {
                    throw new ArgumentException("pem file content is incorrect.");
                }
                finally { binr.Close(); }
            }
            private static bool CompareBytearrays(byte[] a, byte[] b)
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
            private static int GetIntegerSize(BinaryReader binr)
            {
                byte bt = 0;
                byte lowbyte = 0x00;
                byte highbyte = 0x00;
                int count = 0;
                bt = binr.ReadByte();
                if (bt != 0x02)     //expect integer
                    return 0;
                bt = binr.ReadByte();

                if (bt == 0x81)
                    count = binr.ReadByte();    // data size in next byte
                else
                    if (bt == 0x82)
                {
                    highbyte = binr.ReadByte();	// data size in next 2 bytes
                    lowbyte = binr.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else
                {
                    count = bt;		// we already have the data size
                }



                while (binr.ReadByte() == 0x00)
                {   //remove high order zeros in data
                    count -= 1;
                }
                binr.BaseStream.Seek(-1, SeekOrigin.Current);       //last ReadByte wasn't a removed zero, so back up a byte
                return count;
            }

            #endregion


        }

        #endregion



    }


}

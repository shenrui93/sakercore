using SakerCore.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SakerCoreXUnitTest.Serialization
{
    public class Base64SerialzierTest
    {
        [Xunit.Fact]
        public void ToBase64String()
        {
            byte[] buffer = new byte[5] { 1, 2, 3, 4, 5 };
            var r = Base64Serialzier.ToBase64String(buffer);

            if (r != "AQIDBAU")
                throw new Exception("序列化错误");

            var b = Base64Serialzier.FromBase64String(r);
        }
    }
}

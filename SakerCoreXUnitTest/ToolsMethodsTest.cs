

using SakerCore;
using System;
using Xunit;

namespace SakerCoreXUnitTest.SakerCore
{ 
    public class ToolsMethodsTest
    {
        [Fact]
        public void WildMatchByPtr()
        {

            string a = "a12345";
            string[] ma;
            var result = ToolsMethods.WildMatchByPtr(a, "a*", out ma);
            if (!result)
            { 
               throw new Exception("方法匹配失败！"); 
            }
            if (ma == null || ma.Length != 1)
            {
                throw new Exception("输出的匹配参数错误！");
            }
            if (ma[0] != "12345")
            {
                throw new Exception("输出的匹配结果错误！");
            }

            a = null;
            result = ToolsMethods.WildMatchByPtr(a, "a*", out ma);
            if (!result && ma == null) ;
            else
            {
                throw new Exception("输出的匹配结果错误！");
            }

            const string r = "◣◥◢◣◤ ◥№↑↓→←↘";
            a = @"︻︼︽︾〒↑↓☉⊙●〇◎¤★☆■▓「」『』◆◇▲△▼▽◣◥◢◣◤ ◥№↑↓→←↘↙Ψ※㊣∑⌒∩【】〖〗＠ξζω□∮〓※》∏卐√ ╳々♀♂∞①ㄨ≡╬╭╮╰╯╱╲ ▂ ▂ ▃ ▄ ▅ ▆ ▇ █ ▂▃▅▆█ ▁▂▃▄▅▆▇█▇▆▅▄▃▂▁";
            var pa = @"︻︼︽︾〒↑↓☉⊙●〇◎¤★☆■▓「」『』◆◇▲△▼▽*↙Ψ※㊣∑⌒∩【】〖〗＠ξζω□∮〓※》∏卐√ ╳々♀♂∞①ㄨ≡╬╭╮╰╯╱╲ ▂ ▂ ▃ ▄ ▅ ▆ ▇ █ ▂▃▅▆█ ▁▂▃▄▅▆▇█▇▆▅▄▃▂▁";
            result = ToolsMethods.WildMatchByPtr(a, pa, out ma);
            if (result && ma != null && ma[0] == r) ;
            else
            {
                throw new Exception("输出的匹配参数错误！"); 
            }
        }

    }
}

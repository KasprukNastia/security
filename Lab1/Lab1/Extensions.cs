using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lab1
{
    public static class Extensions
    {
        public static string HexStringToString(this string hexString)
        {
            return string.Join("", Regex.Split(hexString, "(?<=\\G..)(?!$)").Select(x => (char)Convert.ToByte(x, 16)));
        }
    }
}

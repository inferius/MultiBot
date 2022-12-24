using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoE.Farmer.Library.Windows.Helpers
{
    public static class AntiProtect
    {
        private static Dictionary<string, string> protectedNames = new Dictionary<string, string>();

        private static Random random = new Random();
        public static string RandomString(int length, bool hasNumber = true)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
            const string charsWithNum = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            return new string(Enumerable.Repeat(hasNumber ? charsWithNum  : chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetAntiProtectFunctionName(string name)
        {
            if (protectedNames.ContainsKey(name)) return protectedNames[name];
            else
            {
                var min = random.Next(5, 10);
                var max = random.Next(min, min + random.Next(5, 20));

                var n = RandomString(3, false) + RandomString(random.Next(min, max));

                protectedNames.Add(name, n);

                return n;
            }
        }
    }

    public static class AP
    {
        public static string _f(string name)
        {
            return name;
            //return AntiProtect.GetAntiProtectFunctionName(name);
        }
    }
}

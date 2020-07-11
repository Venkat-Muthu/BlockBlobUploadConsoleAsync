using System;
using System.Text;

namespace BlockBlobConsole
{
    public static class Extensions
    {
        public static string ToBase64(this string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }

        public static string ToBase64(this long s)
        {
            return ToBase64(s.ToString("d20"));
        }
    }
}

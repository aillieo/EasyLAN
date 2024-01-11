using System.Linq;

namespace AillieoUtils.EasyLAN
{
    public static class ByteArrayExtensions
    {
        public static string ToStringEx(this byte[] bytes)
        {
            return string.Join("-", bytes.Select(b => b.ToString()));
        }
    }
}

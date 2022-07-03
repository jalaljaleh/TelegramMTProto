using System;
using System.Linq;

namespace TelegramMTProto
{
    internal static class ArrayExtensions
    {
        private static readonly Random Random = new Random();
        public static void GenerateRandomBytes(this byte[] buffer)
        {
            Random.NextBytes(buffer);
        }
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Buffer.BlockCopy(data, index, result, 0, length);
            return result;
        }
        public static byte[] HexToByteArray(this string hexString)
        {
            return Enumerable.Range(0, hexString.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
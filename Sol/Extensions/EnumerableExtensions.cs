﻿#region USING_DIRECTIVES

using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

#endregion USING_DIRECTIVES

namespace Sol.Extensions
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
        {
            if (!items.Any())
                return items;

            using (var provider = RandomNumberGenerator.Create())
            {
                var list = items.ToList();
                int n = list.Count;
                while (n > 1)
                {
                    byte[] box = new byte[(n / byte.MaxValue) + 1];
                    int boxSum;
                    do
                    {
                        provider.GetBytes(box);
                        boxSum = box.Sum(b => b);
                    } while (!(boxSum < n * ((byte.MaxValue * box.Length) / n)));
                    int k = (boxSum % n);
                    n--;
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
                return list;
            }
        }
    }
}
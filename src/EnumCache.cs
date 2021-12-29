using System;
using System.Collections.Generic;

namespace Zergatul.Obs.InputOverlay
{
    public class EnumCache<T>
        where T : struct, Enum
    {
        private readonly Dictionary<T, string> dictionary;

        public EnumCache()
        {
            dictionary = new Dictionary<T, string>();
            foreach (T value in Enum.GetValues<T>())
            {
                dictionary.Add(value, value.ToString());
            }
        }

        public string this[T value] => dictionary[value];
    }
}
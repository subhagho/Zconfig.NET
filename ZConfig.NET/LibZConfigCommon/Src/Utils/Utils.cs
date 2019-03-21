using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LibZConfig.Common.Utils
{
    public static class General
    {
        public static Dictionary<K, T> Clone<K, T>(Dictionary<K, T> map)
        {
            Dictionary<K, T> dict = new Dictionary<K, T>();
            if (map != null && map.Count > 0)
            {
                foreach(K key in map.Keys)
                {
                    dict.Add(key, map[key]);
                }
            }
            return dict;
        }

        public static string Capitalize(this string value)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(value));
            return String.Format("{0}{1}", Char.ToUpper(value[0]), value.Substring(1));
        }
    }
}

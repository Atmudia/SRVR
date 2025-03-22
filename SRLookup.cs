using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
using UnityEngine;
using System;

namespace SRVR
{
    public static class SRLookup
    {
        private static readonly Dictionary<Type, Object[]> cache = new Dictionary<Type, Object[]>();

        public static T Get<T>(string name) where T : Object
        {
            Type selected = typeof(T);
            if (!cache.ContainsKey(selected))
                cache.Add(selected, Resources.FindObjectsOfTypeAll<T>());

            T found = (T)cache[selected].FirstOrDefault(x => x.name == name);
            if (found == null)
            {
                cache[selected] = Resources.FindObjectsOfTypeAll<T>();
                found = (T)cache[selected].FirstOrDefault(x => x.name == name);
            }

            return found;
        }
        public static T GetCopy<T>(string name) where T : Object =>
            Object.Instantiate(Get<T>(name));

        public static T Get<T>(string name, System.Func<T, bool> predicate) where T : Object
        {
            Type selected = typeof(T);
            if (!cache.ContainsKey(selected))
                cache.Add(selected, Resources.FindObjectsOfTypeAll<T>());

            T found = (T)cache[selected].FirstOrDefault(x => x.name == name && predicate((T)x));
            if (found == null)
            {
                cache[selected] = Resources.FindObjectsOfTypeAll<T>();
                found = (T)cache[selected].FirstOrDefault(x => x.name == name && predicate((T)x));
            }

            return found;
        }

        public static T[] GetAll<T>(string name) where T : Object
        {
            Type selected = typeof(T);
            if (!cache.ContainsKey(selected))
                cache.Add(selected, Resources.FindObjectsOfTypeAll<T>());

            T[] found = cache[selected].Where(x => x.name == name).Select(y => (T)y).ToArray();
            if (found.Length == 0)
            {
                cache[selected] = Resources.FindObjectsOfTypeAll<T>();
                found = cache[selected].Where(x => x.name == name).Select(y => (T)y).ToArray();
            }

            return found;
        }
    }
}

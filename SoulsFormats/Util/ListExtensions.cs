using System.Collections.Generic;

namespace SoulsFormats.Util {
    internal static class ListExtensions {
        public static T EchoAdd<T>(this List<T> list, T item) {
            list.Add(item);
            return item;
        }
    }
}

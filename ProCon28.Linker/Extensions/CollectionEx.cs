using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker.Extensions
{
    public static class CollectionEx
    {
        public static void AddRange<T>(this ICollection<T> Collection, IEnumerable<T> Items)
        {
            foreach (T item in Items)
                Collection.Add(item);
        }
    }
}

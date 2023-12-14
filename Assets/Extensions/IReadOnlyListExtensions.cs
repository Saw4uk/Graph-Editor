using System.Collections.Generic;


public static class IReadOnlyListExtensions
{
    public static int FindIndex<T>(this IReadOnlyList<T> list, T obj)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Equals(obj)) return i;
        }

        return -1;
    }
}
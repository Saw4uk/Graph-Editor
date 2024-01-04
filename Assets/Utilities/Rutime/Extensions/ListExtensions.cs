using System;
using System.Collections.Generic;


public static class ListExtensions
{
    public static int FindIndex<T>(this IReadOnlyList<T> list, T obj)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Equals(obj)) return i;
        }

        return -1;
    }
    
    public static IEnumerable<T> GetRandomElements<T>(this IReadOnlyList<T> list, Random random, int elementsCount)
    {
        if (elementsCount < 0)
            throw new InvalidOperationException();
        if (list.Count == 0 || elementsCount == 0)
            yield break;
        var visitedElements = new HashSet<int>();

        elementsCount = Math.Min(elementsCount, list.Count);
        
        for (var i = 0; i < elementsCount; i++)
        {
            var index = random.Next(0, list.Count);
            while (visitedElements.Contains(index))
                index = random.Next(0, list.Count);
            
            visitedElements.Add(index);
            yield return list[index];
        }
    }
}
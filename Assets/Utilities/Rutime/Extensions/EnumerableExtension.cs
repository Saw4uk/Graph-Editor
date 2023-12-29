using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;


public static class EnumerableExtension
{
    public static Stack<T> ToStack<T>(this IEnumerable<T> enumerable, bool useReverse = false)
    {
        var tempList = enumerable.ToList();
        if (useReverse)
            tempList.Reverse();

        var result = new Stack<T>();
        foreach (var element in tempList)
        {
            result.Push(element);
        }

        return result;
    }

    /// <summary>
    /// Берет компонент у каждого элемента коллекции
    /// </summary>
    public static IEnumerable<TComponent> GetComponentMany<TComponent>(this IEnumerable<GameObject> enumerable)
    {
        return enumerable
            .Select(x => x.GetComponent<TComponent>())
            .Where(x => x != null);
    }

    public static T MaxItem<T>(this IEnumerable<T> enumerable, Comparison<T> comparison)
    {
        return enumerable.Aggregate((i1, i2) => comparison(i1, i2) > 0 ? i1 : i2);
    }

    public static T MinItem<T>(this IEnumerable<T> enumerable, Comparison<T> comparison)
    {
        return enumerable.Aggregate((i1, i2) => comparison(i1, i2) < 0 ? i1 : i2);
    }

    public static void SetActiveAll(this IEnumerable<GameObject> gameObjects, bool value)
    {
        foreach (var gameObject in gameObjects)
            gameObject.SetActive(value);
    }
    public static IEnumerable<GameObject> GameObjects(this IEnumerable<Component> components)
    {
        return components.Select(x => x.gameObject);
    }
    
    public static IEnumerable<int> GetRandomIntEnumerable(Random random, int minValue, int maxValue)
    {
        while (true)
        {
            yield return random.Next(minValue, maxValue);
        }
    }
}
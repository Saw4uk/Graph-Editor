using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

namespace DataStructures
{
    public class RandomChanceList<T> //Придумать другое название?
    {
        private readonly List<ChanceItem<T>> list = new List<ChanceItem<T>>();
        private float sumChance;

        public void Add(T obj, float chance)
        {
            list.Add(new ChanceItem<T>(obj, sumChance, sumChance + chance));
            sumChance += chance;
        }

        public T PickRandomObject()
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException("List is empty!");
            }
            var randomValue = Random.Range(0, sumChance);
            foreach (var item in list)
            {
                if (randomValue >= item.MinChance && randomValue <= item.MaxChance)
                    return item.Object;
            }

            throw new ArgumentException("RandomChanceList has failed to choose an object!");
        }

        private class ChanceItem<T1>
        {
            public readonly T1 Object;
            public readonly float MinChance;
            public readonly float MaxChance; //?можно обойтись без этой переменной?

            public ChanceItem(T1 obj, float minChance, float maxChance)
            {
                this.Object = obj;
                this.MinChance = minChance;
                this.MaxChance = maxChance;
            }

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DataStructures
{
    public class IndexList<T> : IReadOnlyIndexList<T>
    {
        private int lastIndex;
        private readonly Dictionary<int, T> indexToObject;
        private readonly Dictionary<T, int> objectToIndex;
        private readonly HashSet<int> indexesSet;

        public IReadOnlyDictionary<T, int> ObjectToIndex => objectToIndex;

        public IndexList() 
        {
            lastIndex = 0;
            indexToObject = new Dictionary<int, T>();
            objectToIndex = new Dictionary<T, int>();
            indexesSet = new HashSet<int>();    
        }

        public IndexList(IReadOnlyDictionary<int, T> dict) :this()
        {
            var maxIndex = int.MinValue;
            foreach (var pair in dict)
            {
                indexToObject[pair.Key] = pair.Value;
                objectToIndex[pair.Value] = pair.Key;
                maxIndex = Mathf.Max(maxIndex, pair.Key);
            }
            lastIndex = maxIndex + 1;
        }

        public int Add(T item)
        {
            var index = lastIndex;
            Add(index, item);
            lastIndex++;
            while(indexesSet.Contains(lastIndex))
                lastIndex++;

            return index;
        }

        public void Add(int index, T item)
        {
            if (indexesSet.Contains(index)) 
                throw new ArgumentException("Index is already taken!");

            indexToObject[index] = item;
            objectToIndex[item] = index;
            indexesSet.Add(index);
        }

        public void Remove(T item)
        {
            var index = objectToIndex[item];
            indexToObject.Remove(index);
            objectToIndex.Remove(item);
            indexesSet.Remove(index);
        }

        public void Remove(int index)
        {
            var obj = indexToObject[index];
            Remove(obj);
        }

        public void Clear()
        {
            lastIndex = 0;
            indexToObject.Clear();
            objectToIndex.Clear();
            indexesSet.Clear();
        }

        public static IndexList<T> FromList(IEnumerable<T> list)
        {
            var indexList = new IndexList<T>();

            foreach(var item in list)
            {
                indexList.Add(item);
            }

            return indexList;
        }



        #region IReadOnlyDictionary Realization
        public bool ContainsKey(int key)
        {
            return indexToObject.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            return indexToObject.GetEnumerator();
        }

        public bool TryGetValue(int key, out T value)
        {
            return indexToObject.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)indexToObject).GetEnumerator();  
        }

        public IEnumerable<int> Keys => indexToObject.Keys;
        public IEnumerable<T> Values => indexToObject.Values;
        public int Count => indexToObject.Count;
        public T this[int key] => indexToObject[key];
        #endregion
    }

    public interface IReadOnlyIndexList<T> : IReadOnlyDictionary<int, T>
    {
        IReadOnlyDictionary<T, int> ObjectToIndex { get; }
    }
}

using System;
using UnityEngine;

namespace DataStructures
{
    public class Singleton<T>: MonoBehaviour
        where T: Singleton<T>
    {
        private static T instance;

        public static T Instance => instance;

        protected virtual void Awake()
        {
            if (GetType() != typeof(T))
            {
                Debug.LogError($"The singleton object {typeof(T).Name} does not refer to himself");
                return;
            }

            if (instance == null)
            {
                instance = (T) this;
            }
            else
            {
                Debug.LogError($"Duplicated {typeof(T).Name} singleton object in this scene!");
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}
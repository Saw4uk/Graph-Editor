using UnityEngine;

namespace DataStructures
{
    public abstract class DontDestroyOnLoadSingleton<T>: MonoBehaviour
        where T: DontDestroyOnLoadSingleton<T>
    {
        private static T instance;

        public static T Instance => instance;

        void Awake()
        {
            if (GetType() != typeof(T))
            {
                Debug.LogError($"The singleton object {typeof(T).Name} does not refer to himself");
                return;
            }

            if (instance == null)
            {
                instance = (T) this;
                DontDestroyOnLoad(gameObject);
                OnAwake();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected abstract void OnAwake();
    }
}
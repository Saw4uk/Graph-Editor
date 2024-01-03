using UnityEditor;
using UnityEngine;

namespace GraphEditor
{
    public class EditorObjectCreator : IObjectCreator
    {
        public TObj CreateInstance<TObj>(TObj obj) where TObj : Object
        {
            return (TObj) PrefabUtility.InstantiatePrefab(obj);
        }

        public TObj CreateInstance<TObj>(TObj obj, Transform parent) where TObj : Object
        {
            return (TObj) PrefabUtility.InstantiatePrefab(obj, parent);
        }

        public void ReleaseInstance<TObj>(TObj obj) where TObj : Object
        {
            Debug.Log($"Delete {typeof(TObj).Name} {obj.name}");
            Object.DestroyImmediate(obj);
        }
    }
}
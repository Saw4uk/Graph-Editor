using UnityEditor;
using UnityEngine;

namespace GraphEditor
{
    public class EditorObjectCreatorAndDestroyer : IObjectCreatorAndDestroyer
    {
        public TObj CreateInstance<TObj>(TObj obj) where TObj : Object
        {
            var instance = (TObj) PrefabUtility.InstantiatePrefab(obj);
            return instance;
        }

        public TObj CreateInstance<TObj>(TObj obj, Transform parent) where TObj : Object
        {
            var instance = (TObj) PrefabUtility.InstantiatePrefab(obj, parent);
            return instance;
        }

        public void ReleaseInstance<TObj>(TObj obj) where TObj : Object
        {
            Object.DestroyImmediate(obj);
        }
    }
}
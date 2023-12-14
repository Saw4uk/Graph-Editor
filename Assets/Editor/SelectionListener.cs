using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class SelectionListener<T>
    where T : MonoBehaviour
{
    private bool needUpdateHash;
    private T[] hashedActiveNodes;
    private T[] hashedDisableNodes;
    private T[] hashedActivatedNodes;

    public SelectionListener()
    {
        hashedActiveNodes = Array.Empty<T>();
        hashedDisableNodes = Array.Empty<T>();
        hashedActivatedNodes = Array.Empty<T>();

        Selection.selectionChanged += OnSelectionChanged;
    }

    ~SelectionListener()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }


    private void OnSelectionChanged() => needUpdateHash = true;

    public IEnumerable<T> GetActive()
    {
        UpdateHash();
        return hashedActiveNodes;
    }

    public IEnumerable<T> GetDisabled()
    {
        UpdateHash();
        return hashedDisableNodes;
    }

    public IEnumerable<T> GetActivated()
    {
        UpdateHash();
        return hashedActivatedNodes;
    }

    private void UpdateHash()
    {
        if (!needUpdateHash) return;

        var oldHashedNodes = hashedActiveNodes;

        hashedActiveNodes = Selection.gameObjects
            .Where(x => x.scene.name != null && x.scene.name != x.name)
            .Select(o => o.GetComponent<T>())
            .Where(o => o != null)
            .ToArray();

        hashedDisableNodes = oldHashedNodes
            .Except(hashedActiveNodes)
            .Where(x => x != null)
            .ToArray();

        hashedActivatedNodes = hashedActiveNodes
            .Except(oldHashedNodes)
            .Where(x => x != null)
            .ToArray();

        needUpdateHash = false;
    }
}
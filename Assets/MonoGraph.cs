using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

public class MonoGraph : MonoBehaviour
{
    public static MonoGraph Instance { get; private set; }

    [field: SerializeField] public GameObject NodesParent { get; set; }
    [field: SerializeField] public GameObject EdgesParent { get; set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogError("Более одного графа!");
            Destroy(gameObject);
        }
    }
}

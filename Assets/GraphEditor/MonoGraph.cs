using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

public class MonoGraph : MonoBehaviour
{
    [field: SerializeField] public GameObject NodesParent { get; set; }
    [field: SerializeField] public GameObject EdgesParent { get; set; }
}

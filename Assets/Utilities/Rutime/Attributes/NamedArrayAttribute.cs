﻿using UnityEngine;


public class NamedArrayAttribute : PropertyAttribute
{
    public string VarName { get; }

    public NamedArrayAttribute(string elementTitleVar = "")
    {
        VarName = elementTitleVar;
    }
}
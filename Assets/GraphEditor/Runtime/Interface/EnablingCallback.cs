using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnablingCallback : MonoBehaviour
{
    public UnityEvent Enabled; 
    public UnityEvent Disabled; 
    private void OnEnable()
    {
        Enabled?.Invoke();
    }
    
    private void OnDisable()
    {
        Disabled?.Invoke();
    }
}

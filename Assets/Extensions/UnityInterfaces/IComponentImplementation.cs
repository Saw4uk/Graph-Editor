using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


public interface IComponentImplementation : IObjectImplementation
{
    Transform transform { get; }
    GameObject gameObject { get; }
    string tag { get; set; }
    Component rigidbody { get; }
    Component rigidbody2D { get; }
    Component camera { get; }
    Component light { get; }
    Component animation { get; }
    Component constantForce { get; }
    Component renderer { get; }
    Component audio { get; }
    Component networkView { get; }
    Component collider { get; }
    Component collider2D { get; }
    Component hingeJoint { get; }
    Component particleSystem { get; }
    Component GetComponent(Type type);
    T GetComponent<T>();
    Component GetComponent(string type);
    bool TryGetComponent(Type type, out Component component);
    bool TryGetComponent<T>(out T component);
    Component GetComponentInChildren(Type t, bool includeInactive);
    Component GetComponentInChildren(Type t);
    T GetComponentInChildren<T>(bool includeInactive);
    T GetComponentInChildren<T>();
    Component[] GetComponentsInChildren(Type t, bool includeInactive);
    Component[] GetComponentsInChildren(Type t);
    T[] GetComponentsInChildren<T>(bool includeInactive);
    void GetComponentsInChildren<T>(bool includeInactive, List<T> result);
    T[] GetComponentsInChildren<T>();
    void GetComponentsInChildren<T>(List<T> results);
    Component GetComponentInParent(Type t, bool includeInactive);
    Component GetComponentInParent(Type t);
    T GetComponentInParent<T>(bool includeInactive);
    T GetComponentInParent<T>();
    Component[] GetComponentsInParent(Type t, bool includeInactive);
    Component[] GetComponentsInParent(Type t);
    T[] GetComponentsInParent<T>(bool includeInactive);
    void GetComponentsInParent<T>(bool includeInactive, List<T> results);
    T[] GetComponentsInParent<T>();
    Component[] GetComponents(Type type);
    void GetComponents(Type type, List<Component> results);
    void GetComponents<T>(List<T> results);
    T[] GetComponents<T>();
    bool CompareTag(string tag);
    void SendMessageUpwards(string methodName, object value, SendMessageOptions options);
    void SendMessageUpwards(string methodName, object value);
    void SendMessageUpwards(string methodName);
    void SendMessageUpwards(string methodName, SendMessageOptions options);
    void SendMessage(string methodName, object value);
    void SendMessage(string methodName);
    void SendMessage(string methodName, object value, SendMessageOptions options);
    void SendMessage(string methodName, SendMessageOptions options);
    void BroadcastMessage(string methodName, object parameter, SendMessageOptions options);
    void BroadcastMessage(string methodName, object parameter);
    void BroadcastMessage(string methodName);
    void BroadcastMessage(string methodName, SendMessageOptions options);
}
using System.Collections;
using UnityEngine;


public interface IMonoBehaviorImplementation : IBehaviorImplementation
{
    bool useGUILayout { get; set; }
    bool runInEditMode { get; set; }
    bool IsInvoking();
    bool IsInvoking(string methodName);
    void CancelInvoke();
    void CancelInvoke(string methodName);
    void Invoke(string methodName, float time);
    void InvokeRepeating(string methodName, float time, float repeatRate);
    Coroutine StartCoroutine(string methodName);
    Coroutine StartCoroutine(string methodName, object value);
    Coroutine StartCoroutine(IEnumerator routine);
    Coroutine StartCoroutine_Auto(IEnumerator routine);
    void StopCoroutine(IEnumerator routine);
    void StopCoroutine(Coroutine routine);
    void StopCoroutine(string methodName);
    void StopAllCoroutines();
}
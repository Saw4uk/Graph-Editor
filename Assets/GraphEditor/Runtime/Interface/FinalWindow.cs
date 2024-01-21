using System.Collections;
using System.Collections.Generic;
using GraphEditor.Runtime;
using TMPro;
using UnityEngine;

public class FinalWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text resultMark;
    [SerializeField] private TMP_Text description;

    public void ShowWindow(float resultMark)
    {
        gameObject.SetActive(true);
        this.resultMark.text = $"{resultMark.ToString()} / {BaseTask.MaxMark}";
    }
}

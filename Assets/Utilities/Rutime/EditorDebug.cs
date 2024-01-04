using UnityEngine;

public static class EditorDebug
{
    public static void Log(string massage, Object context = null)
    {
        if (!Application.isEditor) return;
        
        if (context == null)
            Debug.Log(massage);
        else
            Debug.Log(massage, context);
    }
    
    public static void LogWarning(string massage, Object context = null)
    {
        if (!Application.isEditor) return;
        
        if (context == null)
            Debug.LogWarning(massage);
        else
            Debug.LogWarning(massage, context);
    }
    
    public static void LogError(string massage, Object context = null)
    {
        if (!Application.isEditor) return;
        
        if (context == null)
            Debug.LogError(massage);
        else
            Debug.LogError(massage, context);
    }
}
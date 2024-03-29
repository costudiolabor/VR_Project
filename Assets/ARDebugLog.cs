using System.Collections;
using UnityEngine;

public class ARDebugLog : MonoBehaviour
{
    [SerializeField] private int fontSize;
    [SerializeField] private Color fontColor;
    private Queue myLogQueue = new Queue();
    uint qsize = 15; // number of messages to keep    Queue myLogQueue = new Queue();
    private GUIStyle _guiStyle;

    private void Awake()
    {
        Debug.Log("Started up logging.");
        _guiStyle = new GUIStyle();
        _guiStyle.fontSize = fontSize;
        _guiStyle.normal.textColor = fontColor;
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLogQueue.Enqueue("[" + System.DateTime.Now.ToString("HH:mm:ss") + "]" + "[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()), _guiStyle);
        GUILayout.EndArea();
    }
}
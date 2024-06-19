using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogsToTextComponent : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;

    void Start() {
        Reset();
    }

    public void Reset() {
        Debug.Log("Reset logs");
        logText.text = "";
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
        if (type == LogType.Error || type == LogType.Exception)
        {
            Debug.Log("Found an error");
            string message = $"{type}: {logString}\n";
            logText.text += message;
            Debug.Log($"We just added {message} to the log output");

            // Optionally, you can limit the log size to avoid performance issues
            if (logText.text.Length > 5000)
            {
                logText.text = logText.text.Substring(logText.text.Length - 5000);
            }
        }
    }
}

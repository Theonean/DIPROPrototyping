using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum LogLevel
{
    INFO = 1,
    WARNING = 2,
    ERROR = 4,
    DEBUG = 8,
    FORCE = 16 //Always log, regardless of filter
}

[Flags] //makes it possible to multi-select
public enum LogType
{
    LOGGER = 1,
    DRONE = 2,
    ROCKETS = 4,
    ENEMY = 8,
    RESOURCE = 16,
    HARVESTER = 32,
    WAVEMANAGEMENT = 64,
    //ergänzen wie man sich lustig fühlt :), sollte granular genug sein um Fehler zu finden oder systeme nachzuvollziehen
}

public class LogEntry
{
    public LogType logType;
    public LogLevel logLevel;
    public string message;
    public float timestamp;

    public LogEntry(LogType logType, LogLevel logLevel, string message, float timestamp)
    {
        this.logType = logType;
        this.logLevel = logLevel;
        this.message = message;
        this.timestamp = timestamp;
    }
}

public class Logger : MonoBehaviour
{
    public static Logger Instance { get; private set; }

    [Header("Filtering")]
    [SerializeField] private LogType typeFilter = LogType.LOGGER;
    [SerializeField] private LogLevel levelFilter = LogLevel.INFO;

    [Header("Logging")]
    [Tooltip("If true, logs will be saved to a file")]
    [SerializeField] private bool logToFile = false;
    private string logFilePath = "";
    [SerializeField] private string logFileName = "log.txt";


    private List<LogEntry> logs = new List<LogEntry>();
    private int maxLogsBeforeWrite = 1000;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        logFilePath = Application.persistentDataPath + "/" + logFileName;
        Log("Logger initialized, saving logs to " + logFilePath + " writing to disk: " + logToFile, LogLevel.INFO, LogType.LOGGER);
    }

    void OnDisable()
    {
        WriteLogsToFile();
    }

    public static void Log(string message, LogLevel logLevel, LogType logType)
    {
        Instance.AddEntry(new LogEntry(logType, logLevel, message, Time.time));

        if ((Instance.levelFilter.HasFlag(logLevel) && Instance.typeFilter.HasFlag(logType)) || logLevel.HasFlag(LogLevel.FORCE))
        {
            switch (logLevel)
            {
                case LogLevel.INFO:
                    Debug.Log(message);
                    break;
                case LogLevel.WARNING:
                    Debug.LogWarning(message);
                    break;
                case LogLevel.ERROR:
                    Debug.LogError(message);
                    break;
            }
        }
    }

    private void AddEntry(LogEntry entry)
    {
        logs.Add(entry);
        if (logs.Count > maxLogsBeforeWrite)
        {
            WriteLogsToFile();
        }
    }

    private void WriteLogsToFile()
    {
        if (!logToFile)
        {
            return;
        }

        string logString = "";
        foreach (LogEntry entry in logs)
        {
            logString += entry.timestamp + " | " + entry.logType.ToString() + " | " + entry.logLevel.ToString() + " | " + entry.message + "\n";
        }

        System.IO.File.AppendAllText(logFilePath, logString);
        Log("Wrote logs to file " + logFilePath, LogLevel.INFO, LogType.LOGGER);
        logs.Clear();
    }
}
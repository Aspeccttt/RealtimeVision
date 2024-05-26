#if UNITY_EDITOR
using UnityEditor;
#endif
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;

public class DatabaseManager : MonoBehaviour
{

    #region User Data Variables
    private Dictionary<string, float> plotStartTimes = new Dictionary<string, float>();
    private Dictionary<string, string> currentCSVFiles = new Dictionary<string, string>();
    private Dictionary<string, int> plotCounts = new Dictionary<string, int>();
    private string activePlotType;
    private List<string> clickedDatapoints = new List<string>();

    private float appStartTime;
    #endregion

    #region DB Variables
    private int plotCount = 0;
    public int playCount = 0;
    private int logCount;
    private Dictionary<string, int> logsCounter = new Dictionary<string, int>();
    private DatabaseReference db;
    #endregion

    void Start()
    {
        appStartTime = Time.time;

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

        db = FirebaseDatabase.DefaultInstance.RootReference;
        InitializePlayCount();
    }

    public void InitializePlayCount()
    {
        db.Child("playCount").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    playCount = int.Parse(snapshot.Value.ToString());
                }
                else
                {
                    playCount = 0; // Default value if playCount does not exist
                }

                // Start a new play session after initialization
                StartNewPlaySession();
            }
            else
            {
                Debug.LogError("Failed to get play count.");
            }
        });
    }

    public void StartNewPlaySession()
    {
        playCount++; // Increment play count for each new session
        db.Child("playCount").SetValueAsync(playCount).ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("Failed to update play count.");
            }
        });
    }

    public void StartPlotTimer(string plotType, string columnInfo)
    {
        if (db == null)
        {
            Debug.LogError("Database reference is not initialized.");
            return;
        }

        if (activePlotType != plotType)
        {
            if (activePlotType != null && plotStartTimes.ContainsKey(activePlotType))
            {
                StopPlotTimer(activePlotType);
            }

            plotStartTimes[plotType] = Time.time;
            if (!plotCounts.ContainsKey(plotType))
            {
                plotCounts[plotType] = 0;
            }

            plotCounts[plotType]++;
            activePlotType = plotType;
        }

        db.Child("Play " + playCount).Child(plotType).Child("CSVChosen").SetValueAsync(columnInfo);
    }

    public void StopPlotTimer(string plotType)
    {
        if (db == null)
        {
            Debug.LogError("Database reference is not initialized.");
            return;
        }

        if (plotStartTimes.ContainsKey(plotType))
        {
            float startTime = plotStartTimes[plotType];
            float elapsedTime = Time.time - startTime;
            plotStartTimes.Remove(plotType);

            int logIndex = plotCounts.ContainsKey(plotType) ? plotCounts[plotType] : 1;
            db.Child("Play " + playCount).Child(plotType).Child("Plot " + logIndex).Child("PlottingDuration").SetValueAsync(elapsedTime);
        }
    }

    public void LogDatapointClick(string datapointName)
    {
        clickedDatapoints.Add(datapointName);
    }

    public void LogAppRuntime()
    {
        if (db == null)
        {
            Debug.LogError("Database reference is not initialized.");
            return;
        }

        float appRuntime = Time.time - appStartTime;
        db.Child("Play " + playCount).Child("TotalRunTime").SetValueAsync(appRuntime);
    }

    public bool IsPlotTimerRunning(string plotType)
    {
        return plotStartTimes.ContainsKey(plotType);
    }

    void OnApplicationQuit()
    {
        foreach (var plot in plotStartTimes.Keys)
        {
            StopPlotTimer(plot);
        }

        LogAppRuntime(); // Log the application runtime when quitting
    }

    public void LogAnswer(string plotType, string question, string answer, float duration, List<string> dataPanels)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        int currentPlotCount = plotCounts.ContainsKey(plotType) ? plotCounts[plotType] : 1;

        db.Child("Play " + playCount).Child(plotType).Child("Plot " + currentPlotCount).Child("Questions").Child(question).Child("Answer").SetValueAsync(answer);
        db.Child("Play " + playCount).Child(plotType).Child("Plot " + currentPlotCount).Child("Questions").Child(question).Child("TimeTakenToAnswer").SetValueAsync(duration);

        // Log data panels information
        foreach (string panel in dataPanels)
        {
            db.Child("Play " + playCount).Child(plotType).Child("Plot " + currentPlotCount).Child("Questions").Child(question).Child("DataPanels").Push().SetValueAsync(panel);
        }
    }

    public void LogAnswerPanelDuration(string plotType, float duration)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        int currentPlotCount = plotCounts.ContainsKey(plotType) ? plotCounts[plotType] : 1;

        db.Child("Play " + playCount).Child(plotType).Child("Plot " + currentPlotCount).Child("TimeTakenToAnswer").Child(timestamp).SetValueAsync(duration); // Change to TimeTakenToAnswer
    }

    public void LogUIDuration(string plotType, float duration, string csvName)
    {
        if (db == null)
        {
            Debug.LogError("Database reference is not initialized.");
            return;
        }

        int logIndex = plotCounts.ContainsKey(plotType) ? plotCounts[plotType] : 1;
        db.Child("Play " + playCount).Child(plotType).Child("Plot " + logIndex).Child("UI Logs").Child("PlottingDuration").SetValueAsync(duration);
        db.Child("Play " + playCount).Child(plotType).Child("Plot " + logIndex).Child("UI Logs").Child("CSVChosen").SetValueAsync(csvName);
    }

#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            LogAppRuntime();
        }
    }
#endif
}

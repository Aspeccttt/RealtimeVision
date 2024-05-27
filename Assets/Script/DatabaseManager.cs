#region Unity Imports
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

public class DatabaseManager : MonoBehaviour
{
    #region User Data Variables
    private Dictionary<string, float> plotStartTimes = new Dictionary<string, float>();
    private Dictionary<string, string> currentCSVFiles = new Dictionary<string, string>();
    private string activePlotType;
    private List<string> clickedDatapoints = new List<string>();
    private float appStartTime;
    #endregion

    #region DB Variables
    private int plotCount = 0;
    private Dictionary<string, int> plotCounts = new Dictionary<string, int>();
    public int playCount = 0;
    private int logCount;
    private Dictionary<string, int> logsCounter = new Dictionary<string, int>();
    private DatabaseReference db;
    #endregion

    #region Unity Methods
    /// <summary>
    /// Initialize the DatabaseManager.
    /// </summary>
    private void Start()
    {
        appStartTime = Time.time;

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

        db = FirebaseDatabase.DefaultInstance.RootReference;
        InitializePlayCount();
    }

    /// <summary>
    /// Log the application runtime on quit.
    /// </summary>
    private void OnApplicationQuit()
    {
        foreach (var plot in plotStartTimes.Keys)
        {
            StopPlotTimer(plot);
        }

        LogAppRuntime(); // Log the application runtime when quitting
    }
    #endregion

    #region Play Count
    /// <summary>
    /// Initialize the play count from the database.
    /// </summary>
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

    /// <summary>
    /// Start a new play session.
    /// </summary>
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
    #endregion

    #region Plot Timer
    /// <summary>
    /// Start the plot timer for a specific plot type.
    /// </summary>
    /// <param name="plotType">The type of plot.</param>
    /// <param name="columnInfo">Column information related to the plot.</param>
    public void StartPlotTimer(string plotType, string columnInfo)
    {
        if (db == null)
        {
            Debug.LogError("Database reference is not initialized.");
            return;
        }

        if (!plotStartTimes.ContainsKey(plotType))
        {
            plotStartTimes[plotType] = Time.time;
        }

        if (!plotCounts.ContainsKey(plotType))
        {
            plotCounts[plotType] = 1; // Initialize plot count to 1
        }
        else
        {
            plotCounts[plotType]++; // Increment plot count by 1
        }

        activePlotType = plotType;

        // Use a consistent key for the plot entry
        string plotKey = $"Plot {plotCounts[plotType]}";

        db.Child("Play " + playCount).Child(plotType).Child(plotKey).Child("Column Info").SetValueAsync(columnInfo);
    }

    /// <summary>
    /// Stop the plot timer for a specific plot type.
    /// </summary>
    /// <param name="plotType">The type of plot.</param>
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
        }
    }

    /// <summary>
    /// Check if a plot timer is running for a specific plot type.
    /// </summary>
    /// <param name="plotType">The type of plot.</param>
    /// <returns>True if the plot timer is running, false otherwise.</returns>
    public bool IsPlotTimerRunning(string plotType)
    {
        return plotStartTimes.ContainsKey(plotType);
    }
    #endregion

    #region Data Logging
    /// <summary>
    /// Log a clicked datapoint.
    /// </summary>
    /// <param name="datapointName">The name of the clicked datapoint.</param>
    public void LogDatapointClick(string datapointName)
    {
        clickedDatapoints.Add(datapointName);
    }

    /// <summary>
    /// Log the application's runtime.
    /// </summary>
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

    /// <summary>
    /// Log the duration of an answer panel.
    /// </summary>
    /// <param name="plotType">The type of plot.</param>
    /// <param name="duration">The duration of the answer panel.</param>
    public void LogAnswerPanelDuration(string plotType, float duration)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        int currentPlotCount = plotCounts.ContainsKey(plotType) ? plotCounts[plotType] : 1;

        db.Child("Play " + playCount).Child(plotType).Child("Plot " + currentPlotCount).Child("TimeTakenToAnswer").Child(timestamp).SetValueAsync(duration); // Change to TimeTakenToAnswer
    }

    /// <summary>
    /// Log the duration of UI interaction.
    /// </summary>
    /// <param name="plotType">The type of plot.</param>
    /// <param name="duration">The duration of the interaction.</param>
    /// <param name="csvName">The name of the CSV file.</param>
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

    /// <summary>
    /// Log an answer to a question.
    /// </summary>
    /// <param name="plotType">The type of plot.</param>
    /// <param name="question">The question asked.</param>
    /// <param name="answer">The user's answer.</param>
    /// <param name="duration">The time taken to answer.</param>
    /// <param name="dataPanels">The data panels used.</param>
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
    #endregion

    #region Plot Count Management
    /// <summary>
    /// Get the plot count for a specific plot type.
    /// </summary>
    /// <param name="plotType">The type of plot.</param>
    /// <returns>The count of plots.</returns>
    public int GetPlotCount(string plotType)
    {
        if (plotCounts.ContainsKey(plotType))
        {
            return plotCounts[plotType];
        }
        return 0;
    }

    /// <summary>
    /// Set the plot count for a specific plot type.
    /// </summary>
    /// <param name="plotType">The type of plot.</param>
    /// <param name="count">The count to set.</param>
    public void SetPlotCount(string plotType, int count)
    {
        plotCounts[plotType] = count;
    }
    #endregion

#if UNITY_EDITOR
    /// <summary>
    /// Log application runtime on play mode state change.
    /// </summary>
    /// <param name="state">The play mode state change.</param>
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            LogAppRuntime();
        }
    }
#endif
}
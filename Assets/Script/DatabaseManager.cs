#if UNITY_EDITOR
using UnityEditor;
#endif
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{

    #region User Data Variables
    private string userId;
    private Dictionary<string, float> plotStartTimes = new Dictionary<string, float>();
    private Dictionary<string, string> currentCSVFiles = new Dictionary<string, string>();
    private Dictionary<string, int> plotCounts = new Dictionary<string, int>();
    private string activePlotType;
    private List<string> clickedDatapoints = new List<string>();

    private float appStartTime; 
    #endregion

    #region DB Variables
    private DatabaseReference db;
    #endregion

    void Start()
    {
        appStartTime = Time.time;

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

        db = FirebaseDatabase.DefaultInstance.RootReference;
        createUser();
    }

    private void createUser()
    {
        userId = SystemInfo.deviceUniqueIdentifier;
        Dictionary<string, string> userInfo = new Dictionary<string, string>
        {
            { "userId", userId }
        };

        db.Child("UserInfo").Child(userId).SetValueAsync(userInfo);
    }

    public void StartPlotTimer(string plotType, string csvName)
    {
        // Stop the timer for the currently active plot
        if (!string.IsNullOrEmpty(activePlotType))
        {
            StopPlotTimer(activePlotType);
        }

        // Start the new plot timer
        plotStartTimes[plotType] = Time.time;
        currentCSVFiles[plotType] = csvName;

        db.Child("UserInfo").Child(userId).Child(plotType).Child("CSVUsed").SetValueAsync(csvName);

        // Increment the plot count
        if (!plotCounts.ContainsKey(plotType))
        {
            plotCounts[plotType] = 0;
        }
        plotCounts[plotType]++;
        db.Child("UserInfo").Child(userId).Child(plotType).Child("PlotCount").SetValueAsync(plotCounts[plotType]);

        // Update the active plot type
        activePlotType = plotType;

        // Reset the clicked datapoints list
        clickedDatapoints.Clear();
    }

    public void StopPlotTimer(string plotType)
    {
        if (!plotStartTimes.ContainsKey(plotType))
        {
            Debug.LogError($"Plot timer for {plotType} not started.");
            return;
        }

        float duration = Time.time - plotStartTimes[plotType];
        db.Child("UserInfo").Child(userId).Child(plotType).Child("TimeSpent").SetValueAsync(duration);

        // Append the clicked datapoints to Firebase
        db.Child("UserInfo").Child(userId).Child(plotType).Child("Datapoints").SetValueAsync(clickedDatapoints);

        plotStartTimes.Remove(plotType);

        if (activePlotType == plotType)
        {
            activePlotType = null;
        }
    }

    public void LogDatapointClick(string datapointName)
    {
        clickedDatapoints.Add(datapointName);
    }

    public void LogAppRuntime()
    {
        float appRuntime = Time.time - appStartTime;
        db.Child("UserInfo").Child(userId).Child("AppRuntime").SetValueAsync(appRuntime);
    }

    void OnApplicationQuit()
    {
        foreach (var plot in plotStartTimes.Keys)
        {
            StopPlotTimer(plot);
        }

        LogAppRuntime(); // Log the application runtime when quitting
    }

    public void LogAnswer(string question, string answer)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        db.Child("UserAnswers").Child(userId).Child(timestamp).Child("Question").SetValueAsync(question);
        db.Child("UserAnswers").Child(userId).Child(timestamp).Child("Answer").SetValueAsync(answer);
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

#region Unity Imports
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#endregion


public class GameManager : MonoBehaviour
{
    #region Global Variables
    public bool IsCSVUploaded = false;
    public List<Dictionary<string, object>> csvData;
    #endregion

    #region Singleton
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize the CSV data list
        csvData = new List<Dictionary<string, object>>();
    }
    #endregion

    // Method to store CSV data
    public void StoreCSVData(List<Dictionary<string, object>> data)
    {
        csvData = data;
        IsCSVUploaded = true;
    }

    // Method to retrieve CSV data
    public List<Dictionary<string, object>> GetCSVData()
    {
        return csvData;
    }

}

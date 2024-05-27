#region Unity Imports
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEditor;
using System.Collections.Generic;
#endregion

public class CSVUploader : MonoBehaviour
{
    #region Variables
    public string path;
    public CSVPlotter csvPlotter;
    private DatabaseManager dbManager;
    #endregion

    #region Unity Methods
    private void Start()
    {
        dbManager = GameManager.Instance.GetComponent<DatabaseManager>();
    }
    #endregion

    #region File Handling
    /// <summary>
    /// Open the file explorer to select a CSV file.
    /// </summary>
    public void OpenFileExplorer()
    {
        path = EditorUtility.OpenFilePanel("Load csv file", "", "csv");
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(FetchCSV());
        }
        else
        {
            Debug.Log("CSV loading cancelled or failed.");
            GameManager.Instance.ShowNotification("Cancelled Upload...");
        }
    }
    #endregion

    #region CSV Fetching
    /// <summary>
    /// Coroutine to fetch the CSV file from the selected path.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator FetchCSV()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("file:///" + path))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                string csvContent = www.downloadHandler.text;
                List<Dictionary<string, object>> parsedData = CSVReader.ReadFromString(csvContent);

                if (parsedData != null && parsedData.Count > 0)
                {
                    if (csvPlotter != null)
                    {
                        csvPlotter.SetData(parsedData, path);

                        GameManager.Instance.GetComponent<MenuManager>().CSVLoaded();
                    }
                    else
                    {
                        Debug.Log("CSVPlotter reference not set in CSVUploader.");
                        GameManager.Instance.ShowNotification("Error 303");
                    }
                }
                else
                {
                    Debug.LogError("No data parsed from CSV.");
                    GameManager.Instance.ShowNotification("No data parsed from CSV.");
                }
            }
        }
    }
    #endregion
}
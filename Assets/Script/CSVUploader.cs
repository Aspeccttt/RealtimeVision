using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEditor;
using System.Collections.Generic;

public class CSVUploader : MonoBehaviour
{
    public string path;
    public CSVPlotter csvPlotter;
    private DatabaseManager dbManager;

    void Start()
    {
        dbManager = GameManager.Instance.GetComponent<DatabaseManager>();
    }

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
        }
    }

    IEnumerator FetchCSV()
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
                        Debug.LogError("CSVPlotter reference not set in CSVUploader.");
                    }
                }
                else
                {
                    Debug.LogError("No data parsed from CSV.");
                }
            }
        }
    }
}

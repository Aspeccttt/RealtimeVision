#region Unity Imports
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
#endregion


public class CSVUploader : MonoBehaviour
{
    #region Global Variables
    public string path;
    public RawImage rawImage; // Assuming this is for preview or other purposes

    private CSVReader reader; // Ensure this matches the correct class name handling CSV reading
    #endregion

    #region Unity Lifecycle
    #endregion

    #region Handler
    public void OpenFileExplorer()
    {
        path = EditorUtility.OpenFilePanel("Load csv file", "", "csv");
        StartCoroutine(FetchCSV());
    }

    IEnumerator FetchCSV()
    {
        UnityWebRequest www = UnityWebRequest.Get("file:///" + path);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("CSV file loaded from path: " + path);
            GameManager.Instance.IsCSVUploaded = true; // Make sure GameManager and its properties are correctly set up
            ReaderCall();
        }
    }

    public void ReaderCall()
    {
        reader = GameObject.Find("GameManager").GetComponent<CSVReader>(); // Make sure this matches your actual GameManager and reader setup

        if (reader != null)
        {
            reader.ReadCSVFileFromPath(path); // You should modify CSVReader to handle path directly
        }
        else
        {
            Debug.LogError("CSVReader component not found in GameManager.");
        }
    }

    public string ReturnLocalURL()
    {
        return path;
    }
}
    #endregion

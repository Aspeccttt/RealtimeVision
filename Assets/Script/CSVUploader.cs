using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CSVUploader : MonoBehaviour
{
    string path;
    public RawImage rawImage;

    CSVreader reader;

    public void OpenFileExplorer()
    {
        path = EditorUtility.OpenFilePanel("Show all images (.csv)", "", "csv");
        StartCoroutine(FetchCSV());
    }

    IEnumerator FetchCSV()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + path);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }else
        {
            Debug.Log(path.ToString());
            ReaderCall();
            StopAllCoroutines();
        }
    }

    public void ReaderCall()
    {
        reader = GameObject.Find("GameManager").GetComponent<CSVreader>();

        reader.ReadCSVFile();
    }

    public string returnLocalURL()
    {
        return path;
    }
}

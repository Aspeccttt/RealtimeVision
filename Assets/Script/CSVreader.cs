using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class CSVreader : MonoBehaviour
{
    private List<string> column1Data = new List<string>();
    private List<string> column2Data = new List<string>();
    private List<string> column3Data = new List<string>();

    [SerializeField]
    private TextMeshProUGUI xUI;
    [SerializeField]
    private TextMeshProUGUI yUI;
    [SerializeField]
    private TextMeshProUGUI zUI;

    [SerializeField]
    private TextAsset csvFile;
    public string localURL;

    private string column1Title, column2Title, column3Title;

    [SerializeField]
    CSVUploader uploader;

    void Start()
    {
        uploader = GameObject.Find("GameManager").GetComponent<CSVUploader>();
    }

    public void ReadCSVFile()
    {
        localURL = uploader.returnLocalURL();
        Debug.Log("BEFORE INITALIZATION: " + localURL.ToString());
        string[] lines = File.ReadAllLines(Path.Combine(Application.dataPath, localURL.ToString()));

        // Parse the first line to get column titles
        string[] titles = lines[0].Split(',');
        //Saving the titles into variables.
        column1Title = titles[0];
        column2Title = titles[1];
        column3Title = titles[2];

        //Setting the titles.
        UpdateXText("X: " + column1Title);
        UpdateYText("Y: " + column2Title); 
        UpdateZText("Z: " + column3Title);

        // Start from the second line to read data
        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Split(',');

            // Assuming each row has exactly 3 columns
            if (row.Length >= 3)
            {
                column1Data.Add(row[0]);
                column2Data.Add(row[1]);
                column3Data.Add(row[2]);
            }
        }
    }

    public void UpdateZText(string text)
    {
        if (zUI != null)
        {
            zUI.text = text;
        }
    }

    public void UpdateYText(string text)
    {
        if (yUI != null)
        {
            yUI.text = text;
        }
    }

    public void UpdateXText(string text)
    {
        if (xUI != null)
        {
            xUI.text = text;
        }
    }

    /// <summary>
    /// Used for debugging purposes.
    /// </summary>
    void OutputDataByColumns()
    {
        // Output for Column 1
        Debug.Log(column1Title);
        foreach (string value in column1Data)
        {
            Debug.Log(value);
        }

        // Output for Column 2
        Debug.Log(column2Title);
        foreach (string value in column2Data)
        {
            Debug.Log(value);
        }

        // Output for Column 3
        Debug.Log(column3Title);
        foreach (string value in column3Data)
        {
            Debug.Log(value);
        }
    }
}

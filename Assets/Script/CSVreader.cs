#region Unity Imports
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
#endregion

public class CSVreader : MonoBehaviour
{
    #region Global Variables
    public GameObject spherePrefab;
    public Transform plotPointsParent;

    private List<string> column1Data = new List<string>();
    private List<string> column2Data = new List<string>();
    private List<string> column3Data = new List<string>();

    private float minX, maxX, minY, maxY, minZ, maxZ;

    [SerializeField]
    private GameObject floor;
    Vector3 floorSize;


    [SerializeField]
    public string localURL;

    private string column1Title, column2Title, column3Title;

    [SerializeField]
    CSVUploader uploader;

    public TMP_Dropdown xDropdown, yDropdown, zDropdown;
    public UnityEngine.UI.Button doneButton; // Reference to the "Done" button

    private List<List<string>> columnData = new List<List<string>>();
    private List<string>[] columnDataArrays; // Array of lists to hold column data
    private List<string> columnNames = new List<string>();
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        uploader = GameObject.Find("GameManager").GetComponent<CSVUploader>();
        doneButton.onClick.AddListener(OnDoneButtonClick);
        floorSize = floor.GetComponent<Renderer>().bounds.size;
    }
    #endregion

    #region CSV File Handling
    public void ReadCSVFile()
    {
        string filePath = uploader.returnLocalURL();

        // Check if the filePath is not empty or null
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("CSV file path is empty or not set.");
            return; // Exit the method if the file path is invalid
        }

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            GameManager.Instance.CSVLoaded();

            string[] headers = lines[0].Split(',');
            columnNames.Clear();
            foreach (string header in headers)
            {
                columnNames.Add(header.Trim());
            }

            columnData.Clear();
            for (int i = 0; i < headers.Length; i++)
            {
                columnData.Add(new List<string>());
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] entries = lines[i].Split(',');
                for (int j = 0; j < entries.Length && j < columnData.Count; j++)
                {
                    // Add data to corresponding column list
                    columnData[j].Add(entries[j].Trim());
                }
            }
            PopulateDropdowns();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading the CSV file: {e.Message}");
        }

        //Verification:
        Debug.Log("CSV Data Loaded:");
        for (int i = 0; i < columnNames.Count; i++)
        {
            Debug.Log($"{columnNames[i]}: {string.Join(", ", columnData[i])}");
        }
    }

    public void LogCurrentSelections()
    {
        string xSelection = xDropdown.options[xDropdown.value].text;
        string ySelection = yDropdown.options[yDropdown.value].text;
        string zSelection = zDropdown.options[zDropdown.value].text;

        Debug.Log($"Current Selections - X: {xSelection}, Y: {ySelection}, Z: {zSelection}");
    }
    #endregion

    #region Menu
    void PopulateDropdowns()
    {
        xDropdown.ClearOptions();
        yDropdown.ClearOptions();
        zDropdown.ClearOptions();

        xDropdown.AddOptions(columnNames);
        yDropdown.AddOptions(columnNames);
        zDropdown.AddOptions(columnNames);

        doneButton.interactable = true;
    }

    public void OnDoneButtonClick()
    {
        GameManager.Instance.UpdateXText(xDropdown.options[xDropdown.value].text);
        GameManager.Instance.UpdateYText(yDropdown.options[yDropdown.value].text);
        GameManager.Instance.UpdateZText(zDropdown.options[zDropdown.value].text);
        LogCurrentSelections();
        PrintSelectedColumnData();
        InstantiateSelectedDataPoints();
        GameManager.Instance.ToggleMenu();
    }

    public void PrintSelectedColumnData()
    {
        int xIndex = xDropdown.value;
        int yIndex = yDropdown.value;
        int zIndex = zDropdown.value;

        Debug.Log($"Selected Data - X ({columnNames[xIndex]}): {string.Join(", ", columnData[xIndex])}");
        Debug.Log($"Selected Data - Y ({columnNames[yIndex]}): {string.Join(", ", columnData[yIndex])}");
        Debug.Log($"Selected Data - Z ({columnNames[zIndex]}): {string.Join(", ", columnData[zIndex])}");
    }
    #endregion

    #region Data Points Handler
    private void InstantiateSelectedDataPoints()
    {
        // Clear previous data points from the plot
        foreach (Transform child in plotPointsParent)
        {
            Destroy(child.gameObject);
        }

        // Convert selected column data from strings to floats
        List<float> xValues = ConvertToFloatList(columnData[xDropdown.value]);
        List<float> yValues = ConvertToFloatList(columnData[yDropdown.value]);
        List<float> zValues = ConvertToFloatList(columnData[zDropdown.value]);

        // Assuming the floor defines the plot area, calculate the plot bounds
        Vector3 bottomLeft = floor.transform.position - floorSize / 2;
        float plotPadding = 1.0f; // Modify this as needed

        // Instantiate data points
        for (int i = 0; i < xValues.Count; i++)
        {
            // Normalize positions based on plot size and padding
            Vector3 position = new Vector3(
                NormalizeValue(xValues[i], minX, maxX, bottomLeft.x + plotPadding, bottomLeft.x + floorSize.x - plotPadding),
                NormalizeValue(yValues[i], minY, maxY, bottomLeft.y + plotPadding, bottomLeft.y + floorSize.y - plotPadding),
                NormalizeValue(zValues[i], minZ, maxZ, bottomLeft.z + plotPadding, bottomLeft.z + floorSize.z - plotPadding)
            );

            // Instantiate the prefab at the calculated position
            Instantiate(spherePrefab, position, Quaternion.identity, plotPointsParent);
        }
    }

    private List<float> ConvertToFloatList(List<string> stringList)
    {
        List<float> floatList = new List<float>();
        foreach (string str in stringList)
        {
            if (float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
            {
                floatList.Add(value);
            }
            else
            {
                Debug.LogError("Failed to parse float from string: " + str);
            }
        }
        return floatList;
    }

    private float NormalizeValue(float value, float min, float max, float newMin, float newMax)
    {
        // Avoid division by zero; return newMin by default
        return (max - min == 0) ? newMin : ((value - min) / (max - min) * (newMax - newMin) + newMin);
    }

    #endregion

    #region Debugging Purposes
    /// <summary>
    /// Used for debugging purposes. Outputs each coloumn seperately.
    /// </summary>
    void OutputDataByColumns()
    {
        Debug.Log(column1Title);
        foreach (string value in column1Data)
        {
            Debug.Log(value);
        }

        Debug.Log(column2Title);
        foreach (string value in column2Data)
        {
            Debug.Log(value);
        }

        Debug.Log(column3Title);
        foreach (string value in column3Data)
        {
            Debug.Log(value);
        }
    }
    #endregion
}

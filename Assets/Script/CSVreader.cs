using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using System.IO;

public class CSVReader : MonoBehaviour
{
    #region Global Variables
    public GameObject spherePrefab;

    private List<Dictionary<string, object>> pointList = new List<Dictionary<string, object>>();

    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    [SerializeField]
    public string localURL;

    [SerializeField]
    private CSVUploader uploader;

    public TMP_Dropdown xDropdown, yDropdown, zDropdown;
    public UnityEngine.UI.Button doneButton;

    private List<string> columnNames = new List<string>();
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        uploader = GameObject.Find("GameManager").GetComponent<CSVUploader>();
        doneButton.onClick.AddListener(OnDoneButtonClick);
    }
    #endregion

    #region CSV File Handling

    public void ReadCSVFileFromPath(string filePath)
    {
        pointList.Clear(); // Clear existing data
        columnNames.Clear(); // Clear existing column names

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length > 1) // Ensure there's more than just the header
            {
                var headers = Regex.Split(lines[0], SPLIT_RE); // Extract column names
                columnNames.AddRange(headers); // Add headers to column names list

                for (int i = 1; i < lines.Length; i++)
                {
                    var values = Regex.Split(lines[i], SPLIT_RE);
                    if (values.Length == 0 || values[0] == "") continue;

                    var entry = new Dictionary<string, object>();
                    for (int j = 0; j < headers.Length && j < values.Length; j++)
                    {
                        string value = values[j].TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                        object finalValue = value; // Default to string
                        if (int.TryParse(value, out int n))
                        {
                            finalValue = n;
                        }
                        else if (float.TryParse(value, out float f))
                        {
                            finalValue = f;
                        }
                        entry[headers[j]] = finalValue;
                    }
                    pointList.Add(entry);
                }

                // Populate dropdowns after reading the CSV
                PopulateDropdowns(columnNames);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading the CSV file: {e.Message}");
        }
    }

    // Accessor for the parsed data
    public List<Dictionary<string, object>> GetPointList()
    {
        return pointList;
    }

    // Method to populate dropdowns with column names
    public void PopulateDropdowns(List<string> columnNames)
    {
        xDropdown.ClearOptions();
        yDropdown.ClearOptions();
        zDropdown.ClearOptions();

        xDropdown.AddOptions(columnNames);
        yDropdown.AddOptions(columnNames);
        zDropdown.AddOptions(columnNames);

        // Add a debug log to confirm this method is called
        Debug.Log("Populating Dropdowns with Column Names");
    }

    public void OnDropdownValueChange() // Make sure this is hooked to each dropdown's OnValueChanged event
    {
        string xColumn = xDropdown.options[xDropdown.value].text;
        string yColumn = yDropdown.options[yDropdown.value].text;
        string zColumn = zDropdown.options[zDropdown.value].text;

        Plotter plotter = FindObjectOfType<Plotter>(); // Find the Plotter script in the scene
        if (plotter != null)
        {
            plotter.UpdateColumnNames(xColumn, yColumn, zColumn); // Update Plotter with new column names
        }
    }

    public void OnDoneButtonClick()
    {
        // Implement the actions that should be performed when the done button is clicked
        // This could include logging selections, updating UI text, and instantiating data points based on selected columns
        //InstantiateSelectedDataPoints();
    }

    #endregion
}
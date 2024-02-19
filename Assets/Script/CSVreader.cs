using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;

public class CSVreader : MonoBehaviour
{
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
    private TextAsset csvFile;
    public string localURL;

    private string column1Title, column2Title, column3Title;

    [SerializeField]
    CSVUploader uploader;

    public TMP_Dropdown xDropdown, yDropdown, zDropdown;
    public UnityEngine.UI.Button doneButton; // Reference to the "Done" button

    private List<List<string>> columnData = new List<List<string>>();
    private List<string>[] columnDataArrays; // Array of lists to hold column data
    private List<string> columnNames = new List<string>();

    void Start()
    {
        uploader = GameObject.Find("GameManager").GetComponent<CSVUploader>();
        doneButton.onClick.AddListener(OnDoneButtonClick);
        floorSize = floor.GetComponent<Renderer>().bounds.size;
    }

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
    }

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
        // Update labels based on selected dropdown values
        GameManager.Instance.UpdateXText(xDropdown.options[xDropdown.value].text);
        GameManager.Instance.UpdateYText(yDropdown.options[yDropdown.value].text);
        GameManager.Instance.UpdateZText(zDropdown.options[zDropdown.value].text);

        // Close the menu
        GameManager.Instance.ToggleMenu();
    }

    private void InstantiateDataPoints()
    {
        Vector3 floorSize = floor.GetComponent<Renderer>().bounds.size;
        Vector3 floorPosition = floor.transform.position;

        // Calculate bottom left position
        Vector3 bottomLeft = new Vector3(
            floorPosition.x - floorSize.x / 2,
            floorPosition.y, // Adjust this based on how you're using the Y-axis
            floorPosition.z - floorSize.z / 2
        );

        float plotPadding = 1.0f; // Add some padding to ensure plots don't touch the edges directly

        // Calculate the new normalized range taking padding into account
        float normalizedMinX = plotPadding;
        float normalizedMaxX = floorSize.x - plotPadding;
        float normalizedMinZ = plotPadding;
        float normalizedMaxZ = floorSize.z - plotPadding;

        for (int i = 0; i < column3Data.Count; i++)
        {
            float intensity = float.Parse(column3Data[i], CultureInfo.InvariantCulture);
            minZ = Mathf.Min(minZ, intensity);
            maxZ = Mathf.Max(maxZ, intensity);
        }

        for (int i = 0; i < column3Data.Count; i++)
        {
            float intensity = float.Parse(column3Data[i], CultureInfo.InvariantCulture);

            // Normalize and scale X and Z within the floor bounds, starting from 0
            float xPosition = NormalizeValue(i, 0, column3Data.Count - 1, 0, floorSize.x);
            float zPosition = NormalizeValue(intensity, minZ, maxZ, 0, floorSize.z); // Assuming intensity determines Z-axis position

            // Apply the bottom left offset to each plot position
            Vector3 plotPosition = new Vector3(xPosition, 0, zPosition) + bottomLeft; // Adjust Y-axis as needed

            // Instantiate the prefab at the calculated position
            Instantiate(spherePrefab, plotPosition, Quaternion.identity, plotPointsParent);
        }
    }
        
    private float NormalizeValue(float value, float min, float max, float normalizedMin, float normalizedMax)
    {
        if (max - min == 0) return normalizedMin; // Return a default value if min and max are equal
        return (value - min) / (max - min) * (normalizedMax - normalizedMin) + normalizedMin;
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

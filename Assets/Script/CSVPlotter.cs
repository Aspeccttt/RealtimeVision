using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;

public class CSVPlotter : MonoBehaviour
{
    public string columnXName;
    public string columnYName;
    public string columnZName;

    public float plotScale = 10; 

    public GameObject PointPrefab;
    public GameObject PointHolder; 

    private List<Dictionary<string, object>> pointList; // Holds data from CSV

    public TMP_Dropdown dropdownX;
    public TMP_Dropdown dropdownY;
    public TMP_Dropdown dropdownZ;

    public GameObject floor;
    public float heightOffset = 0.1f;  // Points will spawn this much above the floor

    public void SetData(List<Dictionary<string, object>> data)
    {
        pointList = data; 
        PopulateDropdowns(); 
        Debug.Log("Data has been initialized in the dropdown.");
    }

    private void PopulateDropdowns()
    {
        List<string> columnList = new List<string>(pointList[0].Keys);

        dropdownX.ClearOptions();
        dropdownY.ClearOptions();
        dropdownZ.ClearOptions();

        dropdownX.AddOptions(columnList);
        dropdownY.AddOptions(columnList);
        dropdownZ.AddOptions(columnList);
    }

    public void PlotData()
    {
        foreach (Transform child in PointHolder.transform)
        {
            Destroy(child.gameObject);
        }

        columnXName = dropdownX.options[dropdownX.value].text;
        columnYName = dropdownY.options[dropdownY.value].text;
        columnZName = dropdownZ.options[dropdownZ.value].text;

        // Find max and min values for normalization
        float xMax = FindMaxValue(columnXName), xMin = FindMinValue(columnXName);
        float yMax = FindMaxValue(columnYName), yMin = FindMinValue(columnYName);
        float zMax = FindMaxValue(columnZName), zMin = FindMinValue(columnZName);

        Vector3 floorSize = floor.GetComponent<Renderer>().bounds.size;
        Vector3 floorPosition = floor.transform.position;

        foreach (var point in pointList)
        {
            // Normalize point data
            float x = (Convert.ToSingle(point[columnXName]) - xMin) / (xMax - xMin);
            float y = (Convert.ToSingle(point[columnYName]) - yMin) / (yMax - yMin);
            float z = (Convert.ToSingle(point[columnZName]) - zMin) / (zMax - zMin);

            // Scale and position points based on floor size and actual data values
            Vector3 plotPosition = new Vector3(
                floorPosition.x + (x * floorSize.x) - (floorSize.x / 2),
                floorPosition.y + (y * plotScale) + heightOffset,
                floorPosition.z + (z * floorSize.z) - (floorSize.z / 2)
            );

            // Instantiate and position the data point
            GameObject dataPoint = Instantiate(PointPrefab, plotPosition, Quaternion.identity);
            dataPoint.transform.parent = PointHolder.transform;

            // Naming and coloring the data point
            dataPoint.name = $"{point[columnXName]} {point[columnYName]} {point[columnZName]}";
            dataPoint.GetComponent<Renderer>().material.color = new Color(x, y, z, 1.0f);
        }

        Debug.Log("Data has been plotted successfully.");
    }

    private float FindMaxValue(string columnName)
    {
        float maxValue = Convert.ToSingle(pointList[0][columnName]);
        foreach (var point in pointList)
            maxValue = Mathf.Max(maxValue, Convert.ToSingle(point[columnName]));
        return maxValue;
    }

    private float FindMinValue(string columnName)
    {
        float minValue = Convert.ToSingle(pointList[0][columnName]);
        foreach (var point in pointList)
            minValue = Mathf.Min(minValue, Convert.ToSingle(point[columnName]));
        return minValue;
    }
}
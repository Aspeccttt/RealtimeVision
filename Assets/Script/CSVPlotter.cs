using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;

public class CSVPlotter : MonoBehaviour
{
    // These should match the dropdown selections
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

    public void SetData(List<Dictionary<string, object>> data)
    {
        pointList = data; 
        PopulateDropdowns(); 
        Debug.Log("Data has been set.");
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

        float xMax = FindMaxValue(columnXName), xMin = FindMinValue(columnXName);
        float yMax = FindMaxValue(columnYName), yMin = FindMinValue(columnYName);
        float zMax = FindMaxValue(columnZName), zMin = FindMinValue(columnZName);

        foreach (var point in pointList)
        {
            // Normalize the point data
            float x = (Convert.ToSingle(point[columnXName]) - xMin) / (xMax - xMin);
            float y = (Convert.ToSingle(point[columnYName]) - yMin) / (yMax - yMin);
            float z = (Convert.ToSingle(point[columnZName]) - zMin) / (zMax - zMin);

            // Instantiate and position the data point
            GameObject dataPoint = Instantiate(PointPrefab, new Vector3(x, y, z) * plotScale, Quaternion.identity);
            dataPoint.transform.parent = PointHolder.transform;

            // Naming and coloring the data point
            dataPoint.name = $"{point[columnXName]} {point[columnYName]} {point[columnZName]}";
            dataPoint.GetComponent<Renderer>().material.color = new Color(x, y, z, 1.0f);
        }

        Debug.Log("Data plotting complete.");
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
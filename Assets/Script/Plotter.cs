using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Plotter : MonoBehaviour
{
    // References for plotting
    public GameObject PointPrefab;
    public GameObject PointHolder;
    public float plotScale = 10;

    // Data
    private List<Dictionary<string, object>> pointList = new List<Dictionary<string, object>>();

    // Column names
    private string xName;
    private string yName;
    private string zName;

    // CSV Reader reference
    private CSVReader csvReader;

    void Start()
    {
        // Find the CSVReader component in the scene
        csvReader = FindObjectOfType<CSVReader>();
        if (csvReader == null)
        {
            Debug.LogError("CSVReader component not found in the scene.");
        }
    }

    public void UpdateColumnNames(string xColumn, string yColumn, string zColumn)
    {
        xName = xColumn;
        yName = yColumn;
        zName = zColumn;

        // Check if pointList is not null and contains data before attempting to plot
        if (pointList != null && pointList.Count > 0)
        {
            PlotData(); // Only plot data if it exists
        }
    }

    public void SetData(List<Dictionary<string, object>> newData)
    {
        pointList = newData; // Assign the new data

        // Optionally, immediately plot the data upon setting it
        if (pointList != null && pointList.Count > 0)
        {
            PlotData();
        }
    }

    public void PlotData()
    {
        if (pointList == null || pointList.Count == 0)
        {
            Debug.LogError("Point list is empty or not initialized.");
            return; // Exit the method if there's no data to plot
        }

        // Clear existing points
        foreach (Transform child in PointHolder.transform)
        {
            Destroy(child.gameObject);
        }

        // Find max and min for each axis for normalization
        float xMax = FindMaxValue(xName);
        float yMax = FindMaxValue(yName);
        float zMax = FindMaxValue(zName);
        float xMin = FindMinValue(xName);
        float yMin = FindMinValue(yName);
        float zMin = FindMinValue(zName);

        // Plot each point based on normalized data
        for (var i = 0; i < pointList.Count; i++)
        {
            // Normalize the data
            float x = (Convert.ToSingle(pointList[i][xName]) - xMin) / (xMax - xMin);
            float y = (Convert.ToSingle(pointList[i][yName]) - yMin) / (yMax - yMin);
            float z = (Convert.ToSingle(pointList[i][zName]) - zMin) / (zMax - zMin);

            // Instantiate data points
            GameObject dataPoint = Instantiate(PointPrefab, new Vector3(x, y, z) * plotScale, Quaternion.identity, PointHolder.transform);
            dataPoint.transform.name = pointList[i][xName] + " " + pointList[i][yName] + " " + pointList[i][zName];
            dataPoint.GetComponent<Renderer>().material.color = new Color(x, y, z, 1.0f);
        }
    }

    private float FindMaxValue(string columnName)
    {
        if (pointList.Count == 0)
        {
            Debug.LogError("Trying to find max value but point list is empty.");
            return 0; // Or return a default value appropriate for your context
        }

        float maxValue = float.MinValue;
        foreach (var point in pointList)
        {
            if (point.ContainsKey(columnName)) // Check if the key exists to avoid KeyNotFoundException
            {
                float value = Convert.ToSingle(point[columnName]);
                if (value > maxValue)
                {
                    maxValue = value;
                }
            }
        }
        return maxValue;
    }

    private float FindMinValue(string columnName)
    {

        float minValue = Convert.ToSingle(pointList[0][columnName]);

        //Loop through Dictionary, overwrite existing minValue if new value is smaller
        for (var i = 0; i < pointList.Count; i++)
        {
            if (Convert.ToSingle(pointList[i][columnName]) < minValue)
                minValue = Convert.ToSingle(pointList[i][columnName]);
        }

        return minValue;
    }
}

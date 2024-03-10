using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;

public class LineGraphPlotter : MonoBehaviour
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

    private float[] xPlotPoints;
    private float[] yPlotPoints;
    private float[] zPlotPoints;

    public TextMeshProUGUI[] xPlotTexts;
    public TextMeshProUGUI[] yPlotTexts;
    public TextMeshProUGUI[] zPlotTexts;

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
        // Find max and min values for normalization
        float xMax = FindMaxValue(columnXName), xMin = FindMinValue(columnXName);
        float yMax = FindMaxValue(columnYName), yMin = FindMinValue(columnYName);
        float zMax = FindMaxValue(columnZName), zMin = FindMinValue(columnZName);

        // Prepare LineRenderer
        LineRenderer lineRenderer = PointHolder.GetComponent<LineRenderer>();
        lineRenderer.positionCount = pointList.Count; // Set the number of points the line will have

        Vector3 floorSize = floor.GetComponent<Renderer>().bounds.size;
        Vector3 floorPosition = floor.transform.position;
        int pointIndex = 0; // Index to track position in LineRenderer

        foreach (var point in pointList)
        {
            // Normalize and position points
            float x = (Convert.ToSingle(point[columnXName]) - xMin) / (xMax - xMin);
            float y = (Convert.ToSingle(point[columnYName]) - yMin) / (yMax - yMin);
            float z = (Convert.ToSingle(point[columnZName]) - zMin) / (zMax - zMin);

            Vector3 plotPosition = new Vector3(
                floorPosition.x + (x * floorSize.x) - (floorSize.x / 2),
                floorPosition.y + (y * plotScale) + heightOffset,
                floorPosition.z + (z * floorSize.z) - (floorSize.z / 2)
            );

            // Set this position in the LineRenderer
            lineRenderer.SetPosition(pointIndex, plotPosition);
            pointIndex++;
        }

        Debug.Log("Line graph has been plotted successfully.");
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

    public float[] CalculatePlotPoints(string columnName)
    {
        float maxVal = FindMaxValue(columnName);
        float minVal = FindMinValue(columnName);
        float[] plotPoints = new float[10]; // Array to hold your plot points

        float interval = (maxVal - minVal) / 9; // Divide the range into 9 intervals (for 10 points)

        for (int i = 0; i < 10; i++)
        {
            plotPoints[i] = minVal + interval * i; // Calculate each plot point
        }

        return plotPoints; // Return the array of plot points
    }

    public void CalculateAllPlotPoints()
    {
        // Ensure the data is set and columns are selected
        if (pointList != null && pointList.Count > 0)
        {
            // Calculate plot points for each axis based on current dropdown selections
            xPlotPoints = CalculatePlotPoints(dropdownX.options[dropdownX.value].text);
            yPlotPoints = CalculatePlotPoints(dropdownY.options[dropdownY.value].text);
            zPlotPoints = CalculatePlotPoints(dropdownZ.options[dropdownZ.value].text);

            // Log plot points for debugging (Optional)
            DebugLogPlotPoints(xPlotPoints, "X");
            DebugLogPlotPoints(yPlotPoints, "Y");
            DebugLogPlotPoints(zPlotPoints, "Z");
        }
    }

    private void DebugLogPlotPoints(float[] plotPoints, string axisName)
    {
        for (int i = 0; i < plotPoints.Length; i++)
        {
            Debug.Log(axisName + " Plot Point " + (i + 1) + ": " + plotPoints[i]);
        }
    }

    public void UpdatePlotPointTexts()
    {
        // Update X-axis plot points text
        for (int i = 0; i < xPlotPoints.Length && i < xPlotTexts.Length; i++)
        {
            xPlotTexts[i].text = xPlotPoints[i].ToString("F2"); // "F2" formats to two decimal places
        }

        // Update Y-axis plot points text
        for (int i = 0; i < yPlotPoints.Length && i < yPlotTexts.Length; i++)
        {
            yPlotTexts[i].text = yPlotPoints[i].ToString("F2");
        }

        // Update Z-axis plot points text
        for (int i = 0; i < zPlotPoints.Length && i < zPlotTexts.Length; i++)
        {
            zPlotTexts[i].text = zPlotPoints[i].ToString("F2");
        }
    }
}
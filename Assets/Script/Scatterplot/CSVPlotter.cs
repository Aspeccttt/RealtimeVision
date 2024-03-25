using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;

public class CSVPlotter : MonoBehaviour
{
    #region Global Variables

    public string columnXName;
    public string columnYName;
    public string columnZName;

    public float plotScale = 10;
    public GameObject floor;
    public float heightOffset = 0.1f;  // Points will spawn this much above the floor

    public GameObject PointPrefab;
    public GameObject PointHolder; 

    public List<Dictionary<string, object>> pointList; // Holds data from CSV

    public TMP_Dropdown dropdownX;
    public TMP_Dropdown dropdownY;
    public TMP_Dropdown dropdownZ;

    private float[] xPlotPoints;
    private float[] yPlotPoints;
    private float[] zPlotPoints;

    public TextMeshProUGUI[] xPlotTexts;
    public TextMeshProUGUI[] yPlotTexts;
    public TextMeshProUGUI[] zPlotTexts;

    //LineGraph
    public TMP_Dropdown lineGraphSelectedColumn;
    public TextMeshProUGUI feedbackText;
    public List<string> selectedColumns = new List<string>(); // This now becomes global
    #endregion
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
        lineGraphSelectedColumn.ClearOptions();

        dropdownX.AddOptions(columnList);
        dropdownY.AddOptions(columnList);
        dropdownZ.AddOptions(columnList);
        lineGraphSelectedColumn.AddOptions(columnList);
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

    public void CalculateLineGraphPoints()
    {
        // Start with extreme values and narrow them down based on actual data
        float globalMax = float.MinValue;
        float globalMin = float.MaxValue;

        // Go through all selected columns to find global min and max
        foreach (string column in selectedColumns)
        {
            float localMax = FindMaxValue(column);
            float localMin = FindMinValue(column);
            if (localMax > globalMax) globalMax = localMax;
            if (localMin < globalMin) globalMin = localMin;
        }

        // Calculate intervals for the labels based on the global min and max
        float interval = (globalMax - globalMin) / 9; // Divide the range into 9 intervals (for 10 points)

        // Update Y-axis labels
        for (int i = 0; i < yPlotTexts.Length; i++)
        {
            // Assuming yPlotTexts is an array of TextMeshProUGUI with length 10
            yPlotTexts[i].text = (globalMin + interval * i).ToString("F2");
        }

        // Updating the X-axis labels
        for (int i = 0; i< xPlotTexts.Length; i++)
        {
            xPlotTexts[i].text = i.ToString();
        }

        // Updating the Z-axis labels
        for (int i = 0; i < zPlotTexts.Length; i++)
        {
            // Check if there is a corresponding selected column for this index
            if (i < selectedColumns.Count)
            {
                // If there is, set the label to the column name
                zPlotTexts[i].text = selectedColumns[i];
            }
            else
            {
                // If there isn't, set the label to an empty string
                zPlotTexts[i].text = "";
            }
        }

        yPlotPoints = CalculatePlotPoints(dropdownY.options[dropdownY.value].text);
    }

    public void CalculateAllPlotPoints()
    {
        xPlotPoints = CalculatePlotPoints(dropdownX.options[dropdownX.value].text);
        yPlotPoints = CalculatePlotPoints(dropdownY.options[dropdownY.value].text);
        zPlotPoints = CalculatePlotPoints(dropdownZ.options[dropdownZ.value].text);
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

    public float NormalizeData(Dictionary<string, object> data, string columnName)
    {
        float value = Convert.ToSingle(data[columnName]);
        float max = FindMaxValue(columnName);
        float min = FindMinValue(columnName);
        return (value - min) / (max - min);
    }

    public void LineGraphPlot()
    {
        // Clear previous points and lines
        foreach (Transform child in PointHolder.transform)
        {
            Destroy(child.gameObject);
        }

        // Find global min and max values for the Y axis across all selected columns
        float globalMin = float.MaxValue;
        float globalMax = float.MinValue;

        foreach (string columnName in selectedColumns)
        {
            foreach (var point in pointList)
            {
                float value = Convert.ToSingle(point[columnName]);
                if (value < globalMin) globalMin = value;
                if (value > globalMax) globalMax = value;
            }
        }

        Vector3 floorSize = floor.GetComponent<Renderer>().bounds.size;
        Vector3 floorPosition = floor.transform.position;

        // Store generated colors for reuse with text labels
        List<Color> generatedColors = new List<Color>();

        for (int columnIndex = 0; columnIndex < selectedColumns.Count; columnIndex++)
        {
            Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1);
            generatedColors.Add(randomColor);

            string columnName = selectedColumns[columnIndex];
            List<Vector3> linePoints = new List<Vector3>();

            // Generate 10 plot points for the current column
            for (int day = 1; day <= 10; day++)
            {
                float yValue = Convert.ToSingle(pointList[day - 1][columnName]);
                float normalizedY = (yValue - globalMin) / (globalMax - globalMin);

                Vector3 plotPosition = new Vector3(
                    floorPosition.x + 2 + (columnIndex * 1),
                    floorPosition.y + (normalizedY * plotScale) + heightOffset,
                    floorPosition.z + ((day / 10f) * floorSize.z) - (floorSize.z / 2)
                );

                linePoints.Add(plotPosition);

                // Instantiate data point at this plot position
                GameObject dataPoint = Instantiate(PointPrefab, plotPosition, Quaternion.identity);
                dataPoint.transform.parent = PointHolder.transform; // Set parent to keep the scene organized
                dataPoint.GetComponent<Renderer>().material.color = randomColor; // Set the data point's color
            }

            // Draw the line for the current column with the generated random color
            GameObject line = DrawLine(linePoints, randomColor);
            line.transform.parent = PointHolder.transform; // Set parent for the line as well
        }
        // Update Z-axis labels with the generated colors
        UpdateZAxisLabels(generatedColors);
    }

    private GameObject DrawLine(List<Vector3> points, Color color)
    {
        GameObject line = new GameObject("Line");
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/Color")); // Use a basic unlit color material
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
        return line; // Return the created line object
    }

    public void AddSelectedColumn()
    {
        // Get the currently selected option
        string selectedColumn = lineGraphSelectedColumn.options[lineGraphSelectedColumn.value].text;

        // Debugging: Print current contents of selectedColumns
        Debug.Log("Current selected columns: " + String.Join(", ", selectedColumns));

        // Check if this column has already been selected
        if (!selectedColumns.Contains(selectedColumn))
        {
            // Add the column to the list and update the feedback text
            selectedColumns.Add(selectedColumn);
            feedbackText.text = $"Added:" + String.Join(", ", selectedColumns);

            // Debugging: Print updated contents of selectedColumns
            Debug.Log("Updated selected columns: " + String.Join(", ", selectedColumns));
        }
        else
        {
            // Inform the user that this column is already added
            feedbackText.text = "Column already added: " + selectedColumn;
        }
    }

    public void UpdateZAxisLabels(List<Color> colors)
    {
        for (int i = 0; i < zPlotTexts.Length; i++)
        {
            if (i < selectedColumns.Count)
            {
                zPlotTexts[i].text = selectedColumns[i];
                zPlotTexts[i].color = colors[i]; // Set the text color
            }
            else
            {
                zPlotTexts[i].text = ""; // Clear any unused labels
            }
        }
    }
}
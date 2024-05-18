using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CSVPlotter : MonoBehaviour
{
    #region db Variables

    private DatabaseManager db;
    private string currentCSV;

    #endregion

    #region Global Variables

    public string columnXName;
    public string columnYName;
    public string columnZName;

    public float plotScale = 10;
    public GameObject floor;
    public float heightOffset = 1f;  // Points will spawn this much above the floor

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

    #region Global Methods

    private void Start()
    {
        db = GameManager.Instance.GetComponent<DatabaseManager>();
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

    public void SetData(List<Dictionary<string, object>> data, string csvName)
    {
        pointList = data;
        currentCSV = csvName;
        PopulateDropdowns();
        Debug.Log("Data has been initialized in the dropdown.");
    }

    public void StopPlot(string plotType)
    {
        db.StopPlotTimer(plotType);
    }

    public void CalculateAllPlotPoints()
    {
        xPlotPoints = CalculatePlotPoints(dropdownX.options[dropdownX.value].text);
        yPlotPoints = CalculatePlotPoints(dropdownY.options[dropdownY.value].text);
        zPlotPoints = CalculatePlotPoints(dropdownZ.options[dropdownZ.value].text);
    }

    public float[] CalculatePlotPoints(string columnName)
    {
        float maxVal = FindMaxValue(columnName);
        float minVal = FindMinValue(columnName);
        float[] plotPoints = new float[10]; 

        float interval = (maxVal - minVal) / 9; 

        for (int i = 0; i < 10; i++)
        {
            plotPoints[i] = minVal + interval * i;
        }

        return plotPoints; 
    }

    private void PopulateDropdowns()
    {
        List<string> columnList = new List<string>(pointList[0].Keys);

        dropdownX.ClearOptions();
        dropdownY.ClearOptions();
        dropdownZ.ClearOptions();
        lineGraphSelectedColumn.ClearOptions();
        histogramXDropdown.ClearOptions();
        histogramZDropdown.ClearOptions();

        dropdownX.AddOptions(columnList);
        dropdownY.AddOptions(columnList);
        dropdownZ.AddOptions(columnList);
        lineGraphSelectedColumn.AddOptions(columnList);
        histogramXDropdown.AddOptions(columnList);
        histogramZDropdown.AddOptions(columnList);

    }

    public void UpdatePlotPointTexts()
    {
        UpdateAxisLabels(xPlotPoints, xPlotTexts);
        UpdateAxisLabels(yPlotPoints, yPlotTexts);
        UpdateAxisLabels(zPlotPoints, zPlotTexts);
    }

    private void UpdateAxisLabels(float[] plotPoints, TextMeshProUGUI[] plotTexts)
{
    for (int i = 0; i < plotPoints.Length && i < plotTexts.Length; i++)
    {
        plotTexts[i].text = Mathf.Round(plotPoints[i]).ToString("F0"); // "F0" format specifier for no decimal points
    }
}

    public float NormalizeData(Dictionary<string, object> data, string columnName)
    {
        float value = Convert.ToSingle(data[columnName]);
        float max = FindMaxValue(columnName);
        float min = FindMinValue(columnName);
        return (value - min) / (max - min);
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
    #endregion

    #region Scatterplot
    public void PlotData()
    {
        // Clear previous points
        foreach (Transform child in PointHolder.transform)
        {
            Destroy(child.gameObject);
        }

        db.StartPlotTimer("Scatterplot", currentCSV);

        // Set tag for PointHolder
        PointHolder.transform.tag = "Scatterplot";

        // Rotate PointHolder by 180 degrees around the Y-axis
        PointHolder.transform.rotation = Quaternion.Euler(0, 180, 0);

        columnXName = dropdownX.options[dropdownX.value].text;
        columnYName = dropdownY.options[dropdownY.value].text;
        columnZName = dropdownZ.options[dropdownZ.value].text;

        // Find max and min values for normalization
        float xMax = FindMaxValue(columnXName), xMin = FindMinValue(columnXName);
        float yMax = FindMaxValue(columnYName), yMin = FindMinValue(columnYName);
        float zMax = FindMaxValue(columnZName), zMin = FindMinValue(columnZName);

        Vector3 floorSize = floor.GetComponent<Renderer>().bounds.size;
        Vector3 floorPosition = floor.transform.position;

        // Check if the CSV contains "Title" or "Name" columns
        bool hasTitleColumn = pointList[0].ContainsKey("Title");
        bool hasNameColumn = pointList[0].ContainsKey("Name");

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

            // Adjust the position to account for the 180-degree rotation
            plotPosition = PointHolder.transform.TransformPoint(plotPosition);

            // Instantiate and position the data point
            GameObject dataPoint = Instantiate(PointPrefab, plotPosition, Quaternion.identity);
            dataPoint.transform.parent = PointHolder.transform;

            // Get the title or name value if available
            string titleOrName = "";
            if (hasTitleColumn)
            {
                titleOrName = Convert.ToString(point["Title"]);
            }
            else if (hasNameColumn)
            {
                titleOrName = Convert.ToString(point["Name"]);
            }

            // Naming and coloring the data point
            if (!string.IsNullOrEmpty(titleOrName))
            {
                dataPoint.name = $"{titleOrName}: {point[columnXName]} {point[columnYName]} {point[columnZName]}";
            }
            else
            {
                dataPoint.name = $"{point[columnXName]} {point[columnYName]} {point[columnZName]}";
            }

            dataPoint.GetComponent<Renderer>().material.color = new Color(x, y, z, 1.0f);
        }
        Debug.Log("Data has been plotted successfully.");
    }
    #endregion

    #region Linegraph
    public GameObject[] lineGraphReferencePoints;


    public void CalculateLineGraphPoints()
    {
        float globalMax = float.MinValue;
        float globalMin = float.MaxValue;

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
        for (int i = 0; i < xPlotTexts.Length; i++)
        {
            xPlotTexts[i].text = (i + 1).ToString();
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

    public void LineGraphPlot()
    {
        // Clear previous points and lines
        foreach (Transform child in PointHolder.transform)
        {
            Destroy(child.gameObject);
        }

        db.StartPlotTimer("Linegraph", currentCSV);

        PointHolder.transform.tag = "LineGraph";

        // Rotate PointHolder by 90 degrees around the Y-axis
        PointHolder.transform.rotation = Quaternion.Euler(0, 90, 0);

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

        // Store generated colors for reuse with text labels
        List<Color> generatedColors = new List<Color>();

        // Fetch X labels
        float[] xLabels = FetchXValues();

        for (int columnIndex = 0; columnIndex < selectedColumns.Count; columnIndex++)
        {
            Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1);
            generatedColors.Add(randomColor);

            string columnName = selectedColumns[columnIndex];
            List<Vector3> linePoints = new List<Vector3>();

            // Get the reference point for this column group
            Vector3 referencePoint = lineGraphReferencePoints[columnIndex].transform.position;

            // Generate plot points for the current column
            for (int i = 0; i < pointList.Count && i < xLabels.Length; i++)
            {
                float zValue = xLabels[i]; // Use X label value for Z position
                float yValue = Convert.ToSingle(pointList[i][columnName]);
                float normalizedY = (yValue - globalMin) / (globalMax - globalMin);

                // Calculate plot position using Z label
                Vector3 plotPosition = new Vector3(
                    referencePoint.x, // X position based on the reference point
                    referencePoint.y + (normalizedY * plotScale) + heightOffset, // Y position based on value
                    referencePoint.z + ((zValue - 1) / 9f * floor.GetComponent<Renderer>().bounds.size.z) - (floor.GetComponent<Renderer>().bounds.size.z / 2) // Z position based on xLabels
                );

                // Ensure plotPosition stays within the bounds of the floor
                plotPosition.z = Mathf.Clamp(plotPosition.z, referencePoint.z - floor.GetComponent<Renderer>().bounds.size.z / 2, referencePoint.z + floor.GetComponent<Renderer>().bounds.size.z / 2);

                linePoints.Add(plotPosition);

                // Instantiate data point at this plot position
                GameObject dataPoint = Instantiate(PointPrefab, plotPosition, Quaternion.identity);
                dataPoint.transform.parent = PointHolder.transform; // Set parent to keep the scene organized
                dataPoint.GetComponent<Renderer>().material.color = randomColor; // Set the data point's color

                string dataValue = yValue.ToString("F2");
                dataPoint.name = $"{columnName} {dataValue} {zValue}";
            }

            // Draw the line for the current column with the generated random color
            GameObject line = DrawLine(linePoints, randomColor);
            line.transform.parent = PointHolder.transform; // Set parent for the line as well
        }

        // Update Z-axis labels with the generated colors
        UpdateZAxisLabels(generatedColors);
    }

    // Helper method to fetch X labels from xPlotTexts
    private float[] FetchXValues()
    {
        return xPlotTexts.Select(x => float.Parse(x.text)).ToArray();
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
        string selectedColumn = lineGraphSelectedColumn.options[lineGraphSelectedColumn.value].text;

        Debug.Log("Current selected columns: " + String.Join(", ", selectedColumns));

        if (!selectedColumns.Contains(selectedColumn))
        {
            selectedColumns.Add(selectedColumn);
            feedbackText.text = $"Added:" + String.Join(", ", selectedColumns);

            Debug.Log("Updated selected columns: " + String.Join(", ", selectedColumns));
        }
        else
        {
            feedbackText.text = "Column already added: " + selectedColumn;
        }
    }
    #endregion

    #region Histogram
    public TMP_Dropdown histogramXDropdown, histogramZDropdown;
    public int numberOfBins = 10; 
    public GameObject plotParent;
    private int[,] bins;

    public void GenerateHistogram()
    {
        db.StartPlotTimer("Histogram", currentCSV);

        if (pointList == null || pointList.Count == 0)
        {
            Debug.Log("No data available to generate histogram.");
            return;
        }

        string xColumn = histogramXDropdown.options[histogramXDropdown.value].text;
        string zColumn = histogramZDropdown.options[histogramZDropdown.value].text;

        var xValues = pointList.Select(dict => Convert.ToSingle(dict[xColumn])).ToList();
        var zValues = pointList.Select(dict => Convert.ToSingle(dict[zColumn])).ToList();

        float xMin = xValues.Min();
        float xMax = xValues.Max();
        float zMin = zValues.Min();
        float zMax = zValues.Max();

        float xRange = (xMax - xMin) / numberOfBins;
        float zRange = (zMax - zMin) / numberOfBins;

        bins = new int[numberOfBins, numberOfBins];

        foreach (var dict in pointList)
        {
            int xIndex = Mathf.Clamp(Mathf.FloorToInt((Convert.ToSingle(dict[xColumn]) - xMin) / xRange), 0, numberOfBins - 1);
            int zIndex = Mathf.Clamp(Mathf.FloorToInt((Convert.ToSingle(dict[zColumn]) - zMin) / zRange), 0, numberOfBins - 1);
            bins[xIndex, zIndex]++;
        }

        // Find maximum count in bins for scaling Y-axis
        int maxCount = bins.Cast<int>().Max();

        // Update Y-axis labels based on max count
        UpdateHistogramYAxisLabels(maxCount);

        // Clear previous histogram bars
        foreach (Transform child in PointHolder.transform)
        {
            Destroy(child.gameObject);
        }

        Vector3 plotDimensions = new Vector3(9f, plotParent.transform.localScale.y - 1, 9f);
        //plotParent.transform.position = new Vector3(2, -2, -1);

        // Create new histogram bars
        for (int i = 0; i < numberOfBins; i++)
        {
            for (int j = 0; j < numberOfBins; j++)
            {
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                float normalizedX = (float)i / (numberOfBins - 1) * plotDimensions.x;
                float normalizedZ = (float)j / (numberOfBins - 1) * plotDimensions.z;
                Vector3 barPosition = new Vector3(normalizedX, bins[i, j] * 0.5f, normalizedZ) + PointHolder.transform.position - plotDimensions / 2;

                bar.transform.position = barPosition;
                bar.transform.localScale = new Vector3(plotDimensions.x / numberOfBins * 0.9f, bins[i, j], plotDimensions.z / numberOfBins * 0.9f);
                bar.transform.parent = PointHolder.transform;

                // Color interpolation based on count
                float intensity = (float)bins[i, j] / maxCount;
                bar.GetComponent<Renderer>().material.color = new Color(0f, intensity, 0f, 1f);  // Green intensity scales with count

                float xRangeStart = xMin + i * xRange;
                float xRangeEnd = xRangeStart + xRange;
                float zRangeStart = zMin + j * zRange;
                float zRangeEnd = zRangeStart + zRange;
                bar.name = $"{xColumn} [{xRangeStart:F2}-{xRangeEnd:F2}], {zColumn} [{zRangeStart:F2}-{zRangeEnd:F2}]: {bins[i, j]}";
                bar.transform.tag = "DataPoint";
                bar.transform.parent.tag = "Histogram";
            }
        }
    }

    public void CalculateHistogramPlotPoints()
    {
        string xColumn = histogramXDropdown.options[histogramXDropdown.value].text;
        string zColumn = histogramZDropdown.options[histogramZDropdown.value].text;

        xPlotPoints = CalculateHistogramAxisPoints(xColumn);
        zPlotPoints = CalculateHistogramAxisPoints(zColumn);

        // Update UI elements or other related components
        UpdateAxisLabels(xPlotPoints, xPlotTexts);
        UpdateAxisLabels(zPlotPoints, zPlotTexts);

    }

    private float[] CalculateHistogramAxisPoints(string columnName)
    {
        float maxVal = FindMaxValue(columnName);
        float minVal = FindMinValue(columnName);
        float[] plotPoints = new float[10];  // This number can be dynamic based on UI or other settings

        float interval = (maxVal - minVal) / 9; // Calculate 10 intervals (for 10 points)

        for (int i = 0; i < 10; i++)
        {
            plotPoints[i] = minVal + interval * i;
        }

        return plotPoints;
    }

    private void CalculateHistogramYAxisPoints()
    {
        // Assuming bins[] has been filled and exists at this scope level
        int maxCount = bins.Cast<int>().Max(); // Find the maximum count across all bins

        yPlotPoints = new float[10];
        float interval = maxCount / 10.0f;

        for (int i = 0; i < 10; i++)
        {
            yPlotPoints[i] = (i + 1) * interval;
        }
    }

    private void UpdateHistogramYAxisLabels(int maxCount)
    {
        float interval = maxCount / 10.0f; // Divide the max count by 10 to find interval steps for labels
        yPlotPoints = new float[10];
        for (int i = 0; i < 10; i++)
        {
            yPlotPoints[i] = (i + 1) * interval;
            yPlotTexts[i].text = $"{(i + 1) * interval:N0}";  // Format as integer (no decimal points)
        }
    }
}
    #endregion








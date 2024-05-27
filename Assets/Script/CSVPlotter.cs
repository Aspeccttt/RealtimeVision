#region Unity Imports
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEditor;
#endregion

public class CSVPlotter : MonoBehaviour
{
    #region db Variables
    private DatabaseManager db;
    public string currentCSV;
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

    // LineGraph
    public TMP_Dropdown lineGraphSelectedColumn;
    public TextMeshProUGUI feedbackText;
    public List<string> selectedColumns = new List<string>(); // This now becomes global

    // Colour Picker Scatterplot
    public Color pointColor = Color.white;

    // Self explained
    public Color histogramColor = Color.white;
    #endregion

    #region Unity Methods
    /// <summary>
    /// Initialize the CSVPlotter.
    /// </summary>
    private void Start()
    {
        db = GameManager.Instance.GetComponent<DatabaseManager>();
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Check if a column contains numeric data.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>True if the column is numeric, false otherwise.</returns>
    private bool IsNumericColumn(string columnName)
    {
        foreach (var point in pointList)
        {
            if (point.ContainsKey(columnName))
            {
                if (!float.TryParse(point[columnName].ToString(), out _))
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Find the maximum value in a column.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>The maximum value.</returns>
    private float FindMaxValue(string columnName)
    {
        float maxValue = float.MinValue;
        foreach (var point in pointList)
        {
            try
            {
                float value = Convert.ToSingle(point[columnName]);
                if (value > maxValue)
                {
                    maxValue = value;
                }
            }
            catch (FormatException)
            {
                Debug.LogWarning($"Unable to parse '{point[columnName]}' as a float.");
            }
        }
        return maxValue;
    }

    /// <summary>
    /// Find the minimum value in a column.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>The minimum value.</returns>
    private float FindMinValue(string columnName)
    {
        float minValue = float.MaxValue;
        foreach (var point in pointList)
        {
            try
            {
                float value = Convert.ToSingle(point[columnName]);
                if (value < minValue)
                {
                    minValue = value;
                }
            }
            catch (FormatException)
            {
                Debug.LogWarning($"Unable to parse '{point[columnName]}' as a float.");
            }
        }
        return minValue;
    }

    /// <summary>
    /// Populate the dropdowns with column names.
    /// </summary>
    private void PopulateDropdowns()
    {
        List<string> columnList = new List<string>(pointList[0].Keys);

        // Filter only numeric columns
        List<string> numericColumns = columnList.Where(IsNumericColumn).ToList();

        dropdownX.ClearOptions();
        dropdownY.ClearOptions();
        dropdownZ.ClearOptions();
        lineGraphSelectedColumn.ClearOptions();
        histogramXDropdown.ClearOptions();
        histogramZDropdown.ClearOptions();

        dropdownX.AddOptions(numericColumns);
        dropdownY.AddOptions(numericColumns);
        dropdownZ.AddOptions(numericColumns);
        lineGraphSelectedColumn.AddOptions(numericColumns);
        histogramXDropdown.AddOptions(numericColumns);
        histogramZDropdown.AddOptions(numericColumns);
    }

    /// <summary>
    /// Update the plot point texts.
    /// </summary>
    public void UpdatePlotPointTexts()
    {
        UpdateAxisLabels(xPlotPoints, xPlotTexts);
        UpdateAxisLabels(yPlotPoints, yPlotTexts);
        UpdateAxisLabels(zPlotPoints, zPlotTexts);
    }

    /// <summary>
    /// Update the axis labels.
    /// </summary>
    /// <param name="plotPoints">Array of plot points.</param>
    /// <param name="plotTexts">Array of plot text UI elements.</param>
    private void UpdateAxisLabels(float[] plotPoints, TextMeshProUGUI[] plotTexts)
    {
        for (int i = 0; i < plotPoints.Length && i < plotTexts.Length; i++)
        {
            plotTexts[i].text = Mathf.Round(plotPoints[i]).ToString("F0"); // "F0" format specifier for no decimal points
        }
    }

    /// <summary>
    /// Normalize data for plotting.
    /// </summary>
    /// <param name="data">The data dictionary.</param>
    /// <param name="columnName">The column name.</param>
    /// <returns>Normalized value.</returns>
    public float NormalizeData(Dictionary<string, object> data, string columnName)
    {
        float value = Convert.ToSingle(data[columnName]);
        float max = FindMaxValue(columnName);
        float min = FindMinValue(columnName);
        return (value - min) / (max - min);
    }

    /// <summary>
    /// Update Z-axis labels with colors and column names.
    /// </summary>
    /// <param name="colors">List of colors.</param>
    /// <param name="columnNames">List of column names.</param>
    public void UpdateZAxisLabels(List<Color> colors, List<string> columnNames)
    {
        for (int i = 0; i < zPlotTexts.Length; i++)
        {
            if (i < columnNames.Count)
            {
                zPlotTexts[i].text = columnNames[i];
                zPlotTexts[i].color = colors[i]; // Set the text color
            }
            else
            {
                zPlotTexts[i].text = ""; // Clear any unused labels
            }
        }
    }

    /// <summary>
    /// Set the data for plotting.
    /// </summary>
    /// <param name="data">The data to set.</param>
    /// <param name="csvName">The name of the CSV file.</param>
    public void SetData(List<Dictionary<string, object>> data, string csvName)
    {
        pointList = data;
        currentCSV = csvName;
        PopulateDropdowns();
        Debug.Log("Data has been initialized in the dropdown.");
    }

    /// <summary>
    /// Stop the plot timer for a specific plot type.
    /// </summary>
    /// <param name="plotType">The type of plot.</param>
    public void StopPlot(string plotType)
    {
        db.StopPlotTimer(plotType);
    }

    /// <summary>
    /// Calculate all plot points for the current data.
    /// </summary>
    public void CalculateAllPlotPoints()
    {
        xPlotPoints = CalculatePlotPoints(dropdownX.options[dropdownX.value].text);
        yPlotPoints = CalculatePlotPoints(dropdownY.options[dropdownY.value].text);
        zPlotPoints = CalculatePlotPoints(dropdownZ.options[dropdownZ.value].text);
    }

    /// <summary>
    /// Calculate plot points for a specific column.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>Array of plot points.</returns>
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
    #endregion

    #region Scatterplot
    /// <summary>
    /// Plot data points for a scatterplot.
    /// </summary>
    public void PlotData()
    {
        // Clear previous points
        foreach (Transform child in PointHolder.transform)
        {
            Destroy(child.gameObject);
        }

        // Gather column info
        string columnInfo = $"X: {dropdownX.options[dropdownX.value].text}, Y: {dropdownY.options[dropdownY.value].text}, Z: {dropdownZ.options[dropdownZ.value].text}";
        db.StartPlotTimer("Scatterplot", columnInfo);

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

            // Naming the data point
            if (!string.IsNullOrEmpty(titleOrName))
            {
                dataPoint.name = $"{titleOrName}: {point[columnXName]} {point[columnYName]} {point[columnZName]}";
            }
            else
            {
                dataPoint.name = $"{point[columnXName]} {point[columnYName]} {point[columnZName]}";
            }

            // Generate the gradient color
            Color gradientColor = new Color(
                pointColor.r * y,
                pointColor.g * y,
                pointColor.b * y,
                1.0f
            );

            dataPoint.GetComponent<Renderer>().material.color = gradientColor;
        }

        Debug.Log("Data has been plotted successfully.");
    }
    #endregion

    #region Linegraph
    public GameObject[] lineGraphReferencePoints;

    /// <summary>
    /// Get the reference column for the line graph.
    /// </summary>
    /// <returns>The reference column name.</returns>
    private string GetReferenceColumn()
    {
        string[] possibleColumns = { "Day", "Days", "Time" };
        foreach (var column in possibleColumns)
        {
            if (pointList[0].ContainsKey(column))
            {
                return column;
            }
        }
        return null;
    }

    /// <summary>
    /// Calculate points for the line graph.
    /// </summary>
    public void CalculateLineGraphPoints()
    {
        string referenceColumn = GetReferenceColumn();
        if (referenceColumn == null)
        {
            Debug.Log("No reference column found (Day, Days, or Time).");
            GameManager.Instance.ShowNotification("Invalid Plot Type for CSV Dataset!");
            return;
        }

        float globalMax = float.MinValue;
        float globalMin = float.MaxValue;

        int numberOfDays = pointList.Count;

        foreach (string column in selectedColumns)
        {
            if (IsNumericColumn(column))
            {
                float localMax = FindMaxValue(column);
                float localMin = FindMinValue(column);
                if (localMax > globalMax) globalMax = localMax;
                if (localMin < globalMin) globalMin = localMin;
            }
        }

        float interval = (globalMax - globalMin) / (numberOfDays - 1);

        for (int i = 0; i < yPlotTexts.Length && i < numberOfDays; i++)
        {
            yPlotTexts[i].text = (globalMin + interval * i).ToString("F2");
        }

        // Reverse xPlotTexts array
        Array.Reverse(xPlotTexts);
        for (int i = 0; i < xPlotTexts.Length && i < numberOfDays; i++)
        {
            xPlotTexts[i].text = (i + 1).ToString(); // Assuming the reference column contains sequential days
        }

        for (int i = 0; i < zPlotTexts.Length; i++)
        {
            if (i < selectedColumns.Count)
            {
                zPlotTexts[i].text = selectedColumns[i];
            }
            else
            {
                zPlotTexts[i].text = "";
            }
        }

        yPlotPoints = CalculatePlotPoints(dropdownY.options[dropdownY.value].text);
    }

    /// <summary>
    /// Plot the line graph.
    /// </summary>
    public void LineGraphPlot()
    {
        string referenceColumn = GetReferenceColumn();
        if (referenceColumn == null)
        {
            Debug.Log("No reference column found (Day, Days, or Time).");
            GameManager.Instance.ShowNotification("Invalid Plot Type for CSV Dataset!");
            return;
        }

        foreach (Transform child in PointHolder.transform)
        {
            Destroy(child.gameObject);
        }

        db.StartPlotTimer("Linegraph", currentCSV);

        PointHolder.transform.tag = "LineGraph";
        PointHolder.transform.rotation = Quaternion.Euler(0, 90, 0);

        float globalMin = float.MaxValue;
        float globalMax = float.MinValue;

        foreach (string columnName in selectedColumns)
        {
            if (IsNumericColumn(columnName))
            {
                foreach (var point in pointList)
                {
                    try
                    {
                        float value = Convert.ToSingle(point[columnName]);
                        if (value < globalMin) globalMin = value;
                        if (value > globalMax) globalMax = value;
                    }
                    catch (FormatException)
                    {
                        Debug.LogWarning($"Unable to parse '{point[columnName]}' as a float.");
                    }
                }
            }
        }

        List<Color> generatedColors = new List<Color>();
        float zInterval = floor.GetComponent<Renderer>().bounds.size.z / (pointList.Count - 1);

        bool hasTitleColumn = pointList[0].ContainsKey("Title");
        bool hasNameColumn = pointList[0].ContainsKey("Name");

        int xOffsetCounter = 0;
        Dictionary<string, Color> nameColorMapping = new Dictionary<string, Color>();
        Dictionary<string, List<Vector3>> nameLinePointsMapping = new Dictionary<string, List<Vector3>>();

        for (int columnIndex = 0; columnIndex < selectedColumns.Count; columnIndex++)
        {
            if (IsNumericColumn(selectedColumns[columnIndex]))
            {
                string columnName = selectedColumns[columnIndex];
                List<Vector3> linePoints = new List<Vector3>();

                Vector3 referencePoint = lineGraphReferencePoints[columnIndex].transform.position;

                string lastTitleOrName = null;

                for (int i = 0; i < pointList.Count; i++)
                {
                    try
                    {
                        float zValue = Convert.ToSingle(pointList[i][referenceColumn]); // Fetch the day directly from the CSV
                        float yValue = Convert.ToSingle(pointList[i][columnName]);
                        float normalizedY = (yValue - globalMin) / (globalMax - globalMin);

                        string titleOrName = "";
                        if (hasTitleColumn)
                        {
                            titleOrName = Convert.ToString(pointList[i]["Title"]).Replace(" ", "");
                        }
                        else if (hasNameColumn)
                        {
                            titleOrName = Convert.ToString(pointList[i]["Name"]).Replace(" ", "");
                        }

                        if (!nameColorMapping.ContainsKey(titleOrName))
                        {
                            Color uniqueColor = GenerateUniqueColor(generatedColors);
                            nameColorMapping[titleOrName] = uniqueColor;
                            generatedColors.Add(uniqueColor);
                        }

                        if (lastTitleOrName == null || titleOrName != lastTitleOrName)
                        {
                            xOffsetCounter++;
                            lastTitleOrName = titleOrName;
                        }

                        Vector3 plotPosition = new Vector3(
                            referencePoint.x - (xOffsetCounter * 0.5f), // Increment X position for each different name on the opposite side
                            referencePoint.y + (normalizedY * plotScale) + heightOffset,
                            referencePoint.z + zValue - (floor.GetComponent<Renderer>().bounds.size.z / 2) - (1)
                        );

                        plotPosition.x = Mathf.Clamp(plotPosition.x, referencePoint.x - floor.GetComponent<Renderer>().bounds.size.x / 2, referencePoint.x + floor.GetComponent<Renderer>().bounds.size.x / 2);

                        linePoints.Add(plotPosition);

                        if (!nameLinePointsMapping.ContainsKey(titleOrName))
                        {
                            nameLinePointsMapping[titleOrName] = new List<Vector3>();
                        }
                        nameLinePointsMapping[titleOrName].Add(plotPosition);

                        GameObject dataPoint = Instantiate(PointPrefab, plotPosition, Quaternion.identity);
                        dataPoint.transform.parent = PointHolder.transform;
                        dataPoint.GetComponent<Renderer>().material.color = nameColorMapping[titleOrName];

                        dataPoint.name = !string.IsNullOrEmpty(titleOrName)
                            ? $"{titleOrName}: {columnName} {yValue} {zValue}"
                            : $"{columnName} {yValue} {zValue}";
                    }
                    catch (FormatException)
                    {
                        Debug.LogWarning($"Unable to parse '{pointList[i][columnName]}' as a float.");
                    }
                    GameManager.Instance.ShowNotification("Linegraph plotted!");
                }
            }
        }

        // Draw lines for each unique name using the corresponding color
        foreach (var nameLinePoints in nameLinePointsMapping)
        {
            GameObject line = DrawLine(nameLinePoints.Value, nameColorMapping[nameLinePoints.Key]);
            line.transform.parent = PointHolder.transform;

            Debug.Log($"Drawn line for {nameLinePoints.Key} with color {nameColorMapping[nameLinePoints.Key]}");
        }

        UpdateZAxisLabels(nameColorMapping.Values.ToList(), nameLinePointsMapping.Keys.ToList());
    }

    /// <summary>
    /// Generate a unique color.
    /// </summary>
    /// <param name="usedColors">List of used colors.</param>
    /// <returns>A unique color.</returns>
    private Color GenerateUniqueColor(List<Color> usedColors)
    {
        Color newColor;
        do
        {
            newColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1);
        } while (usedColors.Contains(newColor));
        return newColor;
    }

    /// <summary>
    /// Draw a line connecting the given points.
    /// </summary>
    /// <param name="points">List of points to connect.</param>
    /// <param name="color">Color of the line.</param>
    /// <returns>The created line GameObject.</returns>
    private GameObject DrawLine(List<Vector3> points, Color color)
    {
        GameObject line = new GameObject("Line");
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default")); // Use the Sprites/Default shader
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());

        Debug.Log($"Line drawn with color: {color}"); // Debug information

        return line;
    }

    /// <summary>
    /// Add the selected column to the list of selected columns for the line graph.
    /// </summary>
    public void AddSelectedColumn()
    {
        string selectedColumn = lineGraphSelectedColumn.options[lineGraphSelectedColumn.value].text;

        // Clear previously selected columns and add the new one
        selectedColumns.Clear();
        selectedColumns.Add(selectedColumn);

        // Provide feedback
        feedbackText.text = $"Selected column: {selectedColumn}";

        // Recalculate and update the line graph
        CalculateLineGraphPoints();
        LineGraphPlot();

        Debug.Log("Updated selected column: " + selectedColumn);
    }
    #endregion

    #region Histogram
    public TMP_Dropdown histogramXDropdown, histogramZDropdown;
    public int numberOfBins = 10;
    public GameObject plotParent;
    private int[,] bins;

    /// <summary>
    /// Generate a histogram from the current data.
    /// </summary>
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

                // Generate the gradient color for the histogram bars
                Color gradientColor = new Color(
                    histogramColor.r * bins[i, j] / (float)maxCount,
                    histogramColor.g * bins[i, j] / (float)maxCount,
                    histogramColor.b * bins[i, j] / (float)maxCount,
                    1.0f
                );

                bar.GetComponent<Renderer>().material.color = gradientColor;

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

    /// <summary>
    /// Calculate plot points for the histogram.
    /// </summary>
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

    /// <summary>
    /// Calculate plot points for a histogram axis.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>Array of plot points.</returns>
    private float[] CalculateHistogramAxisPoints(string columnName)
    {
        float maxVal = FindMaxValue(columnName);
        float minVal = FindMinValue(columnName);
        float[] plotPoints = new float[10];

        float interval = (maxVal - minVal) / 9; // Calculate 10 intervals (for 10 points)

        for (int i = 0; i < 10; i++)
        {
            plotPoints[i] = minVal + interval * i;
        }

        return plotPoints;
    }

    /// <summary>
    /// Calculate Y-axis points for the histogram.
    /// </summary>
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

    /// <summary>
    /// Update Y-axis labels for the histogram.
    /// </summary>
    /// <param name="maxCount">The maximum count in the histogram bins.</param>
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
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(CSVPlotter)), InitializeOnLoadAttribute]
public class CSVPlotterEditor : Editor
{
    CSVPlotter csvPlotter;
    SerializedObject serializedCSVPlotter;

    private void OnEnable()
    {
        csvPlotter = (CSVPlotter)target;
        serializedCSVPlotter = new SerializedObject(csvPlotter);
    }

    public override void OnInspectorGUI()
    {
        serializedCSVPlotter.Update();

        EditorGUILayout.Space();
        GUILayout.Label("CSV Plotter", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
        GUILayout.Label("Custom Script by: Ilario Cutajar", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        EditorGUILayout.Space();

        #region CSV Setup
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("CSV Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        csvPlotter.currentCSV = EditorGUILayout.TextField(new GUIContent("Current CSV", "Name of the currently loaded CSV file."), csvPlotter.currentCSV);
        EditorGUILayout.Space();
        #endregion

        #region Global Settings
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Global Settings", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        csvPlotter.plotScale = EditorGUILayout.FloatField(new GUIContent("Plot Scale", "Scale of the plot."), csvPlotter.plotScale);
        csvPlotter.heightOffset = EditorGUILayout.FloatField(new GUIContent("Height Offset", "Height offset for the points."), csvPlotter.heightOffset);

        csvPlotter.PointPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Point Prefab", "Prefab for the points."), csvPlotter.PointPrefab, typeof(GameObject), false);
        csvPlotter.PointHolder = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Point Holder", "Holder for the points."), csvPlotter.PointHolder, typeof(GameObject), true);
        EditorGUILayout.Space();
        #endregion

        #region Dropdowns Setup
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Dropdowns Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        csvPlotter.dropdownX = (TMP_Dropdown)EditorGUILayout.ObjectField(new GUIContent("Dropdown X", "Dropdown for X-axis."), csvPlotter.dropdownX, typeof(TMP_Dropdown), true);
        csvPlotter.dropdownY = (TMP_Dropdown)EditorGUILayout.ObjectField(new GUIContent("Dropdown Y", "Dropdown for Y-axis."), csvPlotter.dropdownY, typeof(TMP_Dropdown), true);
        csvPlotter.dropdownZ = (TMP_Dropdown)EditorGUILayout.ObjectField(new GUIContent("Dropdown Z", "Dropdown for Z-axis."), csvPlotter.dropdownZ, typeof(TMP_Dropdown), true);
        csvPlotter.lineGraphSelectedColumn = (TMP_Dropdown)EditorGUILayout.ObjectField(new GUIContent("Line Graph Selected Column", "Dropdown for selecting columns for line graph."), csvPlotter.lineGraphSelectedColumn, typeof(TMP_Dropdown), true);
        csvPlotter.histogramXDropdown = (TMP_Dropdown)EditorGUILayout.ObjectField(new GUIContent("Histogram X Dropdown", "Dropdown for X-axis in histogram."), csvPlotter.histogramXDropdown, typeof(TMP_Dropdown), true);
        csvPlotter.histogramZDropdown = (TMP_Dropdown)EditorGUILayout.ObjectField(new GUIContent("Histogram Z Dropdown", "Dropdown for Z-axis in histogram."), csvPlotter.histogramZDropdown, typeof(TMP_Dropdown), true);
        EditorGUILayout.Space();
        #endregion

        #region Plot Texts
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Plot Texts", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        SerializedProperty xPlotTexts = serializedCSVPlotter.FindProperty("xPlotTexts");
        EditorGUILayout.PropertyField(xPlotTexts, new GUIContent("X Plot Texts", "Text labels for X-axis."), true);
        SerializedProperty yPlotTexts = serializedCSVPlotter.FindProperty("yPlotTexts");
        EditorGUILayout.PropertyField(yPlotTexts, new GUIContent("Y Plot Texts", "Text labels for Y-axis."), true);
        SerializedProperty zPlotTexts = serializedCSVPlotter.FindProperty("zPlotTexts");
        EditorGUILayout.PropertyField(zPlotTexts, new GUIContent("Z Plot Texts", "Text labels for Z-axis."), true);
        EditorGUILayout.Space();
        #endregion

        #region Line Graph Settings
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Line Graph Settings", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        csvPlotter.feedbackText = (TextMeshProUGUI)EditorGUILayout.ObjectField(new GUIContent("Feedback Text", "Text to provide feedback on selected columns."), csvPlotter.feedbackText, typeof(TextMeshProUGUI), true);

        SerializedProperty selectedColumns = serializedCSVPlotter.FindProperty("selectedColumns");
        EditorGUILayout.PropertyField(selectedColumns, new GUIContent("Selected Columns", "List of selected columns for the line graph."), true);
        EditorGUILayout.Space();
        #endregion

        // Additional sections (Histogram, etc.) can be added similarly

        if (GUI.changed)
        {
            EditorUtility.SetDirty(csvPlotter);
            Undo.RecordObject(csvPlotter, "CSV Plotter Change");
            serializedCSVPlotter.ApplyModifiedProperties();
        }
    }
}
#endif

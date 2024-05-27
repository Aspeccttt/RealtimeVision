#region Unity Imports
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#endregion

public class ColorPickerUI : MonoBehaviour
{
    #region UI Elements
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;
    public Image colorDisplay;
    #endregion

    #region Variables
    private CSVPlotter csvPlotter;
    #endregion

    #region Unity Methods
    /// <summary>
    /// Initialize the ColorPickerUI.
    /// </summary>
    private void Start()
    {
        csvPlotter = FindObjectOfType<CSVPlotter>();

        // Initialize sliders
        redSlider.onValueChanged.AddListener(UpdateColor);
        greenSlider.onValueChanged.AddListener(UpdateColor);
        blueSlider.onValueChanged.AddListener(UpdateColor);

        // Set initial color
        Color initialColor = csvPlotter.pointColor;
        redSlider.value = initialColor.r;
        greenSlider.value = initialColor.g;
        blueSlider.value = initialColor.b;

        UpdateColorDisplay(initialColor);
    }
    #endregion

    #region Color Update
    /// <summary>
    /// Update the color based on slider values.
    /// </summary>
    /// <param name="_">Slider value (not used).</param>
    public void UpdateColor(float _)
    {
        Color newColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
        csvPlotter.pointColor = newColor;
        csvPlotter.histogramColor = newColor;
        UpdateColorDisplay(newColor);
    }

    /// <summary>
    /// Update the color display UI element.
    /// </summary>
    /// <param name="color">The new color to display.</param>
    private void UpdateColorDisplay(Color color)
    {
        colorDisplay.color = color;
    }
    #endregion
}

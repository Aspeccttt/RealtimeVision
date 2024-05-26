using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPickerUI : MonoBehaviour
{
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;
    public Image colorDisplay;

    private CSVPlotter csvPlotter;

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

    public void UpdateColor(float _)
    {
        Color newColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
        csvPlotter.pointColor = newColor;
        UpdateColorDisplay(newColor);
    }

    private void UpdateColorDisplay(Color color)
    {
        colorDisplay.color = color;
    }
}
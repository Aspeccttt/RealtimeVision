using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public TMP_Dropdown xDropdown;
    public TMP_Dropdown yDropdown;
    public TMP_Dropdown zDropdown;

    private Plotter plotter;

    public GameObject menuPanel;

    void Start()
    {
        plotter = FindObjectOfType<Plotter>();

        InitializeDropdownListeners();
    }

    void InitializeDropdownListeners()
    {
        xDropdown.onValueChanged.AddListener(delegate { UpdatePlotterAxes(); });
        yDropdown.onValueChanged.AddListener(delegate { UpdatePlotterAxes(); });
        zDropdown.onValueChanged.AddListener(delegate { UpdatePlotterAxes(); });
    }

    void UpdatePlotterAxes()
    {
        if (plotter != null)
        {
            string xColumn = xDropdown.options[xDropdown.value].text;
            string yColumn = yDropdown.options[yDropdown.value].text;
            string zColumn = zDropdown.options[zDropdown.value].text;

            plotter.UpdateColumnNames(xColumn, yColumn, zColumn);
        }
        else
        {
            Debug.LogError("Plotter component not found in the scene.");
        }
    }

    public void OnDoneButtonClick()
    {
        if (plotter != null)
        {
            plotter.PlotData(); // Plot the data using the current selections
            menuPanel.SetActive(false); // Replace 'menuPanel' with the actual reference to your menu
        }
        else
        {
            Debug.LogError("Plotter component not found in the scene.");
        }

        // Close the menu (assuming you have a reference to the menu panel)
    }
}

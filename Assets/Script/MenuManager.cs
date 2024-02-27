using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    #region Global Variables
    public GameObject waitingUIPanel;
    public GameObject uploadedUIPanel;
    public GameObject menuPanel;

    [SerializeField] TextMeshProUGUI menuCondition;

    public TMP_Dropdown xDropdown;
    public TMP_Dropdown yDropdown;
    public TMP_Dropdown zDropdown;

    public CSVPlotter CSVPlotter;

    #endregion

    void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    #region Main Menu Controller

    /// <summary>
    /// Toggle the menu's visibility.
    /// </summary>
    public void ToggleMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    public void changeMenuCondition(string Text)
    {
        menuCondition.text = Text;
    }

    public void CSVLoaded()
    {
        waitingUIPanel.SetActive(false);
        uploadedUIPanel.SetActive(true);
    }

    public void PopulateDropdowns(List<Dictionary<string, object>> pointList)
    {
        List<string> columnNames = new List<string>(pointList[0].Keys);

        xDropdown.ClearOptions();
        yDropdown.ClearOptions();
        zDropdown.ClearOptions();

        xDropdown.AddOptions(columnNames);
        yDropdown.AddOptions(columnNames);
        zDropdown.AddOptions(columnNames);
    }

    public void OnDoneButtonClicked()
    {
        CSVPlotter.columnXName = xDropdown.options[xDropdown.value].text;
        CSVPlotter.columnYName = yDropdown.options[yDropdown.value].text;
        CSVPlotter.columnZName = zDropdown.options[zDropdown.value].text;

        CSVPlotter.PlotData();
        ToggleMenu(); 
    }

    #endregion
}

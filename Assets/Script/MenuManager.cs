using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // UI Elements
    public TMP_Dropdown xDropdown;
    public TMP_Dropdown yDropdown;
    public TMP_Dropdown zDropdown;

    public TextMeshProUGUI xLabel;
    public TextMeshProUGUI yLabel;
    public TextMeshProUGUI zLabel;

    public GameObject uiPanel;

    void Start()
    {
        InitializeDropdownListeners();
    }

    /// <summary>
    /// This method will update the labels of the canvas (Labels inside the world).
    /// </summary>
    void InitializeDropdownListeners()
    {
        xDropdown.onValueChanged.AddListener(delegate { UpdateAxisLabel(xDropdown, xLabel); });
        yDropdown.onValueChanged.AddListener(delegate { UpdateAxisLabel(yDropdown, yLabel); });
        zDropdown.onValueChanged.AddListener(delegate { UpdateAxisLabel(zDropdown, zLabel); });
    }

    void UpdateAxisLabel(TMP_Dropdown dropdown, TextMeshProUGUI label)
    {
        label.text = dropdown.options[dropdown.value].text;
    }

    public void ClosePanel()
    {
        uiPanel.SetActive(false);
    }

    public void OpenPanel()
    {
        uiPanel.SetActive(true); 
    }
}

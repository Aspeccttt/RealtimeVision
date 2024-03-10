using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public TextMeshProUGUI xTitle;
    public TextMeshProUGUI yTitle;
    public TextMeshProUGUI zTitle;

    public Button[] buttons; // Assign this array in the inspector with your four buttons.
    private Button selectedButton = null; // Tracks the currently selected button.

    // Color settings for normal and selected buttons
    public Color normalColor = Color.black;
    public Color selectedColor = Color.green;

    public GameObject[] MenuPanels; // Assign the corresponding GameObjects for each button.


    #endregion

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
        Animator menuAnimator = menuPanel.GetComponent<Animator>();

        if (menuPanel.activeSelf)
        {
            menuAnimator.SetTrigger("Close");
            Cursor.lockState = CursorLockMode.Locked;
            StartCoroutine(DeactivateAfterAnimation(menuAnimator, "Close"));
        }
        else
        {
            menuPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            menuAnimator.SetTrigger("Open");
        }
    }

    private IEnumerator DeactivateAfterAnimation(Animator animator, string animation)
    {
        float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationDuration);

        if (animation == "Close")
        {
            menuPanel.SetActive(false);
        }
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
        GameManager.Instance.GetComponent<CSVPlotter>().columnXName = xDropdown.options[xDropdown.value].text;
        GameManager.Instance.GetComponent<CSVPlotter>().columnYName = yDropdown.options[yDropdown.value].text;
        GameManager.Instance.GetComponent<CSVPlotter>().columnZName = zDropdown.options[zDropdown.value].text;

        xTitle.text = GameManager.Instance.GetComponent<CSVPlotter>().columnXName;
        yTitle.text = GameManager.Instance.GetComponent<CSVPlotter>().columnYName;
        zTitle.text = GameManager.Instance.GetComponent<CSVPlotter>().columnZName;

        GameManager.Instance.GetComponent<CSVPlotter>().CalculateAllPlotPoints();
        GameManager.Instance.GetComponent<CSVPlotter>().UpdatePlotPointTexts();

        Debug.Log(GetSelectedButtonName());

        if (GetSelectedButtonName() == "Linegraph")
        {
            GameManager.Instance.GetComponent<LineGraphPlotter>().PlotData();

        }
        else if (GetSelectedButtonName() == "Scatterplot")
        {
            GameManager.Instance.GetComponent<CSVPlotter>().PlotData();

        }

        ToggleMenu();
    }

    public void ButtonClicked(Button clickedButton)
    {
        // Deselect the previously selected button by resetting its text color or other highlighted properties.
        if (selectedButton != null)
        {
            selectedButton.GetComponentInChildren<TextMeshProUGUI>().color = normalColor;
        }

        // Set the newly clicked button as the selected one.
        selectedButton = clickedButton;
        selectedButton.GetComponentInChildren<TextMeshProUGUI>().color = selectedColor;

        // Deactivate all panels then activate the corresponding one.
        foreach (var panel in MenuPanels)
        {
            panel.SetActive(false); // Deactivate all panels
        }

        int index = System.Array.IndexOf(buttons, clickedButton);
        if (index != -1 && index < MenuPanels.Length)
        {
            MenuPanels[index].SetActive(true); // Activate the corresponding panel
        }
    }

    // Optional: Call this method to get the name of the selected button.
    public string GetSelectedButtonName()
    {
        if (selectedButton != null)
        {
            return selectedButton.gameObject.name;
        }
        else
        {
            return "No button selected";
        }
    }

    #endregion
}

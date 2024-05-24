using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    #region db Variables
    private DatabaseManager db;
    #endregion

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

    public TMP_Dropdown MainDropdown; // Reference to the main dropdown

    public FirstPersonController firstPersonController;

    public GameObject AnswerPanel;

    private List<string> questions = new List<string>
{
    "Load the GPU dataset, find out the price of the RTX 3090 cost?",
    "Which is the best budget Graphics card?",
    "Which is the cheapest Graphics card?",
    "Load the Steam dataset, Which game had the lowest PlayerCount?",
    "On the first day, how much players were active on CSGO?",
    "Which game had the worst fall off on the 10th day?"
};

    private List<string> answers = new List<string>();
    private int currentQuestionIndex = 0;
    #endregion

    #region Question Box Variables
    public GameObject questionBox;
    public TextMeshProUGUI questionText1;
    public TextMeshProUGUI questionText2;
    public TMP_InputField answerInputField;
    public Button sendAnswerButton;
    #endregion

    #region UI Panel DB variables
    private float answerPanelOpenTime;
    private float answerPanelCloseTime;
    private float mainPanelOpenTime;
    private float mainPanelCloseTime;
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    void Start()
    {

        db = GameManager.Instance.GetComponent<DatabaseManager>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        firstPersonController = player.GetComponentInChildren<FirstPersonController>();

        sendAnswerButton.onClick.AddListener(OnSendAnswerButtonClicked);
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
            LogAnswerPanelDuration();

        }
        else
        {
            answerPanelOpenTime = Time.time;
            menuPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            menuAnimator.SetTrigger("Open");
        }
    }

    public void ToggleAnswerBox()
    {
        if (!menuPanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Locked;
            AnswerPanel.SetActive(false);
        }
        else
        {
            menuPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            AnswerPanel.SetActive(true);
            
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
        firstPersonController.DespawnAllInfoPanels();
        string plotType = GetSelectedButtonName();


        if (GetSelectedButtonName() == "Linegraph")
        {
            xTitle.text = "Time (days)";
            yTitle.text = "Values";
            zTitle.text = "Selected Columns";

            GameManager.Instance.GetComponent<CSVPlotter>().CalculateLineGraphPoints();

            GameManager.Instance.GetComponent<CSVPlotter>().LineGraphPlot();
        }
        else if (GetSelectedButtonName() == "Scatterplot")
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

            GameManager.Instance.GetComponent<CSVPlotter>().PlotData();
        } 
        else if (GetSelectedButtonName() == "Histogram")
        {

            GameManager.Instance.GetComponent<CSVPlotter>().columnXName = xDropdown.options[xDropdown.value].text;
            GameManager.Instance.GetComponent<CSVPlotter>().columnZName = zDropdown.options[zDropdown.value].text;

            xTitle.text = GameManager.Instance.GetComponent <CSVPlotter>().columnXName;
            zTitle.text = GameManager.Instance.GetComponent<CSVPlotter>().columnZName;
            yTitle.text = "Count";

            GameManager.Instance.GetComponent<CSVPlotter>().CalculateHistogramPlotPoints();
            GameManager.Instance.GetComponent<CSVPlotter>().GenerateHistogram();
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


    public void StartQuestionLoop()
    {
        if (questions.Count > 0)
        {
            currentQuestionIndex = 0;
            AskQuestion(questions[currentQuestionIndex]);
        }
        else
        {
            Debug.LogWarning("No questions to ask.");
        }
    }

    private void AskQuestion(string question)
    {
        questionText1.text = question;
        questionText2.text = question;
        answerInputField.text = "";
        questionBox.SetActive(true);
    }

    public void startQuestions()
    {
        StartQuestionLoop();
    }

    private void OnSendAnswerButtonClicked()
    {
        string currentQuestion = questions[currentQuestionIndex];
        string userAnswer = answerInputField.text;
        string currentPlotType = GetCurrentPlotType(); // Get the current plot type

        answers.Add(userAnswer);
        db.LogAnswer(currentPlotType, currentQuestion, userAnswer); // Pass the plot type to LogAnswer

        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Count)
        {
            AskQuestion(questions[currentQuestionIndex]);
        }
        else
        {
            ToggleAnswerBox(); // Close the answer panel and log the duration
            questionBox.SetActive(false);
            Debug.Log("All questions have been answered.");
        }
    }

    private string GetCurrentPlotType()
    {
        return GetSelectedButtonName(); // Assuming this returns the current plot type
    }

    private void LogAnswerPanelDuration()
    {
        float duration = answerPanelCloseTime - answerPanelOpenTime;
        string currentPlotType = GetCurrentPlotType();
        db.LogAnswerPanelDuration(currentPlotType, duration);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

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
    "Which is the best budget Graphics card?, Explain the relationship with 3D mark and the price",
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
    private Dictionary<string, float> plotStartTimes = new Dictionary<string, float>();
    private float questionStartTime;
    #endregion

    /// <summary>
    /// Interactions whereby happens once or in a rare occurances.
    /// </summary>
    #region Special Interactions
    [SerializeField]
    private Button nextButton;

    private IEnumerator EnableNextButtonAfterDelay(Button button, float delay)
    {
        yield return new WaitForSeconds(delay);
        button.interactable = true;
    }
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
        StartCoroutine(EnableNextButtonAfterDelay(nextButton, 5f));
    }

    #region Main Menu Controller

    /// <summary>
    /// Toggle the menu's visibility.
    /// </summary>
    public void ToggleMenu()
{
    Animator menuAnimator = menuPanel.GetComponent<Animator>();
        GameManager.Instance.PlaySound("Menu");

    if (menuPanel.activeSelf)
    {
        menuAnimator.SetTrigger("Close");
        Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(DeactivateAfterAnimation(menuAnimator, "Close"));
        mainPanelCloseTime = Time.time; // Set close time
        LogMainPanelDuration(); // Log the duration
    }
    else
    {
        mainPanelOpenTime = Time.time; // Set open time
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
        string columnInfo = GetColumnInfo(plotType);

        if (!db.IsPlotTimerRunning(plotType))
        {
            db.StartPlotTimer(plotType, columnInfo);
        }

        if (plotType == "Linegraph")
        {
            xTitle.text = "Time (days)";
            yTitle.text = "Values";
            zTitle.text = "Selected Columns";

            GameManager.Instance.GetComponent<CSVPlotter>().CalculateLineGraphPoints();
            GameManager.Instance.GetComponent<CSVPlotter>().LineGraphPlot();

            GameManager.Instance.ShowNotification("Linegraph plotted!");

        }
        else if (plotType == "Scatterplot")
        {
            GameManager.Instance.GetComponent<CSVPlotter>().columnXName = xDropdown.options[xDropdown.value].text;
            GameManager.Instance.GetComponent<CSVPlotter>().columnYName = yDropdown.options[yDropdown.value].text;
            GameManager.Instance.GetComponent<CSVPlotter>().columnZName = zDropdown.options[zDropdown.value].text;

            xTitle.text = GameManager.Instance.GetComponent<CSVPlotter>().columnXName;
            yTitle.text = GameManager.Instance.GetComponent<CSVPlotter>().columnYName;
            zTitle.text = GameManager.Instance.GetComponent<CSVPlotter>().columnZName;

            GameManager.Instance.GetComponent<CSVPlotter>().CalculateAllPlotPoints();
            GameManager.Instance.GetComponent<CSVPlotter>().UpdatePlotPointTexts();
            GameManager.Instance.GetComponent<CSVPlotter>().PlotData();

            GameManager.Instance.ShowNotification("Scatterplot plotted!");
        }
        else if (plotType == "Histogram")
        {
            GameManager.Instance.GetComponent<CSVPlotter>().columnXName = xDropdown.options[xDropdown.value].text;
            GameManager.Instance.GetComponent<CSVPlotter>().columnZName = zDropdown.options[zDropdown.value].text;

            xTitle.text = GameManager.Instance.GetComponent<CSVPlotter>().columnXName;
            zTitle.text = GameManager.Instance.GetComponent<CSVPlotter>().columnZName;
            yTitle.text = "Count";

            GameManager.Instance.GetComponent<CSVPlotter>().CalculateHistogramPlotPoints();
            GameManager.Instance.GetComponent<CSVPlotter>().GenerateHistogram();

            GameManager.Instance.ShowNotification("Histogram plotted!");
        }

        // Stop plot timer and log the time taken
        db.StopPlotTimer(plotType);

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
            Debug.Log("Finished.");
            questionText1.text = "FINISHED";
            questionText2.text = "FINISHED";
        }
    }

    private void AskQuestion(string question)
    {
        questionText1.text = question;
        questionText2.text = question;
        answerInputField.text = "";
        questionBox.SetActive(true);
        questionStartTime = Time.time; // Start timing when the question is asked
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

        float questionEndTime = Time.time; // End timing when the answer is submitted
        float timeTakenToAnswer = questionEndTime - questionStartTime;

        // Get the current data panels from the FirstPersonController
        List<string> dataPanels = firstPersonController.GetCurrentDataPanels();

        answers.Add(userAnswer);
        db.LogAnswer(currentPlotType, currentQuestion, userAnswer, timeTakenToAnswer, dataPanels); // Pass the exact data panel names to LogAnswer

        // Plot the currently open data panels
        PlotDataPanels(dataPanels);

        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Count)
        {
            AskQuestion(questions[currentQuestionIndex]);
        }
        else
        {
            questionBox.SetActive(false);
            GameManager.Instance.ShowNotification("All questions have been answered.");
            sendAnswerButton.interactable = false; 
        }
    }

    private void LogMainPanelDuration()
    {
        float duration = mainPanelCloseTime - mainPanelOpenTime;
        if (duration < 0) duration = 0; // Ensure non-negative duration
        string currentPlotType = GetCurrentPlotType();
        string currentCSV = GetCurrentCSV(); // You need to implement this method to retrieve the current CSV name
        db.LogUIDuration(currentPlotType, duration, currentCSV);
    }



    #region DB Methods 
    // Method to start the plot timer
    public void StartPlotTimer(string plotType)
    {
        plotStartTimes[plotType] = Time.time;
    }

    // Method to stop the plot timer and return the elapsed time
    public float StopPlotTimer(string plotType)
    {
        if (plotStartTimes.ContainsKey(plotType))
        {
            float startTime = plotStartTimes[plotType];
            float elapsedTime = Time.time - startTime;
            plotStartTimes.Remove(plotType);
            return elapsedTime;
        }
        return 0f;
    }

    private string GetColumnInfo(string plotType)
    {
        if (plotType == "Scatterplot" || plotType == "Histogram")
        {
            return $"X: {xDropdown.options[xDropdown.value].text}, Y: {yDropdown.options[yDropdown.value].text}, Z: {zDropdown.options[zDropdown.value].text}";
        }
        else if (plotType == "Linegraph")
        {
            return $"Columns: {string.Join(", ", GameManager.Instance.GetComponent<CSVPlotter>().selectedColumns)}";
        }
        return string.Empty;
    }
    #endregion

    #region DB Methods Fetching
    private string GetCurrentCSV()
    {
        CSVPlotter csvPlotter = GameManager.Instance.GetComponent<CSVPlotter>();
        if (csvPlotter != null && !string.IsNullOrEmpty(csvPlotter.currentCSV))
        {
            // Find the position of "Resources/" in the path
            int resourcesIndex = csvPlotter.currentCSV.IndexOf("Resources/");
            if (resourcesIndex != -1)
            {
                // Extract the part of the path after "Resources/"
                return csvPlotter.currentCSV.Substring(resourcesIndex + "Resources/".Length);
            }
        }
        return "No CSV Loaded";
    }

    // Method to plot data panels
    private void PlotDataPanels(List<string> dataPanels)
{
    foreach (string panel in dataPanels)
    {
        // Implement your plotting logic here
        // Example: db.PlotDataPanel(panel);
        Debug.Log("Plotting data panel: " + panel);
    }
}
    private string GetCurrentPlotType()
    {
        return GetSelectedButtonName(); // Assuming this returns the current plot type
    }
    #endregion

#if UNITY_EDITOR
    [CustomEditor(typeof(MenuManager)), InitializeOnLoadAttribute]
    public class MenuManagerEditor : Editor
    {
        MenuManager menuManager;
        SerializedObject serializedMenuManager;

        private void OnEnable()
        {
            menuManager = (MenuManager)target;
            serializedMenuManager = new SerializedObject(menuManager);
        }

        public override void OnInspectorGUI()
        {
            serializedMenuManager.Update();

            EditorGUILayout.Space();
            GUILayout.Label("Menu Manager", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
            GUILayout.Label("Custom Script by: Ilario Cutajar", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
            EditorGUILayout.Space();

            #region UI Panels Setup

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("UI Panels Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            menuManager.waitingUIPanel = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Waiting UI Panel", "UI Panel shown while waiting."), menuManager.waitingUIPanel, typeof(GameObject), true);
            menuManager.uploadedUIPanel = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Uploaded UI Panel", "UI Panel shown after uploading."), menuManager.uploadedUIPanel, typeof(GameObject), true);
            menuManager.menuPanel = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Menu Panel", "Main menu panel."), menuManager.menuPanel, typeof(GameObject), true);

            EditorGUILayout.Space();

            #endregion

            #region Dropdowns and Titles

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Dropdowns and Titles", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            menuManager.xDropdown = (TMP_Dropdown)EditorGUILayout.ObjectField(new GUIContent("X Dropdown", "Dropdown for X-axis."), menuManager.xDropdown, typeof(TMP_Dropdown), true);
            menuManager.yDropdown = (TMP_Dropdown)EditorGUILayout.ObjectField(new GUIContent("Y Dropdown", "Dropdown for Y-axis."), menuManager.yDropdown, typeof(TMP_Dropdown), true);
            menuManager.zDropdown = (TMP_Dropdown)EditorGUILayout.ObjectField(new GUIContent("Z Dropdown", "Dropdown for Z-axis."), menuManager.zDropdown, typeof(TMP_Dropdown), true);

            menuManager.xTitle = (TextMeshProUGUI)EditorGUILayout.ObjectField(new GUIContent("X Title", "Title for X-axis."), menuManager.xTitle, typeof(TextMeshProUGUI), true);
            menuManager.yTitle = (TextMeshProUGUI)EditorGUILayout.ObjectField(new GUIContent("Y Title", "Title for Y-axis."), menuManager.yTitle, typeof(TextMeshProUGUI), true);
            menuManager.zTitle = (TextMeshProUGUI)EditorGUILayout.ObjectField(new GUIContent("Z Title", "Title for Z-axis."), menuManager.zTitle, typeof(TextMeshProUGUI), true);

            EditorGUILayout.Space();

            #endregion

            #region Buttons and Panels

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Buttons and Panels", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
            menuManager.nextButton = (Button)EditorGUILayout.ObjectField(new GUIContent("Next Button", "Button to be enabled after a delay."), menuManager.nextButton, typeof(Button), true);
            EditorGUILayout.Space();

            SerializedProperty buttons = serializedMenuManager.FindProperty("buttons");
            EditorGUILayout.PropertyField(buttons, new GUIContent("Buttons", "Array of buttons."), true);

            menuManager.normalColor = EditorGUILayout.ColorField(new GUIContent("Normal Color", "Color for normal buttons."), menuManager.normalColor);
            menuManager.selectedColor = EditorGUILayout.ColorField(new GUIContent("Selected Color", "Color for selected buttons."), menuManager.selectedColor);

            SerializedProperty menuPanels = serializedMenuManager.FindProperty("MenuPanels");
            EditorGUILayout.PropertyField(menuPanels, new GUIContent("Menu Panels", "Array of menu panels."), true);

            menuManager.MainDropdown = (TMP_Dropdown)EditorGUILayout.ObjectField(new GUIContent("Main Dropdown", "Reference to the main dropdown."), menuManager.MainDropdown, typeof(TMP_Dropdown), true);

            menuManager.firstPersonController = (FirstPersonController)EditorGUILayout.ObjectField(new GUIContent("First Person Controller", "Reference to the first person controller."), menuManager.firstPersonController, typeof(FirstPersonController), true);

            EditorGUILayout.Space();

            #endregion

            #region Answer Panel Setup

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Answer Panel Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            menuManager.AnswerPanel = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Answer Panel", "Panel for displaying answers."), menuManager.AnswerPanel, typeof(GameObject), true);

            EditorGUILayout.Space();

            #endregion

            #region Question Box Setup

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Question Box Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            menuManager.questionBox = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Question Box", "Box for displaying questions."), menuManager.questionBox, typeof(GameObject), true);
            menuManager.questionText1 = (TextMeshProUGUI)EditorGUILayout.ObjectField(new GUIContent("Question Text 1", "Text for the first question display."), menuManager.questionText1, typeof(TextMeshProUGUI), true);
            menuManager.questionText2 = (TextMeshProUGUI)EditorGUILayout.ObjectField(new GUIContent("Question Text 2", "Text for the second question display."), menuManager.questionText2, typeof(TextMeshProUGUI), true);
            menuManager.answerInputField = (TMP_InputField)EditorGUILayout.ObjectField(new GUIContent("Answer Input Field", "Input field for the answer."), menuManager.answerInputField, typeof(TMP_InputField), true);
            menuManager.sendAnswerButton = (Button)EditorGUILayout.ObjectField(new GUIContent("Send Answer Button", "Button to send the answer."), menuManager.sendAnswerButton, typeof(Button), true);

            EditorGUILayout.Space();

            #endregion

            if (GUI.changed)
            {
                EditorUtility.SetDirty(menuManager);
                Undo.RecordObject(menuManager, "Menu Manager Change");
                serializedMenuManager.ApplyModifiedProperties();
            }
        }
    }
#endif
}

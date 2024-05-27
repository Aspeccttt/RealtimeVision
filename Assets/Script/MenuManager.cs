#region Unity Imports
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#endregion

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
        "Which is the cheapest Graphics card available in the dataset?",
        "Load the Steam dataset, Which game had the lowest Player Count in all of the data?",
        "On the first day, how much players were active on CSGO altogether?",
        "Which game had the worst fall off of average players on the 10th day?",
        "Load the CPU Dataset, What is the distribution of clock speed (GHz) for different CPU Cores?",
        "How is the Thermal Design Power (TDP) distributed among different CPUs",
        "What is the distribution of CPU cores across different CPUs Threads?"
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
    private float mainPanelOpenTime;
    private float mainPanelCloseTime;
    private Dictionary<string, float> plotStartTimes = new Dictionary<string, float>();
    private float questionStartTime;
    #endregion

    #region Special Interactions
    [SerializeField]
    private Button nextButton;

    private float originalWalkSpeed;

    /// <summary>
    /// Coroutine to enable a button after a delay.
    /// </summary>
    private IEnumerator EnableNextButtonAfterDelay(Button button, float delay)
    {
        yield return new WaitForSeconds(delay);
        button.interactable = true;
    }
    #endregion

    #region Unity Methods
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void Start()
    {
        db = GameManager.Instance.GetComponent<DatabaseManager>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        firstPersonController = player.GetComponentInChildren<FirstPersonController>();

        originalWalkSpeed = firstPersonController.walkSpeed; // Store the original walk speed

        sendAnswerButton.onClick.AddListener(OnSendAnswerButtonClicked);
        StartCoroutine(EnableNextButtonAfterDelay(nextButton, 5f));
    }
    #endregion

    #region Main Menu Controller

    /// <summary>
    /// Toggle the menu's visibility.
    /// </summary>
    public void ToggleMenu()
    {
        if (AnswerPanel.activeSelf)
        {
            Debug.Log("Close the AnswerBox before opening the menu.");
            return;
        }

        Animator menuAnimator = menuPanel.GetComponent<Animator>();
        GameManager.Instance.PlaySound("Menu");

        if (menuPanel.activeSelf)
        {
            menuAnimator.SetTrigger("Close");
            Cursor.lockState = CursorLockMode.Locked;
            StartCoroutine(DeactivateAfterAnimation(menuAnimator, "Close"));
            mainPanelCloseTime = Time.time; // Set close time
            LogMainPanelDuration(); // Log the duration
            SetPlayerMovementSpeed(originalWalkSpeed); // Restore original speed
        }
        else
        {
            mainPanelOpenTime = Time.time; // Set open time
            menuPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            menuAnimator.SetTrigger("Open");
            SetPlayerMovementSpeed(0f); // Set speed to 0
        }
    }

    /// <summary>
    /// Toggle the answer box's visibility.
    /// </summary>
    public void ToggleAnswerBox()
    {
        if (menuPanel.activeSelf)
        {
            Debug.Log("Close the menu before opening the AnswerBox.");
            return;
        }

        // Check the state of the AnswerPanel and toggle its visibility
        if (AnswerPanel.activeSelf)
        {
            AnswerPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            SetPlayerMovementSpeed(originalWalkSpeed); // Restore original speed
        }
        else
        {
            AnswerPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            SetPlayerMovementSpeed(0f); // Set speed to 0
        }
    }

    /// <summary>
    /// Coroutine to deactivate a GameObject after an animation.
    /// </summary>
    private IEnumerator DeactivateAfterAnimation(Animator animator, string animation)
    {
        float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationDuration);

        if (animation == "Close")
        {
            menuPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Handle actions after CSV is loaded.
    /// </summary>
    public void CSVLoaded()
    {
        waitingUIPanel.SetActive(false);
        uploadedUIPanel.SetActive(true);
    }

    /// <summary>
    /// Populate dropdowns with column names.
    /// </summary>
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

    /// <summary>
    /// Handle Done button click.
    /// </summary>
    public void OnDoneButtonClicked()
    {
        firstPersonController.DespawnAllInfoPanels();
        string plotType = GetSelectedButtonName();
        string columnInfo = GetColumnInfo(plotType);

        // Start the plot timer and increment the plot count
        db.StartPlotTimer(plotType, columnInfo);

        if (plotType == "Linegraph")
        {
            xTitle.text = "Time (days)";
            yTitle.text = "Values";
            zTitle.text = "Selected Columns";

            GameManager.Instance.GetComponent<CSVPlotter>().CalculateLineGraphPoints();
            GameManager.Instance.GetComponent<CSVPlotter>().LineGraphPlot();
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

    /// <summary>
    /// Handle button click in the menu.
    /// </summary>
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

        // Ensure the AnswerPanel is hidden when a button is clicked in the menu
        if (AnswerPanel.activeSelf)
        {
            AnswerPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Get the name of the selected button.
    /// </summary>
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

    #region Question Handling

    /// <summary>
    /// Start the question loop.
    /// </summary>
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

    /// <summary>
    /// Ask the current question.
    /// </summary>
    private void AskQuestion(string question)
    {
        questionText1.text = question;
        questionText2.text = question;
        answerInputField.text = "";
        questionBox.SetActive(true);
        questionStartTime = Time.time; // Start timing when the question is asked
    }

    /// <summary>
    /// Start the questions.
    /// </summary>
    public void startQuestions()
    {
        StartQuestionLoop();
    }

    /// <summary>
    /// Set the player's movement speed.
    /// </summary>
    public void SetPlayerMovementSpeed(float speed)
    {
        firstPersonController.walkSpeed = speed;
        firstPersonController.sprintSpeed = speed; // Also set sprint speed to 0 if needed
    }

    /// <summary>
    /// Handle Send Answer button click.
    /// </summary>
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
    #endregion

    #region Logging

    /// <summary>
    /// Log the duration the main panel was open.
    /// </summary>
    private void LogMainPanelDuration()
    {
        float duration = mainPanelCloseTime - mainPanelOpenTime;
        if (duration < 0) duration = 0; // Ensure non-negative duration
        string currentPlotType = GetCurrentPlotType();
        string currentCSV = GetCurrentCSV(); // You need to implement this method to retrieve the current CSV name
        db.LogUIDuration(currentPlotType, duration, currentCSV);
    }
    #endregion

    #region DB Methods

    /// <summary>
    /// Start the plot timer.
    /// </summary>
    public void StartPlotTimer(string plotType)
    {
        plotStartTimes[plotType] = Time.time;
    }

    /// <summary>
    /// Stop the plot timer and return the elapsed time.
    /// </summary>
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

    /// <summary>
    /// Get column information for a plot type.
    /// </summary>
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

    /// <summary>
    /// Get the current CSV name.
    /// </summary>
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

    /// <summary>
    /// Plot data panels.
    /// </summary>
    private void PlotDataPanels(List<string> dataPanels)
    {
        foreach (string panel in dataPanels)
        {
            // Implement your plotting logic here
            // Example: db.PlotDataPanel(panel);
            Debug.Log("Plotting data panel: " + panel);
        }
    }

    /// <summary>
    /// Get the current plot type.
    /// </summary>
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

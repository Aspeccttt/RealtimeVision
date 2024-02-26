#region Unity Imports
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#endregion


public class GameManager : MonoBehaviour
{
    #region Global Variables

    //UI
    public GameObject waitingUIPanel;
    public GameObject uploadedUIPanel;
    public GameObject menuPanel;

    public bool IsCSVUploaded = false;


    [SerializeField] TextMeshProUGUI menuCondition;
    #endregion

    #region Singleton
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Check if there are any other instances conflicting
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Unity Lifecycle
    void Update()
    {
        // Check for the ESC key press to toggle the menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }

        if (IsCSVUploaded == true)
        {
            CSVLoaded();
        }
    }

    private void Start()
    {
        if (!IsCSVUploaded)
        {
            menuCondition.text = "No uploaded CSV, please upload by pressing the button!";
        }
        else { menuCondition.text = ""; }
    }
    #endregion

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
    #endregion

    #region SceneWorld Scripts

    #endregion

}

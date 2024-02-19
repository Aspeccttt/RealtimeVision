using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI xUI;
    [SerializeField]
    private TextMeshProUGUI yUI;
    [SerializeField]
    private TextMeshProUGUI zUI;

    //UI
    public GameObject waitingUIPanel;
    public GameObject uploadedUIPanel;
    public GameObject menuPanel;


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
    void Update()
    {
        // Check for the ESC key press to toggle the menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    /// <summary>
    /// Toggle the menu's visibility.
    /// </summary>
    public void ToggleMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    /// <summary>
    /// These methods are just to update the text for the UI.
    /// </summary>
    /// <param name="text"></param>
    public void UpdateZText(string text)
    {
        if (zUI != null)
        {
            zUI.text = text;
        }
    }

    public void UpdateYText(string text)
    {
        if (yUI != null)
        {
            yUI.text = text;
        }
    }

    public void UpdateXText(string text)
    {
        if (xUI != null)
        {
            xUI.text = text;
        }
    }

    public void CSVLoaded()
    {
        waitingUIPanel.SetActive(false);
        uploadedUIPanel.SetActive(true);
    }
}

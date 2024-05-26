#region Unity Imports
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#endregion


public class GameManager : MonoBehaviour
{
    #region Global Variables
    public bool IsCSVUploaded = false;
    public List<Dictionary<string, object>> csvData;
    #endregion

    #region Notifications
    public GameObject notificationPrefab;
    public Canvas uiCanvas; // Assign your Canvas in the Inspector

    public void ShowNotification(string message)
    {
        // Instantiate the prefab
        PlaySound("Notification", 0.1f);
        GameObject notificationInstance = Instantiate(notificationPrefab, uiCanvas.transform);

        // Find the TextMeshProUGUI component in the instantiated prefab
        TextMeshProUGUI textComponent = notificationInstance.GetComponentInChildren<TextMeshProUGUI>();

        // Set the text to the provided message
        if (textComponent != null)
        {
            textComponent.text = message;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in children of the notification prefab.");
        }

        // Optionally, start the animation if it's not set to play automatically
        Animator animator = notificationInstance.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Show"); // Assumes you have a trigger parameter named "Show"
        }

        Destroy(notificationInstance, 3f);
    }
    #endregion

    #region Audio
    private AudioSource audioSource;

    public void PlaySound(string soundName, float volume = 1f)
    {
        // Load the audio clip from the "Sounds" folder
        AudioClip clip = Resources.Load<AudioClip>("Sounds/" + soundName);
        if (clip != null)
        {
            audioSource.volume = 0.2f;
            audioSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogError("Sound not found: " + soundName);
        }
    }
    #endregion


    #region Singleton
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize the CSV data list
        csvData = new List<Dictionary<string, object>>();

        // Initialize the objectives/questions
        Instance.GetComponent<MenuManager>().StartQuestionLoop();

        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        ShowNotification("Welcome! Press ESC to start.");


    }
    #endregion

    // Method to store CSV data
    public void StoreCSVData(List<Dictionary<string, object>> data)
    {
        csvData = data;
        IsCSVUploaded = true;
    }

    // Method to retrieve CSV data
    public List<Dictionary<string, object>> GetCSVData()
    {
        return csvData;
    }

}

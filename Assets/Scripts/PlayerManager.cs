using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public string nick = "nick";
    [SerializeField] TMP_InputField nickInputField;
    public Material playerMaterial;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject titlePanel;

    public int botLevel = 0; // Default bot level
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
        // Initialize player material if not set
        playerMaterial = GetComponent<SettingsManager>().playerMaterials[0];
    }

    void Update()
    {
        nick = nickInputField.text;
    }

    public void OnStartButtonClicked()
    {
        nick = nickInputField.text;
        SceneManager.LoadScene("GameScene");
    }

    public void OnSettingsButtonClicked()
    {
        titlePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnSettingsCloseButtonClicked()
    {
        titlePanel.SetActive(true);
        settingsPanel.SetActive(false);
    }



}


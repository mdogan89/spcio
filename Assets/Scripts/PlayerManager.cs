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
    [SerializeField] TextMeshProUGUI highScoreText;

    public int botLevel = 0; // Default bot level
    public bool isCube = false; // Default player shape

    public int highScore = 0; // High score for the player
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

        // Load the player's nickname from PlayerPrefs if it exists
        if (PlayerPrefs.HasKey("PlayerNick"))
        {
            nick = PlayerPrefs.GetString("PlayerNick");
            nickInputField.text = nick; // Set the input field text to the loaded nickname
        }
        else
        {
            nick = "Player" + Random.Range(1000, 9999); // Assign a default nickname if none is set
        }
        // Load the high score from PlayerPrefs if it exists
        if (PlayerPrefs.HasKey("HighScore"))
        {
            highScore = PlayerPrefs.GetInt("HighScore");
            highScoreText.text = nick + " : " + highScore; // Update the high score text
        }
        else
        {
            highScore = 0; // Default high score if none exists
            highScoreText.text = nick + " : " + highScore;
        }
    }

    void Update()
    {
        if(nickInputField != null)
        {
            nick = nickInputField.text; // Update the input field with the current nickname
        }
    }

    public void OnStartButtonClicked()
    {
        nick = nickInputField.text;
        SceneManager.LoadScene("GameScene");
        PlayerPrefs.SetString("PlayerNick", nick); // Save the player's nickname
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


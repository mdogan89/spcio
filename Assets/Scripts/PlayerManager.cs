using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public string nick = "nick";
    [SerializeField] TMP_InputField nickInputField;
    public int skinId = 0; // Default skin ID
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject titlePanel;
    [SerializeField] TextMeshProUGUI highScoreText;
    public int mapId = 0; // Default map ID

    public int botLevel = 0; // Default bot level
    public bool isCube = false; // Default player shape

    public int highScore = 0; // High score for the player

    public bool easyControls = true; // Default control scheme

    public bool thirdPersonView = true; // Default camera view

    public string lastNick = "Nick";

    public string hsNick = "Nick";

    public bool fogEnabled = true; // Default fog setting

    public bool trailerEnabled = true; // Default trailer setting

    public bool starsEnabled = true; // Default stars setting

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

        // Load the player's nickname from PlayerPrefs if it exists
        if (PlayerPrefs.HasKey("LastNick"))
        {
            lastNick = PlayerPrefs.GetString("LastNick");
            nick = lastNick; // Set the nickname to the last used nickname
            nickInputField.text = nick; // Set the input field text to the loaded nickname
        }
        else
        {
            nick = "Player" + Random.Range(1000, 9999); // Assign a default nickname if none is set
        }
        // Load the high score from PlayerPrefs if it exists

        if (PlayerPrefs.HasKey("hsNick"))
        {
            hsNick = PlayerPrefs.GetString("hsNick");
        }
        else
        {
            hsNick = "Nick"; // Default high score nickname
        }

        if (PlayerPrefs.HasKey(hsNick))
        {
            highScore = PlayerPrefs.GetInt(hsNick);
            highScoreText.text = hsNick + " : " + highScore; // Update the high score text
        }
        else
        {
            highScore = 0; // Default high score if none exists
            highScoreText.text = hsNick + " : " + highScore;
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
        lastNick = nick;
        SceneManager.LoadScene("GameScene");
        PlayerPrefs.SetString("LastNick", lastNick); // Save the player's nickname
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


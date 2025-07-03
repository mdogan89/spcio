using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public string nick = "nick";
    [SerializeField] TMP_InputField nickInputField;
    public int skinId; // Default skin ID
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject titlePanel;
    [SerializeField] TextMeshProUGUI highScoreText;
    [SerializeField] TMP_Dropdown gameModeDropdown; // Dropdown for selecting game mode


    public int mapId; // Default map ID

    public int botLevel = 0; // Default bot level
    public bool isCube = false; // Default player shape

    public int highScore = 0; // High score for the player

    public bool easyControls; // Default control scheme

    public bool thirdPersonView; // Default camera view

    public string lastNick = "Nick";

    public string hsNick = "Nick";

    public bool fogEnabled; // Default fog setting

    public bool trailerEnabled; // Default trailer setting

    public bool starsEnabled; // Default stars setting

    public int gameMode = 0; // Default game mode

    public int numberOfBots; // Number of bots in the game

    public int numberOfFood; // Number of food items in the game

    public float spawnRadius; // Default spawn radius

    public float exposure; // Default exposure value for the skybox

    public float lookSensitivity; // Sensitivity for camera rotation

    public float moveSensitivity; // Sensitivity for movement

    public bool vibration = true; // Default vibration setting

    public float timer;

    public Color skinColor;

    public float volume;
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

        LoadSettings();
    }

    void Update()
    {
        if (nickInputField != null)
        {
            nick = nickInputField.text; // Update the input field with the current nickname
        }
        if (SceneManager.GetActiveScene().name == "Title")
        {
            RenderSettings.skybox.SetFloat("_Rotation", -Time.time * 0.3f); // Rotate the skybox over time in the title scene
        }
        if(Camera.main != null)
        {
            Camera.main.GetComponent<AudioSource>().volume = volume / 5; // Set the audio source volume based on the saved volume level
        }
    }

    public void OnStartButtonClicked()
    {
        nick = nickInputField.text;
        lastNick = nick;
        PlayerPrefs.SetString("LastNick", lastNick); // Save the player's nickname
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

    public void OnGameModeSelected(int mode)
    {
        gameMode = mode; // Set the selected game mode
        PlayerPrefs.SetInt("GameMode", gameMode); // Save the game mode to PlayerPrefs
    }

    public void OnHowToPlayClicked()
    {
        SceneManager.LoadScene("HowToPlay"); // Load the How To Play scene
    }

    void LoadSettings()
    {
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
        if (PlayerPrefs.HasKey("SkinId"))
        {
            skinId = PlayerPrefs.GetInt("SkinId");
        }
        else
        {
            skinId = 0;
        }
        if (PlayerPrefs.HasKey("MapId"))
        {
            mapId = PlayerPrefs.GetInt("MapId");
        }
        else
        {
            mapId = 0; // Default map ID
        }
        if (PlayerPrefs.HasKey("BotLevel"))
        {
            botLevel = PlayerPrefs.GetInt("BotLevel");
        }
        else
        {
            botLevel = 0; // Default bot level
        }
        if (PlayerPrefs.HasKey("NumberOfBots"))
        {
            numberOfBots = PlayerPrefs.GetInt("NumberOfBots");
        }
        else
        {
            numberOfBots = 50; // Default number of bots
        }
        if (PlayerPrefs.HasKey("NumberOfFood"))
        {
            numberOfFood = PlayerPrefs.GetInt("NumberOfFood");
        }
        else
        {
            numberOfFood = 50; // Default number of food items
        }
        if (PlayerPrefs.HasKey("SpawnRadius"))
        {
            spawnRadius = PlayerPrefs.GetFloat("SpawnRadius");
        }
        else
        {
            spawnRadius = 50f; // Default spawn radius
        }
        if (PlayerPrefs.HasKey("ThirdPersonView"))
        {
            thirdPersonView = PlayerPrefs.GetInt("ThirdPersonView") == 1; // Convert int to bool
        }
        else
        {
            thirdPersonView = true; // Default camera view
        }
        if (PlayerPrefs.HasKey("FogEnabled"))
        {
            fogEnabled = PlayerPrefs.GetInt("FogEnabled") == 1; // Convert int to bool
        }
        else
        {
            fogEnabled = true; // Default fog setting
        }
        if (PlayerPrefs.HasKey("TrailerEnabled"))
        {
            trailerEnabled = PlayerPrefs.GetInt("TrailerEnabled") == 1; // Convert int to bool
        }
        else
        {
            trailerEnabled = true; // Default trailer setting
        }
        if (PlayerPrefs.HasKey("StarsEnabled"))
        {
            starsEnabled = PlayerPrefs.GetInt("StarsEnabled") == 1; // Convert int to bool
        }
        else
        {
            starsEnabled = true; // Default stars setting
        }
        if (PlayerPrefs.HasKey("EasyControls"))
        {
            easyControls = PlayerPrefs.GetInt("EasyControls") == 1; // Convert int to bool
        }
        else
        {
            easyControls = true; // Default control scheme
        }
        if (PlayerPrefs.HasKey("Brightness"))
        {
            exposure = PlayerPrefs.GetFloat("Brightness");
        }
        else
        {
            exposure = 3.0f; // Default exposure value for the skybox
        }
        if (PlayerPrefs.HasKey("LookSensitivity"))
        {
            lookSensitivity = PlayerPrefs.GetFloat("LookSensitivity");
        }
        else
        {
            lookSensitivity = 1.0f; // Default camera rotation sensitivity
        }
        if (PlayerPrefs.HasKey("MoveSensitivity"))
        {
            moveSensitivity = PlayerPrefs.GetFloat("MoveSensitivity");
        }
        else
        {
            moveSensitivity = 1.0f; // Default movement sensitivity
        }
        if (PlayerPrefs.HasKey("GameMode"))
        {
            gameMode = PlayerPrefs.GetInt("GameMode");
            gameModeDropdown.value = gameMode; // Set the dropdown value to the saved game mode
        }
        else
        {
            gameMode = 0; // Default game mode
            gameModeDropdown.value = 0; // Set the dropdown value to the default game mode
        }
        if (PlayerPrefs.HasKey("Vibration"))
        {
            vibration = PlayerPrefs.GetInt("Vibration") == 1; // Convert int to bool
        }
        else
        {
            vibration = true; // Default vibration setting
        }
        if (PlayerPrefs.HasKey("Timer"))
        {
            timer = PlayerPrefs.GetFloat("Timer");
        }
        else
        {
            timer = 120f; // Default timer value
        }
        if (PlayerPrefs.HasKey("SkinColor"))
        {
            string colorString = "#" + PlayerPrefs.GetString("SkinColor");
            ColorUtility.TryParseHtmlString(colorString, out skinColor); // Parse the saved color string            
        }
        else
        {
            skinColor = Color.red; // Default
        }
        if(PlayerPrefs.HasKey("Volume"))
        {
            volume = PlayerPrefs.GetFloat("Volume");
        }
        else
        {
            volume = 0.3f; // Default volume level
        }
    }
}


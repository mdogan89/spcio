using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; set; }
    public string nick;
    public TMP_InputField nickInputField;
    public int skinId; // Default skin ID
    public GameObject settingsPanel;
    public GameObject titlePanel;
    public TextMeshProUGUI highScoreText;
    public TMP_Dropdown gameModeDropdown; // Dropdown for selecting game mode
    public bool showAds = true; // Default ad visibility setting

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

    public bool music;

    public bool easyFood;

    public int musicId;

    public AudioClip[] musicAudios; // Array of audio sources for different sounds

    public Canvas pauseMenuCanvas; // Reference to the pause menu canvas

    public bool powerup;

    Toggle musicToggle; // Toggle for enabling/disabling music
    Toggle vibrationToggle; // Toggle for enabling/disabling vibration
    Toggle camToggle; // Toggle for switching camera view
    Slider volumeSlider; // Slider for adjusting volume
    Slider brightnessSlider; // Slider for adjusting brightness
    TMP_Dropdown musicDropdown;
    public Button multiplayerButton; // Inspector'dan atayabilirsin

    public Material[] skins;

    public bool dailyPotionClaimed = true;
    public bool dailyPotionActivated = false;

    [SerializeField] Button dailyPotionActiveButton;
    [SerializeField] GameObject dailyPotionParent;
    [SerializeField] Button pauseButton;

    public float potionTimer = 0f;
    public TextMeshProUGUI potionTimerText;


    public GameObject shopPanel;

    public bool adsRemoved = false;

    public bool dailyPotionShowed = false;

    void Awake()
    {
#if UNITY_SERVER
        SceneManager.LoadScene("Multiplayer");
#endif

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
#if UNITY_WEBGL
        Camera.main.GetComponent<AudioSource>().Pause(); // Play the main camera's audio source for WebGL builds
#endif

        LoadSettings();

        //if(nickInputField.text != "" || nickInputField.text != string.Empty)
        //    nick = nickInputField.text ; // Set the input field text to the loaded nickname


        if (Camera.main != null && Camera.main.GetComponent<AudioSource>() != null)
        {
            Camera.main.GetComponent<AudioSource>().volume = volume / 5; // Set the audio source volume based on the saved volume level
        }

       

        if (PlayerPrefs.HasKey("potionTimer"))
        {
            if (PlayerPrefs.GetFloat("potionTimer") > 0)
            {
                potionTimer = PlayerPrefs.GetFloat("potionTimer"); // Load the current potion timer value
            }
        }
        else
        {
            potionTimer = 0f; // Default potion timer value
        }

        dailyPotionShowed = PlayerPrefs.GetInt("dailyPotionShowed", 0) == 1;

    }
    private void Start()
    {
        if (Camera.main != null && Camera.main.GetComponent<AudioSource>() != null)
        {
            Camera.main.GetComponent<AudioSource>().volume = volume / 5; // Set the audio source volume based on the saved volume level
        }
        CheckInternetAndSetMultiplayerButton();

       

    }
    void Update()
    {
        if(!dailyPotionShowed && SceneManager.GetActiveScene().buildIndex == 0 && !adsRemoved && ShopScript.receiptChecked)
        {
            SetDailyPotion();
            dailyPotionShowed = true;
            PlayerPrefs.SetInt("dailyPotionShowed", 1);
        }
        else if(SceneManager.GetActiveScene().buildIndex == 0 && adsRemoved && ShopScript.receiptChecked)
        {
            dailyPotionParent.SetActive(false);
            dailyPotionShowed = true;
            PlayerPrefs.SetInt("dailyPotionShowed", 1);
            showAds = false;
            dailyPotionActivated = false;
            dailyPotionClaimed = false;
            PlayerPrefs.SetInt("dailyPotionClaimed", 0);
            PlayerPrefs.SetInt("smallPotions", 0);
        }

        //if(Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().buildIndex != 0)
        //{
        //    if (pauseMenuCanvas != null)
        //    {
        //        pauseMenuCanvas.enabled = !pauseMenuCanvas.enabled; // Toggle the pause menu visibility
        //    }
        //}
        if (music && Camera.main != null)
        {
            Camera.main.GetComponent<AudioSource>().resource = musicAudios[musicId]; // Set the audio source to the selected music
            if (!Camera.main.GetComponent<AudioSource>().isPlaying)
            {
                Camera.main.GetComponent<AudioSource>().Play(); // Play the audio source if music is enabled
            }
        }

        //if (nickInputField != null && nickInputField.text != "")
        //{
        //    nick = nickInputField.text; // Update the input field with the current nickname
        //}
        if (SceneManager.GetActiveScene().buildIndex == 0 || SceneManager.GetActiveScene().buildIndex == 3)
        {
            RenderSettings.skybox.SetFloat("_Rotation", -Time.time * 0.3f); // Rotate the skybox over time in the title scene
        }
        if (Camera.main != null)
        {
            Camera.main.GetComponent<AudioSource>().volume = volume / 5; // Set the audio source volume based on the saved volume level
        }
        if (!music && Camera.main != null)
            Camera.main.GetComponent<AudioSource>().Pause(); // Play the main camera's audio source if music is enabled
        else if (music && Camera.main != null)
            Camera.main.GetComponent<AudioSource>().UnPause(); // Pause the main camera's audio source if music is disabled
        PauseMenu();

        CheckInternetAndSetMultiplayerButton();



        if (potionTimer > 0)
        {
            if (dailyPotionActivated) { 
                dailyPotionActiveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Activated(" + potionTimer.ToString("F0") + "s)";
                dailyPotionActiveButton.interactable = false;
            }
            potionTimer -= Time.deltaTime;
            showAds = false;
        }
        else if (potionTimer <= 0)
        {
            if(dailyPotionActivated)
            {
                dailyPotionActiveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ended";
                dailyPotionActivated = false;
                //dailyPotionClaimed = true;
                dailyPotionActiveButton.interactable = false;
                //PlayerPrefs.SetInt("dailyPotionClaimed", 1);
            }
            showAds = true;
            potionTimer = 0;
        }
        else if (potionTimer <= 0 )
        {
            if ( SceneManager.GetActiveScene().buildIndex == 0 && !dailyPotionActivated ) { 
            dailyPotionActiveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Activate";
            }
            potionTimer = 0;
            showAds = true;
        }

        if (potionTimerText != null)
        {
            if (ShopScript.showAds)
            {
                if (potionTimer > 0)
                {
                    potionTimerText.text = potionTimer.ToString("F0");
                }
                else
                {
                    potionTimerText.text = "000";
                }
            }
            else
            {
                potionTimerText.text = "999";
            }
        }

        if ( SceneManager.GetActiveScene().buildIndex == 0 )
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("dailyPotionParent clicked");
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RectTransform rectTransform = dailyPotionActiveButton.GetComponent<RectTransform>();

                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
                {
                    Debug.Log("dailyPotionActiveButton clicked");
                    if (!dailyPotionActivated)
                    {
                        OnDailyPotionActivated();
                    }
                }
                else
                {
                    OnDailyPotionClosed();
                }

            }
        }
    }

    void SetDailyPotion()
    {
        if (PlayerPrefs.HasKey("lastLoginDate"))
        {
            string lastLoginDate = PlayerPrefs.GetString("lastLoginDate");
            string currentDate = System.DateTime.Now.ToString("yyyyMMdd");
            if (lastLoginDate != currentDate)
            {
                dailyPotionClaimed = true; // Reset the daily potion claim status if the date has changed
                PlayerPrefs.SetInt("dailyPotionClaimed", 1);
                int smallPotions = PlayerPrefs.GetInt("smallPotions");
                smallPotions += 1; // Add one small potion for daily login
                PlayerPrefs.SetInt("smallPotions", smallPotions);
            }
            else
            {
                dailyPotionClaimed = false;
                PlayerPrefs.SetInt("dailyPotionClaimed", 0);
            }
        }
        else
        {
            dailyPotionClaimed = true; // No previous login date, so reset the daily potion claim status
            PlayerPrefs.SetInt("dailyPotionClaimed", 1);
            int smallPotions = PlayerPrefs.GetInt("smallPotions");
            smallPotions += 1; // Add one small potion for daily login
            PlayerPrefs.SetInt("smallPotions", smallPotions);
        }
        if (PlayerPrefs.GetInt("dailyPotionClaimed") == 1)
        {
            dailyPotionParent.SetActive(true);
        }
        else
        {
            dailyPotionParent.SetActive(false);
        }
    }

    //public void OnSongChanged(int value)
    //{
    //    musicId = value; // Update the music ID based on the dropdown selection
    //    PlayerPrefs.SetInt("MusicId", musicId); // Save the selected music ID to PlayerPrefs
    //    if (Camera.main != null && Camera.main.GetComponent<AudioSource>() != null)
    //    {
    //        Camera.main.GetComponent<AudioSource>().resource = musicAudios[musicId];
    //        Camera.main.GetComponent<AudioSource>().Play(); // Play the selected music        
    //    }
    //}


    public void OnStartButtonClicked()
    {
        if(nickInputField.text == "" || nickInputField.text == string.Empty)
        {
            nick = "Player" + Random.Range(1000, 9999); // Assign a default nickname if none is set
            nickInputField.text = nick; // Set the input field text to the default nickname
            Debug.Log("OnStartButtonClicked, no nick, assigned default: " + nick);
        }
        else
           nick = nickInputField.text;
         
        lastNick = nick;
        PlayerPrefs.SetString("LastNick", lastNick); // Save the player's nickname
        SceneManager.LoadScene(1);
        Time.timeScale = 1f; // Ensure the game runs at normal speed when starting
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
        SceneManager.LoadScene(2); // Load the How To Play scene
    }

    public void OnCreditsButtonClicked()
    {
        SceneManager.LoadScene(3); // Load the Credits scene
    }

    public void OnMultiplayerButtonClicked()
    {
        if (nickInputField.text == "" || nickInputField.text == string.Empty)
        {
            nick = "Player" + Random.Range(1000, 9999); // Assign a default nickname if none is set
            nickInputField.text = nick; // Set the input field text to the default nickname
            Debug.Log("OnStartButtonClicked, no nick, assigned default: " + nick);
        }
        else
            nick = nickInputField.text;

        lastNick = nick;
        PlayerPrefs.SetString("LastNick", lastNick); // Save the player's nickname
        Debug.Log("OnMultiplayerButtonClicked" + nick);
        SceneManager.LoadScene(4); // Load the Multiplayer scene
    }

    public void OnShopButtonClicked()
    {
        titlePanel.SetActive(false);
        shopPanel.SetActive(true);
    }

    public void OnShopCloseButtonClicked()
    {
        titlePanel.SetActive(true);
        shopPanel.SetActive(false);
    }


    public void OnDailyPotionActivated()
    {
        int smallPotions = PlayerPrefs.GetInt("smallPotions");
        if (smallPotions > 0) {
            smallPotions -= 1; // Add one small potion for daily login
            PlayerPrefs.SetInt("smallPotions", smallPotions);
            dailyPotionActivated = true;
        dailyPotionClaimed = false; // Mark the daily potion as claimed
        PlayerPrefs.SetInt("dailyPotionClaimed", 0);
        potionTimer += 300f;
        potionTimer -= Time.deltaTime;
       
        
    }
        else {             Debug.Log("No small potions available to activate.");
        }
    }

    public void OnDailyPotionClosed()
    {
        dailyPotionParent.SetActive(false);
    }



    void PauseMenu()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            pauseMenuCanvas = GameObject.Find("PauseMenuCanvas")?.GetComponent<Canvas>(); // Find the pause menu canvas in the current scene
            if (pauseMenuCanvas != null)
            {
                //set volume slider
                volumeSlider = pauseMenuCanvas.GetComponentInChildren<Slider>();
                if (volumeSlider != null)
                {
                    volumeSlider.value = volume; // Set the slider value to the saved volume level
                    volumeSlider.onValueChanged.AddListener(value =>
                    {
                        volume = value; // Update the volume when the slider value changes
                        PlayerPrefs.SetFloat("Volume", volume); // Save the volume to PlayerPrefs
                        if (Camera.main != null && Camera.main.GetComponent<AudioSource>() != null)
                        {
                            Camera.main.GetComponent<AudioSource>().volume = volume / 5; // Adjust the audio source volume
                        }
                    });
                }
                //Set music toggle
                musicToggle = pauseMenuCanvas.GetComponentInChildren<Toggle>();
                if (musicToggle != null)
                {
                    musicToggle.isOn = music; // Set the toggle state based on the music setting
                    musicToggle.onValueChanged.AddListener(value =>
                    {
                        music = value; // Update the music setting
                        PlayerPrefs.SetInt("Music", music ? 1 : 0); // Save the music setting to PlayerPrefs
                        if (Camera.main != null && Camera.main.GetComponent<AudioSource>() != null)
                        {
                            if (music)
                            {
                                Camera.main.GetComponent<AudioSource>().Play(); // Play the audio source if music is enabled
                            }
                            else
                            {
                                Camera.main.GetComponent<AudioSource>().Pause(); // Pause the audio source if music is disabled
                            }
                        }
                    });
                }
               // Set the music dropdown
                musicDropdown = pauseMenuCanvas.GetComponentInChildren<TMP_Dropdown>();
                if (musicDropdown != null)
                {
                    musicDropdown.value = musicId;
                    musicDropdown.onValueChanged.AddListener(value =>
                    {
                        musicId = value; // Update the music ID based on the dropdown selection
                        PlayerPrefs.SetInt("MusicId", musicId); // Save the selected music ID to PlayerPrefs
                        if (Camera.main != null && Camera.main.GetComponent<AudioSource>() != null)
                        {
                            Camera.main.GetComponent<AudioSource>().clip = musicAudios[musicId]; // Set the audio source to the selected music
                            Camera.main.GetComponent<AudioSource>().Play(); // Play the selected music
                        }
                    });
                }
                // Set the vibration toggle
                vibrationToggle = pauseMenuCanvas.GetComponentsInChildren<Toggle>()[1];
                    if (vibrationToggle != null)
                    {
                        vibrationToggle.isOn = vibration; // Set the toggle state based on the vibration setting
                        vibrationToggle.onValueChanged.AddListener(value =>
                        {
                            vibration = value; // Update the vibration setting
                            PlayerPrefs.SetInt("Vibration", vibration ? 1 : 0); // Save the vibration setting to PlayerPrefs
                        });
                    }
                    // Set camera view toggle
                    camToggle = pauseMenuCanvas.GetComponentsInChildren<Toggle>()[2];
                    if (camToggle != null)
                    {
                        camToggle.isOn = thirdPersonView; // Set the toggle state based on the camera view setting
                        camToggle.onValueChanged.AddListener(value =>
                        {
                            thirdPersonView = value; // Update the camera view setting
                            PlayerPrefs.SetInt("ThirdPersonView", thirdPersonView ? 1 : 0); // Save the camera view setting to PlayerPrefs
                        });
                    }
                    brightnessSlider = pauseMenuCanvas.GetComponentsInChildren<Slider>()[1];
                    if (brightnessSlider != null)
                    {
                        brightnessSlider.value = exposure; // Set the slider value to the saved exposure value
                        brightnessSlider.onValueChanged.AddListener(value =>
                        {
                            exposure = value; // Update the exposure value
                            PlayerPrefs.SetFloat("Brightness", exposure); // Save the exposure value to PlayerPrefs
                            if (PlayerManager.Instance.mapId == 2 || PlayerManager.Instance.mapId == 3)
                                exposure /= 2f; // Adjust exposure for specific maps
                            RenderSettings.skybox.SetFloat("_Exposure", exposure); // Set the skybox exposure
                        });
                    }
                
            }
        }
    }

    private void OnDestroy()
    {
        if(potionTimer >0)
            PlayerPrefs.SetFloat("potionTimer", potionTimer); // Save the current potion timer value
        else
            PlayerPrefs.SetFloat("potionTimer", 0f); // Reset the potion timer if it's not active

        PlayerPrefs.SetString("lastLoginDate", System.DateTime.Now.ToString("yyyyMMdd")); // Save the current date as the last login date
    }

    void LoadSettings()
    {
       //Load the player's nickname from PlayerPrefs if it exists
        if (PlayerPrefs.HasKey("LastNick"))
        {
            lastNick = PlayerPrefs.GetString("LastNick");
            nick = lastNick; // Set the nickname to the last used nickname
            Debug.Log("LoadSettings called" + nick);
            nickInputField.text = nick; // Set the input field text to the loaded nickname
        }
        else if (nickInputField.text != "" || nickInputField.text != string.Empty)
        {
            nick = nickInputField.text; // Set the nickname to the input field text if it's not empty
            Debug.Log("LoadSettings called from input field" + nick);
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
            skinId = 3;
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
            skinColor = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.5f, 1f); // Default
        }
        if(PlayerPrefs.HasKey("Volume"))
        {
            volume = PlayerPrefs.GetFloat("Volume");
        }
        else
        {
            volume = 0.2f; // Default volume level
        }
        if (PlayerPrefs.HasKey("Music"))
        {
            music = PlayerPrefs.GetInt("Music") == 1; // Convert int to bool
        }
        else
        {
            music = true; // Default music setting
        }
        if (PlayerPrefs.HasKey("MusicId"))
        {
            musicId = PlayerPrefs.GetInt("MusicId"); // Load the selected music ID
        }
        else
        {
            musicId = 0; // Default music ID
        }
        if (PlayerPrefs.HasKey("EasyFood"))
        {
            easyFood = PlayerPrefs.GetInt("EasyFood") == 1; // Convert int to bool
        }
        else
        {
            easyFood = true; // Default easy food setting
        }
        if (PlayerPrefs.HasKey("Powerup"))
        {
            powerup = PlayerPrefs.GetInt("Powerup") == 1; // Convert int to bool
        }
        else
        {
            powerup = true; // Default powerup setting
        }
    }

    private void CheckInternetAndSetMultiplayerButton()
    {
        if (multiplayerButton == null)
            return;

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            multiplayerButton.interactable = false;
        }
        else
        {
            multiplayerButton.interactable = true;
        }
    }
}


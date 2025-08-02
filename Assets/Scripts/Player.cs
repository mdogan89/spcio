using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Player : MonoBehaviour
{
    private const int MaxSize = 2000;
    private const int FoodScore = 50;
    private const int FoodSize = 12;
    private const float MapLimit = 2000f;
    private static readonly Color FirstPersonTextColor = Color.white;
    private static readonly Color CubeMapTextColor = Color.black;

    [SerializeField] private float speedMult = 5f;
    public Color color;
    [SerializeField] private int score = 100;
    [SerializeField] private int size = 1;
    public string nick = "nick";

    public float SpeedMult => speedMult;
    public int Score => score;
    public int Size => size;
    public string Nick => nick;

    [SerializeField] TextMeshProUGUI nickText;
    [SerializeField] TextMeshProUGUI scoreText;
    Bot bot;
    Animator _animator;
    GameManager gameManager;
    [SerializeField] private AudioSource foodAudio;
    [SerializeField] private AudioSource absorbAudio;
    [SerializeField] private AudioSource shieldAudio;
    [SerializeField] private AudioSource doublePointsAudio;
    [SerializeField] private AudioSource piranhaAudio;
    private MeshRenderer meshRenderer;
    [SerializeField] GameObject cupObject; // Reference to the cup object for the player

    // Event tanýmlamalarý ekleyin
    public event System.Action<int> OnScoreChanged;
    public event System.Action<int> OnSizeChanged;

    public bool hasPowerup;
    public PowerupType currentPowerupType = PowerupType.None;
    private Coroutine powerupCountdown;
    bool doublePoints = false;
    public bool shield = false;
    public bool piranha = false;
    LocalPlayer localPlayer;
    SphereCollider playerCollider;
    Material material;
    Canvas powerupCanvas;
    TextMeshProUGUI powerupText;
    float powerupTimer = 10f; // Default powerup duration
    bool trailerEnabled = true;
    void Awake()
    {
        // Ensure the PlayerManager instance is initialized
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("PlayerManager instance is not found. Make sure it is initialized before Player.");
            return;
        }
        if(Camera.main != null)
            Camera.main.GetComponent<AudioSource>().volume = PlayerManager.Instance.volume/5f; // Set the camera audio volume based on the saved volume level
    }





    void Start()
    {

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("PlayerManager instance is not found. Make sure it is initialized before Player.");
            return;
        }
        Camera.main.GetComponent<AudioSource>().volume = PlayerManager.Instance.volume / 5f; // Set the camera audio volume based on the saved volume level
        meshRenderer = GetComponent<MeshRenderer>();
        gameManager = FindObjectOfType<GameManager>();
        playerCollider = GetComponent<SphereCollider>();
        _animator = GetComponent<Animator>();
        material = meshRenderer.materials[0];
        bot = GetComponent<Bot>();
        if (bot == null)
        {
            localPlayer = GetComponent<LocalPlayer>();
            absorbAudio.volume = PlayerManager.Instance.volume;
            foodAudio.volume = PlayerManager.Instance.volume;
            shieldAudio.volume = PlayerManager.Instance.volume;
            doublePointsAudio.volume = PlayerManager.Instance.volume;
            piranhaAudio.volume = PlayerManager.Instance.volume;
            color = PlayerManager.Instance.skinColor;
            powerupCanvas = GameObject.Find("PowerupCanvas").GetComponent<Canvas>();
            powerupText = powerupCanvas.GetComponentInChildren<TextMeshProUGUI>();
            powerupCanvas.gameObject.SetActive(PlayerManager.Instance.powerup);
            trailerEnabled = PlayerManager.Instance.trailerEnabled;

            if (PlayerManager.Instance.nick == null || PlayerManager.Instance.nick == "")
            {
                PlayerManager.Instance.nick = "Player" + Random.Range(1000, 9999);
                nick = PlayerManager.Instance.nick;
            }
            else
            {
                nick = PlayerManager.Instance.nick;
            }
        }

        // Update the UI text elements with the player's nickname and score
        UpdateUI();

    }

    private void LateUpdate()
    {
        if (bot == null)
        {
            if(!PlayerManager.Instance.thirdPersonView && SceneManager.GetActiveScene().buildIndex != 2)
            {
                material = gameManager.playerMaterials[0];
                SetTextColor(FirstPersonTextColor);
                PlayerManager.Instance.trailerEnabled = false; // Disable trailer effect in first-person view
                // Update the camera position and rotation for third-person view
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, transform.position, Time.deltaTime * 5f); // Smoothly transition camera position
                Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, transform.rotation * Quaternion.Euler(15, 0, 0), Time.deltaTime * 5f); // Smoothly transition camera rotation
            }
            else
            {
                material = gameManager.playerMaterials[PlayerManager.Instance.skinId];
                SetTextColor(CubeMapTextColor);
                // Update the camera position and rotation for first-person view
                PlayerManager.Instance.trailerEnabled = trailerEnabled; // Enable trailer effect in third-person view
                Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, new Vector3(0, 2, -4), Time.deltaTime * 5f); // Smoothly transition camera position
                Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, transform.rotation * Quaternion.Euler(15, 0, 0), Time.deltaTime * 5f); // Smoothly transition camera rotation
            }
            //Camera.main.transform.localScale = Vector3.one * size / 100f; // Adjust camera scale based on player size
        }    
}

    private void Update()
    {
        //speed powerup
        if (hasPowerup && currentPowerupType == PowerupType.SpeedBoost)
        {
            speedMult += speedMult; // Double the speed when speed boost powerup is active
            _animator.SetTrigger("Speed"); // Trigger speed boost animation

            if (bot == null)
            {
                powerupTimer = 1f; // Set powerup timer to 1 second for speed boost
                powerupTimer -= Time.deltaTime; // Decrease powerup timer

                GameObject.FindGameObjectWithTag("GlobalVolume").gameObject.GetComponent<UnityEngine.Rendering.Volume>().profile.TryGet(out UnityEngine.Rendering.Universal.WhiteBalance whiteBalance);
                if (whiteBalance != null)
                    whiteBalance.temperature.value = 50f; // Change color temperature to a warmer tone  
                RenderSettings.skybox.SetFloat("_Exposure", PlayerManager.Instance.exposure + 2f); // Increase skybox exposure for visual effect
                Camera.main.GetComponent<AudioSource>().pitch = 1.5f; // Increase camera pitch for speed boost effect
                localPlayer.audioSource.pitch = 1.5f; // Increase camera pitch for speed boost effect                   
            }
        }
        else
        {
            if (bot == null)
            {
                GameObject.FindGameObjectWithTag("GlobalVolume").gameObject.GetComponent<UnityEngine.Rendering.Volume>().profile.TryGet(out UnityEngine.Rendering.Universal.WhiteBalance whiteBalance);
                if (whiteBalance != null)
                {
                    whiteBalance.temperature.value = 0f; // Reset color temperature to default
                }
                RenderSettings.skybox.SetFloat("_Exposure", PlayerManager.Instance.exposure); // Increase skybox exposure for visual effect
                localPlayer.audioSource.pitch = 1f; // Reset camera pitch to normal
                Camera.main.GetComponent<AudioSource>().pitch = 1f; // Reset camera pitch to normal
            }
            speedMult = (size / Mathf.Pow(size, 1.1f)) * 5; // Reset speed multiplier based on size
        }
        //Shield powerup
        if (hasPowerup && currentPowerupType == PowerupType.Shield)
        {
            shield = true; // Activate shield powerup
            //playerCollider.material.bounciness = 10f; // Set bounciness to 10 for shield effect
            if(SceneManager.GetActiveScene().buildIndex != 3)
                meshRenderer.material = gameManager.playerMaterials[4]; // Change material to shield effect
            if (bot == null) {
                powerupTimer -= Time.deltaTime; // Decrease powerup timer
                Debug.Log("Shield powerup activated!");
                if (!shieldAudio.isPlaying)
                {
                    shieldAudio.Play(); // Play shield activation sound
                }
            }
        }
        else
        {
            shield = false; // Deactivate shield powerup
            meshRenderer.material = material; // Reset material to original
            if (bot == null)
                shieldAudio.Pause();
            //playerCollider.material.bounciness = 1f; // Set bounciness to 10 for shield effect
        }
        //Double points powerup
        if (hasPowerup && currentPowerupType == PowerupType.DoublePoints)
        {
            doublePoints = true;
            if(SceneManager.GetActiveScene().buildIndex != 3)
                meshRenderer.material = gameManager.playerMaterials[5]; // Change material to double points effect
            if (bot == null)
            {
                powerupTimer -= Time.deltaTime; // Decrease powerup timer

                Debug.Log("Double points powerup activated!");
                if (!doublePointsAudio.isPlaying)
                {
                    doublePointsAudio.Play(); // Play double points activation sound        
                }
            }
        }
        else
        {
            doublePoints = false;
            if (!hasPowerup)
                meshRenderer.material = material; // Reset material to original
            if (bot == null)
                doublePointsAudio.Stop(); // Pause double points sound when powerup is inactive
        }
        // Piranha powerup
        if (hasPowerup && currentPowerupType == PowerupType.Piranha)
        {
            piranha = true; // Activate piranha powerup
            if(SceneManager.GetActiveScene().buildIndex != 3)
                meshRenderer.material = gameManager.playerMaterials[6]; // Change material to piranha effect
            if (bot == null)
            {
                powerupTimer -= Time.deltaTime; // Decrease powerup timer

                Debug.Log("Piranha powerup activated!");
                if (!piranhaAudio.isPlaying)
                {
                    piranhaAudio.Play(); // Play piranha activation sound
                }
            }
        }
        else
        {
            piranha = false; // Deactivate piranha powerup
            if (!hasPowerup)
                meshRenderer.material = material; // Reset material to original
            if (bot == null)
                piranhaAudio.Stop(); // Pause piranha sound when powerup is inactive
        }

        //set custom skin
        if (bot == null && meshRenderer != null)
        {
            if (PlayerManager.Instance.skinId == 3)
            {
                meshRenderer.materials[0].SetColor("_EmissionColor", color);
            }
        }
        // Set the text color for cube map
        if (PlayerManager.Instance.mapId == 1 && bot != null)
        {
            SetTextColor(CubeMapTextColor);
        }
        //check if player is out of map
        if (transform.position.magnitude > MapLimit)
        {
            transform.position = Spawner.GetRandomPosition();
        }
        // Disable UI for how to play scene
        if (SceneManager.GetActiveScene().buildIndex != 2)
        {
            UpdateUI();
        }
        // Check if the player has reached the maximum size
        if (size >= MaxSize && PlayerManager.Instance.gameMode != 1)
        {
            size = MaxSize;
            if (bot == null)
            {
                LocalPlayer.winner = true;
                gameManager.GameOver(score);
            }
        }

        if (bot == null && powerupText != null)
        {
            powerupText.text = hasPowerup ? currentPowerupType.ToString() + " - " + powerupTimer.ToString("0.000") : "No Powerup"; // Update powerup text
        }

        CheckCupOWner();
        CheckSurvivalGameMode();
        if(bot == null) { 
            doublePointsAudio.volume = PlayerManager.Instance.volume; // Set the double points audio volume based on the saved volume level
            shieldAudio.volume = PlayerManager.Instance.volume; // Set the shield audio volume based on the saved volume level
            piranhaAudio.volume = PlayerManager.Instance.volume; // Set the piranha audio volume based on the saved volume level
        }
    }

    private void CheckSurvivalGameMode()
    {
        if (PlayerManager.Instance.gameMode != 1) return;

        if (Spawner.botList.Count == 0 && GameObject.FindGameObjectWithTag("Player") != null)
        {
            LocalPlayer.winner = true;
            if (gameManager != null)
            {
                gameManager.GameOver(score);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            var food = other.GetComponent<Food>();
            if (food != null)
            {
                EatFood(food);
            }
        }
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            currentPowerupType = other.GetComponent<Powerup>().powerupType;
            //powerupIndicator.gameObject.SetActive(true);
            Destroy(other.gameObject);
            if (powerupCountdown != null)
            {
                StopCoroutine(powerupCountdown);
            }
            powerupCountdown = StartCoroutine(PowerupCountdown());
        }
    }
    IEnumerator PowerupCountdown()
    {
        if (currentPowerupType == PowerupType.SpeedBoost)
            yield return new WaitForSeconds(1f);
        else
            yield return new WaitForSeconds(10f);
        hasPowerup = false;
        doublePoints = false;
        shield = false;
        piranha = false;
        currentPowerupType = PowerupType.None;
        powerupTimer = 10f; // Reset powerup timer to default value
        //powerupIndicator.gameObject.SetActive(false);
        if (bot == null)
            Debug.Log("Powerup ended");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.gameObject == null) return;
        Player collidedPlayer = collision.gameObject.GetComponent<Player>();
        if (collidedPlayer == null) return;
        var collidedBot = collidedPlayer.GetComponent<Bot>();
        if (score == collidedPlayer.score) return; // Do not process collision if scores are equal
        // Bot collided with player
        if (collidedBot == null)
        {
            if (score < collidedPlayer.score)
            {
                if (piranha && !collidedPlayer.piranha && !collidedPlayer.shield)
                    gameManager.GameOver(collidedPlayer.score); // Trigger game over if the player is absorbed by a piranha
                else if (collidedPlayer.piranha && !piranha && !collidedPlayer.shield)
                    gameManager.GameOver(collidedPlayer.score); // Trigger game over if the player is absorbed by a piranha
                else
                    collidedPlayer.AbsorbPlayer(this);
            }
            else if (score > collidedPlayer.score)
            {
                if (piranha && !collidedPlayer.piranha)
                    collidedPlayer.AbsorbPlayer(this);
                else if (!piranha && collidedPlayer.piranha)
                    collidedPlayer.AbsorbPlayer(this); // Absorb the collided player if the player has a higher score
                else if (!collidedPlayer.shield)
                    gameManager.GameOver(collidedPlayer.score); // Trigger game over if the player is absorbed
            }
        }
        // Bot collided with another bot
        else if (collidedBot != null && bot != null)
        {
            if (score < collidedPlayer.score)
            {
                if (piranha && !collidedPlayer.piranha)
                    AbsorbPlayer(collidedPlayer);
                else if (collidedPlayer.piranha && !piranha)
                    AbsorbPlayer(collidedPlayer);
                else
                    collidedPlayer.AbsorbPlayer(this);
            }
            else if (score > collidedPlayer.score)
            {
                if (piranha && !collidedPlayer.piranha)
                    collidedPlayer.AbsorbPlayer(this);
                else if (collidedPlayer.piranha && !piranha)
                    collidedPlayer.AbsorbPlayer(this);
                else
                    AbsorbPlayer(collidedPlayer);
            }
        }
    }
    void EatFood(Food food)
    {
        if (doublePoints)
        {
            UpdateScore(FoodScore * 2); // Double the score if double points powerup is active
            size += FoodSize * 2; // Double the size increase if double points powerup is active
            OnSizeChanged?.Invoke(size);
        }
        else
        {
            UpdateScore(FoodScore);
            size += FoodSize;
            OnSizeChanged?.Invoke(size);
        }
        if (bot == null)
        {
            if (PlayerManager.Instance.skinId == 3)
                _animator.SetTrigger("EmissiveEat");
            else
                _animator.SetTrigger("Eat");

            foodAudio.Play();
#if UNITY_ANDROID_API || UNITY_IOS
            if (PlayerManager.Instance.vibration)
                Handheld.Vibrate();
#endif
        }
        UpdateSize();
        UpdateSpeed();

        Vector3 position = Spawner.GetRandomPosition();
        food.transform.position = position;

        if (bot != null)
            bot.hasTarget = false;
    }

    private void UpdateScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
        UpdateUI();
    }

    void UpdateSize()
    {
        if (size < 0)
        {
            Debug.Log("Size cannot be negative. Setting to 0.");
            size = 0;
        }

        float scale = 1f + 1000f * (Mathf.Clamp(size, 0, 65535) / 65535f);
        transform.localScale = Vector3.one * scale;
    }

    public void AbsorbPlayer(Player other)
    {
        if (other.shield)
        {
            Debug.Log("Cannot absorb player with shield.");
            return; // Cannot absorb player with shield
        }
        else
        {



            if (doublePoints)
            {
                UpdateScore(other.score * 2); // Double the score if double points powerup is active
                size += other.size * 2; // Double the size increase if double points powerup is active
                OnSizeChanged?.Invoke(size);

            }
            else
            {
                UpdateScore(other.score);
                size += other.size;
                OnSizeChanged?.Invoke(size);
            }
            UpdateSize();
            UpdateSpeed();
            if (PlayerManager.Instance.gameMode == 1)
            {
                Destroy(other.gameObject);
                Spawner.botList.Remove(other.GetComponent<Bot>());
                Spawner.playerList.Remove(other);
            }
            else
            {
                other.score = 100;
                other.size = 1;
                other.speedMult = 5f;
                other.transform.localScale = new Vector3(1, 1, 1);
                other.GetComponent<Rigidbody>().MovePosition(Spawner.GetRandomPosition());
                if (other.GetComponent<Bot>() != null)
                    other.GetComponent<Bot>().hasTarget = false;
            }
            if (bot != null)
                bot.hasTarget = false;

            if (bot == null)
            {
                if (PlayerManager.Instance.skinId == 3)
                    _animator.SetTrigger("EmissiveEat");
                _animator.SetTrigger("Absorb");
                absorbAudio.Play();
#if UNITY_ANDROID_API || UNITY_IOS
                if (PlayerManager.Instance.vibration)
                    Handheld.Vibrate();
#endif
            }
        }
    }

    private void UpdateSpeed()
    {
        speedMult = (size / Mathf.Pow(size, 1.1f)) * 5;
    }

    private void UpdateUI()
    {
        if (nickText != null) nickText.text = nick;
        if (scoreText != null) scoreText.text = score.ToString();
    }

    private void SetTextColor(Color color)
    {
        if (scoreText != null) scoreText.color = color;
        if (nickText != null) nickText.color = color;
    }

    void CheckCupOWner()
    {
        if (PlayerManager.Instance.gameMode == 3 && localPlayer == null)
            return;
        if(Spawner.playerList.Count == 0)
            return; // No players to check for cup ownership
        List<GameObject> targets = Spawner.playerList.Where(p => p != null).Select(p => p.gameObject).ToList();
        targets.Sort((a, b) => b.GetComponent<Player>().Score.CompareTo(a.GetComponent<Player>().Score)); // Sort players by score in descending order
        Player cupOwner;
        if (targets[0] != null)
        {
            cupOwner = targets[0].GetComponent<Player>(); // Get the player with the highest score
            if (cupOwner == this)
            {
                cupObject.SetActive(true); // Show cup object for the player with the highest score
                if (bot == null)
                    Camera.main.GetComponent<AudioSource>().pitch = 1.1f; // Increase camera pitch for the player with the cup
            }
            else
            {
                cupObject.SetActive(false); // Hide cup object for other players
                if (bot == null)
                    Camera.main.GetComponent<AudioSource>().pitch = 1f; // Increase camera pitch for the player with the cup
            //}
            //if (SceneManager.GetActiveScene().buildIndex == 3)
            //{
            //   Camera.main.transform.position = Vector3.Lerp(cupOwner.transform.position, cupOwner.transform.position + new Vector3(0, 2, -10), Time.deltaTime); // Smoothly transition camera position
            //    Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, cupOwner.transform.rotation * Quaternion.Euler(15, 0, 0), Time.deltaTime * 5f);
            //    Camera.main.transform.localScale = Vector3.one * cupOwner.size / 100f; // Adjust camera scale based on player size
                // Smoothly transition camera rotation
            }
        }
      
    }






}

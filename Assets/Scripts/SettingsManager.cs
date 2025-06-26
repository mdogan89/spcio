using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] ToggleGroup skinToggleGroup; // Group for skin toggles
    [SerializeField] ToggleGroup mapToggleGroup; // Group for map toggles
    [SerializeField] ToggleGroup botLevelToggleGroup;
    [SerializeField] Slider playerNumberSlider; // Slider for number of bots
    [SerializeField] Slider foodNumberSlider; // Slider for number of food items
    [SerializeField] Slider spawnRadiusSlider; // Slider for spawn radius
    [SerializeField] Toggle thirdPersonViewToggle; // Toggle for third-person view
    [SerializeField] Toggle fogToggle; // Toggle for fog
    [SerializeField] Toggle trailerToggle; // Toggle for trailer
    [SerializeField] Toggle starsToggle; // Toggle for stars
    [SerializeField] Toggle easyControlsToggle; // Toggle for easy controls
    [SerializeField] Slider brightnessSlider; // Slider for brightness
    [SerializeField] Slider lookSensitivitySlider; // Slider for look sensitivity
    [SerializeField] Slider moveSensitivitySlider; // Slider for move sensitivity
    void Awake()
    {
        // Ensure the PlayerManager instance is initialized
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("PlayerManager instance is not initialized.");
            return;
        }
        // Load saved settings from PlayerPrefs
        LoadSettings();
    }

    public void OnSkinSelected(int skinId)
    {
        PlayerManager.Instance.skinId = skinId;
        PlayerPrefs.SetInt("SkinId", skinId); // Save the selected skin ID to PlayerPrefs
    }

    public void OnMapSelected(int mapId)
    {
        PlayerManager.Instance.mapId = mapId;
        PlayerPrefs.SetInt("MapId", mapId); // Save the selected map ID to PlayerPrefs
    }

    public void OnBotLevelSelected(int level)
    {
        PlayerManager.Instance.botLevel = level;
        PlayerPrefs.SetInt("BotLevel", level); // Save the selected bot level to PlayerPrefs
    }

    public void OnPlayerNumberSelected(float number)
    {
        PlayerManager.Instance.numberOfBots = Convert.ToInt32(number);
        PlayerPrefs.SetInt("NumberOfBots", PlayerManager.Instance.numberOfBots); // Save the number of bots to PlayerPrefs
    }

    public void OnFoodNumberSelected(float number)
    {
        PlayerManager.Instance.numberOfFood = Convert.ToInt32(number);
        PlayerPrefs.SetInt("NumberOfFood", PlayerManager.Instance.numberOfFood); // Save the number of food items to PlayerPrefs
    }

    public void OnSpawnRadiusSelected(float radius)
    {
        PlayerManager.Instance.spawnRadius = radius;
        PlayerPrefs.SetFloat("SpawnRadius", PlayerManager.Instance.spawnRadius); // Save the spawn radius to PlayerPrefs
    }
    public void OnBrightnessSelected(float brightness)
    {
        PlayerManager.Instance.exposure = brightness;
        PlayerPrefs.SetFloat("Brightness", brightness); // Save the brightness setting to PlayerPrefs
    }
    public void OnCameraPosition(bool thirdPerson)
    {
        PlayerManager.Instance.thirdPersonView = thirdPerson;
        PlayerPrefs.SetInt("ThirdPersonView", thirdPerson ? 1 : 0); // Save the camera position setting to PlayerPrefs
    }
    public void OnFogSelected(bool isOn)
    {
        //PlayerManager.Instance.fogEnabled = isOn;
        //if (isOn)
        //{
        //    RenderSettings.fog = true;
        //    RenderSettings.fogColor = Color.gray; // Set fog color to gray
        //    RenderSettings.fogDensity = 0.05f; // Set fog density
        //}
        //else
        //{
        //    RenderSettings.fog = false;
        //}
        PlayerManager.Instance.fogEnabled = isOn;
        PlayerPrefs.SetInt("FogEnabled", isOn ? 1 : 0); // Save the fog setting to PlayerPrefs
    }
    public void OnTrailerSelected(bool isOn)
    {
        PlayerManager.Instance.trailerEnabled = isOn;
        PlayerPrefs.SetInt("TrailerEnabled", isOn ? 1 : 0); // Save the trailer setting to PlayerPrefs
    }
    public void OnStarsSelected(bool isOn)
    {
        PlayerManager.Instance.starsEnabled = isOn;
        PlayerPrefs.SetInt("StarsEnabled", isOn ? 1 : 0); // Save the stars setting to PlayerPrefs
    }
    public void OnEasyControlsClicked(bool isOn)
    {
        PlayerManager.Instance.easyControls = isOn;
        PlayerPrefs.SetInt("EasyControls", isOn ? 1 : 0); // Save the easy controls setting to PlayerPrefs
    }

    public void OnLookSenstivitySelected(float sensitivity)
    {
        PlayerManager.Instance.lookSensitivity = sensitivity;
        PlayerPrefs.SetFloat("LookSensitivity", sensitivity); // Save the look sensitivity to PlayerPrefs
    }

    public void OnMoveSenstivitySelected(float sensitivity)
    {
        PlayerManager.Instance.moveSensitivity = sensitivity;
        PlayerPrefs.SetFloat("MoveSensitivity", sensitivity); // Save the move sensitivity to PlayerPrefs
    }

    void LoadSettings()
    {
        skinToggleGroup.GetComponentInChildren<Transform>().Find($"SkinToggle{PlayerManager.Instance.skinId}").GetComponent<Toggle>().isOn = true;
        mapToggleGroup.GetComponentsInChildren<Toggle>()[PlayerManager.Instance.mapId].isOn = true; // Set the selected map toggle based on saved map ID    
        botLevelToggleGroup.GetComponentsInChildren<Toggle>()[PlayerManager.Instance.botLevel].isOn = true; // Set the selected bot level toggle based on saved bot level
        playerNumberSlider.value = PlayerManager.Instance.numberOfBots; // Set the slider value for number of bots based on saved value
        foodNumberSlider.value = PlayerManager.Instance.numberOfFood; // Set the slider value for number of food items based on saved value
        spawnRadiusSlider.value = PlayerManager.Instance.spawnRadius; // Set the slider value for spawn radius based on saved value
        thirdPersonViewToggle.isOn = PlayerManager.Instance.thirdPersonView; // Set the toggle for third-person view based on saved setting
        fogToggle.isOn = PlayerManager.Instance.fogEnabled; // Set the toggle for fog based on saved setting
        trailerToggle.isOn = PlayerManager.Instance.trailerEnabled; // Set the toggle for trailer based on saved setting
        starsToggle.isOn = PlayerManager.Instance.starsEnabled; // Set the toggle for stars based on saved setting
        easyControlsToggle.isOn = PlayerManager.Instance.easyControls; // Set the toggle for easy controls based on saved setting
        brightnessSlider.value = PlayerManager.Instance.exposure; // Set the brightness slider value based on saved exposure value
        lookSensitivitySlider.value = PlayerManager.Instance.lookSensitivity; // Set the look sensitivity slider value based on saved value
        moveSensitivitySlider.value = PlayerManager.Instance.moveSensitivity; // Set the move sensitivity slider value based on saved value
    }

    public void OnDefaultsButtonClicked()
    {
        skinToggleGroup.GetComponentInChildren<Transform>().Find($"SkinToggle{0}").GetComponent<Toggle>().isOn = true;
        mapToggleGroup.GetComponentsInChildren<Toggle>()[0].isOn = true; // Set the selected map toggle based on saved map ID    
        botLevelToggleGroup.GetComponentsInChildren<Toggle>()[0].isOn = true; // Set the selected bot level toggle based on saved bot level
        playerNumberSlider.value = 50; // Set the slider value for number of bots based on saved value
        foodNumberSlider.value = 50; // Set the slider value for number of food items based on saved value
        spawnRadiusSlider.value = 50; // Set the slider value for spawn radius based on saved value
        thirdPersonViewToggle.isOn = true; // Set the toggle for third-person view based on saved setting
        fogToggle.isOn = true; // Set the toggle for fog based on saved setting
        trailerToggle.isOn = true; // Set the toggle for trailer based on saved setting
        starsToggle.isOn = true; // Set the toggle for stars based on saved setting
        easyControlsToggle.isOn = true; // Set the toggle for easy controls based on saved setting
        brightnessSlider.value = 3.0f; // Set the brightness slider value based on saved exposure value
        lookSensitivitySlider.value = 1.0f; // Set the look sensitivity slider value based on saved value
        moveSensitivitySlider.value = 1.0f; // Set the move sensitivity slider value based on saved value
    }



}

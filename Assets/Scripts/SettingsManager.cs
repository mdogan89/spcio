using System;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public void OnSkinSelected(int index)
    {
        PlayerManager.Instance.skinId = index;
    }

    public void OnBotLevelSelected(int level)
    {
        PlayerManager.Instance.botLevel = level;
    }

    public void OnPlayerNumberSelected(float number)
    {
        Spawner.numberOfBots = Convert.ToInt32(number);
    }

    public void OnFoodNumberSelected(float number)
    {
        Spawner.numberOfFood = Convert.ToInt32(number);
    }

    public void OnSpawnRadiusSelected(float radius)
    {
        Spawner.spawnRadius = radius;
    }

    public void OnBrightnessSelected(float brightness)
    {
       LocalPlayer.exposure = brightness;
    }

    public void OnLookSenstivitySelected(float sensitivity)
    {
        LocalPlayer.lookSensitivity = sensitivity;
    }

    public void OnMoveSenstivitySelected(float sensitivity)
    {
        LocalPlayer.moveSensitivity = sensitivity;
    }

    public void OnMapSelected(int mapId)
    {
        PlayerManager.Instance.mapId = mapId;
    }

    public void OnEasyControlsClicked(bool isOn)
    {
        PlayerManager.Instance.easyControls = isOn;
    }

    public void OnCameraPosition(bool thirdPerson)
    {
        PlayerManager.Instance.thirdPersonView = thirdPerson;
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
    }

    public void OnTrailerSelected(bool isOn)
    {
        PlayerManager.Instance.trailerEnabled = isOn;
    }

    public void OnStarsSelected(bool isOn)
    {
        PlayerManager.Instance.starsEnabled = isOn;
    }



}

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
        Player.exposure = brightness;
    }

    public void OnLookSenstivitySelected(float sensitivity)
    {
        Player.lookSensitivity = sensitivity;
    }

    public void OnMoveSenstivitySelected(float sensitivity)
    {
        Player.moveSensitivity = sensitivity;
    }

    public void OnMapSelected(int mapId)
    {
        PlayerManager.Instance.mapId = mapId;
        if(mapId ==1)
        {
            PlayerManager.Instance.isCube = true; // Set the player shape to cube for map 1
        }
    }

    public void OnEasyControlsClicked(bool isOn)
    {
        PlayerManager.Instance.easyControls = isOn;
    }

    public void OnCameraPosition(bool thirdPerson)
    {
        PlayerManager.Instance.thirdPersonView = thirdPerson;
    }



}

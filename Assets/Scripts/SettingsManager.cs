using System;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public Material[] playerMaterials;
    public Material[] mapMaterials;
    Skybox skybox;
    public void OnMoonSkinSelected(bool toggle) {
        PlayerManager.Instance.playerMaterial = playerMaterials[0];
    }

    public void OnMarsSkinSelected(bool toggle)
    {
        PlayerManager.Instance.playerMaterial = playerMaterials[1];
    }

    public void OnBurntSkinSelected(bool toggle)
    {
        PlayerManager.Instance.playerMaterial = playerMaterials[2];
    }

    public void OnSkinSelected(int index)
    {
        if (index >= 0 && index < playerMaterials.Length)
        {
            PlayerManager.Instance.playerMaterial = playerMaterials[index];
        }
        else
        {
            Debug.LogError("Invalid skin index selected: " + index);
        }
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
        mapMaterials[0].SetFloat("_Exposure", brightness);
        //skybox = Camera.main.GetComponent<Skybox>();
        //if (skybox != null)
        //{
        //    skybox.material.SetFloat("exposure", brightness);
        //}
        //else
        //{
        //    Debug.LogError("Skybox component not found on the main camera.");
        //}
    }

    public void OnLookSenstivitySelected(float sensitivity)
    {
        Player.lookSensitivity = sensitivity;
    }

    public void OnMoveSenstivitySelected(float sensitivity)
    {
        Player.moveSensitivity = sensitivity;
    }

    public void OnMapSelected(bool cube)
    {
        PlayerManager.Instance.isCube = cube;



        //if (index >= 0 && index < mapMaterials.Length)
        //{
        //    Camera.main.GetComponent<Skybox>().material = mapMaterials[index];
        //}
        //else
        //{
        //    Debug.LogError("Invalid map index selected: " + index);
        //}
    }



}

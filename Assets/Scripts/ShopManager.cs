using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] GameObject potionsCanvas;
    [SerializeField] TextMeshProUGUI potionsButtonTxt;
    [SerializeField] GameObject skinsCanvas;
    [SerializeField] TextMeshProUGUI skinsButtonTxt;

    [SerializeField] TextMeshProUGUI smallPotionsText;
    [SerializeField] TMP_InputField smallPotionsInputField;
    [SerializeField] int totalCoins;
    [SerializeField] Toggle smallPotionToggle;

    [SerializeField] TextMeshProUGUI mediumPotionsText;
    [SerializeField] TMP_InputField mediumPotionsInputField;
    [SerializeField] Toggle mediumPotionToggle;

    [SerializeField] TextMeshProUGUI bigPotionsText;
    [SerializeField] TMP_InputField bigPotionsInputField;
    [SerializeField] Toggle bigPotionToggle;

    [SerializeField] TextMeshProUGUI totalCoinsText;
    [SerializeField] Button earthBuyButton;
    [SerializeField] Toggle earthToggle;

    [SerializeField] Button moonBuyButton;
    [SerializeField] Toggle moonToggle;

    [SerializeField] Button marsBuyButton;
    [SerializeField] Toggle marsToggle;


    void Start()
    {
        OnPotionsButtonClicked();
        PlayerPrefs.SetInt("totalCoins", 500); // For testing purposes, set initial coins
    }

    void Update()
    {

        totalCoins = PlayerPrefs.GetInt("totalCoins", 0);
        totalCoinsText.text = totalCoins.ToString();
        smallPotionsText.text = PlayerPrefs.GetInt("smallPotions", 0).ToString();
        mediumPotionsText.text = PlayerPrefs.GetInt("mediumPotions", 0).ToString();
        bigPotionsText.text = PlayerPrefs.GetInt("bigPotions", 0).ToString();

        if (PlayerManager.Instance.potionTimer > 0)
        {
            PlayerManager.Instance.potionTimer -= Time.deltaTime;
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                if (smallPotionToggle.isOn)
                    smallPotionToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Activated\n" + PlayerManager.Instance.potionTimer.ToString("F0") + "s";
                if (mediumPotionToggle.isOn)
                    mediumPotionToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Activated\n" + PlayerManager.Instance.potionTimer.ToString("F0") + "s";
                if (bigPotionToggle.isOn)
                    bigPotionToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Activated\n" + PlayerManager.Instance.potionTimer.ToString("F0") + "s";
                smallPotionToggle.interactable = false;
                mediumPotionToggle.interactable = false;
                bigPotionToggle.interactable = false;
            }
            PlayerManager.Instance.showAds = false;
        }
        else
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                smallPotionToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Activate";
                mediumPotionToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Activate";
                bigPotionToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Activate";
                smallPotionToggle.isOn = false;
                mediumPotionToggle.isOn = false;
                bigPotionToggle.isOn = false;
                smallPotionToggle.interactable = true;
                mediumPotionToggle.interactable = true;
                bigPotionToggle.interactable = true;
            }
            PlayerManager.Instance.potionTimer = 0f;
            PlayerManager.Instance.showAds = true;
        }

        if (PlayerPrefs.GetInt("earthSkin", 0) == 1 && SceneManager.GetActiveScene().buildIndex == 0)
        {
            earthToggle.interactable = true;
            earthBuyButton.interactable = false;
            earthBuyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Owned";
        }
        else
        {
            earthToggle.isOn = false;
            earthToggle.interactable = false;
        }

        if (PlayerPrefs.GetInt("moonSkin", 0) == 1 && SceneManager.GetActiveScene().buildIndex == 0)
        {
            moonToggle.interactable = true;
            moonBuyButton.interactable = false;
            moonBuyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Owned";
        }
        else
        {
            moonToggle.isOn = false;
            moonToggle.interactable = false;
        }
        if (PlayerPrefs.GetInt("marsSkin", 0) == 1 && SceneManager.GetActiveScene().buildIndex == 0)
        {
            marsToggle.interactable = true;
            marsBuyButton.interactable = false;
            marsBuyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Owned";
        }
        else
        {
            marsToggle.isOn = false;
            marsToggle.interactable = false;
        }

        int skinId = PlayerManager.Instance.skinId;
        if (skinId == 3 && SceneManager.GetActiveScene().buildIndex == 0)
        {
            moonToggle.isOn = false;
            earthToggle.isOn = false;
            marsToggle.isOn = false;
        }
    }




    public void OnPotionsButtonClicked()
    {
        potionsCanvas.gameObject.SetActive(true);
        potionsButtonTxt.color = Color.white;
        skinsCanvas.gameObject.SetActive(false);
        skinsButtonTxt.color = Color.black;
    }
    public void OnSkinsButtonClicked()
    {
        potionsCanvas.gameObject.SetActive(false);
        potionsButtonTxt.color = Color.black;
        skinsCanvas.gameObject.SetActive(true);
        skinsButtonTxt.color = Color.white;
    }

    public void OnPotionBought(int id)
    {
        int coins = PlayerPrefs.GetInt("totalCoins");
        if (coins <= 0)
            return;
        if (id == 0)
        {
            int smallPotions = PlayerPrefs.GetInt("smallPotions");
            if (int.TryParse(smallPotionsInputField.text, out int quantity) && quantity > 0)
            {
                smallPotions += quantity;
                coins -= quantity; // Assuming each small potion costs 1 coin
                PlayerPrefs.SetInt("smallPotions", smallPotions);
                smallPotionsInputField.text = "";
            }
        }
        if(id == 1)
        {
            int mediumPotions = PlayerPrefs.GetInt("mediumPotions");
            if (int.TryParse(mediumPotionsInputField.text, out int quantity) && quantity > 0)
            {
                mediumPotions += quantity;
                coins -= quantity * 2;
                PlayerPrefs.SetInt("mediumPotions", mediumPotions);
                mediumPotionsInputField.text = "";
            }
        }
        if (id == 2)
        {
            int bigPotions = PlayerPrefs.GetInt("bigPotions");
            if (int.TryParse(bigPotionsInputField.text, out int quantity) && quantity > 0)
            {
                bigPotions += quantity;
                coins -= quantity * 3;
                PlayerPrefs.SetInt("bigPotions", bigPotions);
                bigPotionsInputField.text = "";
            }
        }
        PlayerPrefs.SetInt("totalCoins", coins);
    }

    public void OnSmallPotionToggleChanged(bool isOn)
    {
        int smallPotions = PlayerPrefs.GetInt("smallPotions",0);

        if (smallPotions > 0)
        {
            smallPotions -= 1;
            PlayerPrefs.SetInt("smallPotions", smallPotions);
            PlayerManager.Instance.potionTimer = 300f; // Add 5 minutes (300 seconds)
            Debug.Log("Small Potion Activated! Remaining count: " + smallPotions);
        }
        else
        {
            Debug.Log("No Small Potions left to activate!");
            smallPotionToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Activate";
            smallPotionToggle.isOn = false;
           // PlayerManager.Instance.potionTimer = 0f;
            PlayerManager.Instance.showAds = true;
        }
    }

    public void OnMediumPotionToggleChanged(bool isOn)
    {
        int mediumPotions = PlayerPrefs.GetInt("mediumPotions",0);

        if (mediumPotions > 0)
        {
            mediumPotions -= 1;
            PlayerPrefs.SetInt("mediumPotions", mediumPotions);
            PlayerManager.Instance.potionTimer = 900f;
            Debug.Log("Medium Potion Activated! Remaining count: " + mediumPotions);
        }
        else
        {
            Debug.Log("No Medium Potions left to activate!");
            mediumPotionToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Activate";
            mediumPotionToggle.isOn = false;
            //potionTimer = 0f;
            PlayerManager.Instance.showAds = true;
        }
    }

    public void OnBigPotionToggleChanged(bool isOn)
    {
        int bigPotions = PlayerPrefs.GetInt("bigPotions",0);

        if (bigPotions > 0)
        {
            bigPotions -= 1;
            PlayerPrefs.SetInt("bigPotions", bigPotions);
            PlayerManager.Instance.potionTimer = 1800f; 
            Debug.Log("Big Potion Activated! Remaining count: " + bigPotions);
        }
        else
        {
            Debug.Log("No Big Potions left to activate!");
            bigPotionToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Activate";
            bigPotionToggle.isOn = false;
            //potionTimer = 0f;
            PlayerManager.Instance.showAds = true;
        }
    }

    public void OnBuyEarthSkinClicked()
    {
        int coins = PlayerPrefs.GetInt("totalCoins");
        if (coins >= 50 && !earthToggle.isOn)
        {
            coins -= 50; // Assuming the Earth skin costs 50 coins
            PlayerPrefs.SetInt("totalCoins", coins);
            earthToggle.isOn = true;
            earthToggle.interactable = true;
            PlayerPrefs.SetInt("earthSkin", 1);
            Debug.Log("Earth Skin Purchased!");
        }
        else if (earthToggle.isOn)
        {
            Debug.Log("Earth Skin already owned!");
        }
        else
        {
            Debug.Log("Not enough coins to purchase Earth Skin!");
        }
    }

    public void OnEarthSkinSelected(bool isOn)
    {
        if (isOn)
        {
            PlayerManager.Instance.skinId = 2;
            Debug.Log("Earth Skin Selected!");
        }
    }
    
    public void OnBuyMoonSkinClicked()
    {
        int coins = PlayerPrefs.GetInt("totalCoins");
        if (coins >= 100 && !moonToggle.isOn)
        {
            coins -= 100; // Assuming the Moon skin costs 50 coins
            PlayerPrefs.SetInt("totalCoins", coins);
            moonToggle.isOn = true;
            moonToggle.interactable = true;
            PlayerPrefs.SetInt("moonSkin", 1);
            PlayerManager.Instance.skinId = 0;
            Debug.Log("Moon Skin Purchased!");
        }
        else if (marsToggle.isOn)
        {
            Debug.Log("Moon Skin already owned!");
        }
        else
        {
            Debug.Log("Not enough coins to purchase Moon Skin!");
        }
    }

    public void OnMoonSkinSelected(bool isOn)
    {
        if (isOn)
        {
            PlayerManager.Instance.skinId = 0;
            Debug.Log("Moon Skin Selected!");
        }
    }

    public void OnBuyMarsSkinClicked()
    {
        int coins = PlayerPrefs.GetInt("totalCoins");
        if (coins >= 200 && !marsToggle.isOn)
        {
            coins -= 200; // Assuming the Mars skin costs 200 coins
            PlayerPrefs.SetInt("totalCoins", coins);
            marsToggle.isOn = true;
            marsToggle.interactable = true;
            PlayerPrefs.SetInt("marsSkin", 1);
            PlayerManager.Instance.skinId = 1;
            Debug.Log("Mars Skin Purchased!");
        }
        else if (marsToggle.isOn)
        {
            Debug.Log("Mars Skin already owned!");
        }
        else
        {
            Debug.Log("Not enough coins to purchase Mars Skin!");
        }
    }

    public void OnMarsSkinSelected(bool isOn)
    {
        if (isOn)
        {
            PlayerManager.Instance.skinId = 1;
            Debug.Log("Mars Skin Selected!");
        }
    }

}


using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

[Serializable]
public class NonConsumableItem
{
    public string Name;
    public string Id;
    public string desc;
    public float price;
}

[Serializable]
public class ConsumableItem
{
    public string Name;
    public string Id;
    public string desc;
    public float price;
}

public class ShopScript : MonoBehaviour , IStoreListener
{
    public static bool receiptChecked = false;
    public static bool showAds = true;
    public NonConsumableItem ncItem;
    public ConsumableItem cItem;
    IStoreController storeController;
    [SerializeField] Button nonConsumableBtn;
    [SerializeField] TextMeshProUGUI coinTxt;
    [SerializeField] Button consumableBtn;

    public Data data;
    public Payload payload;
    public PayloadData payloadData;


    public void NonConsumable_Btn_Pressed()
    {
        storeController.InitiatePurchase(ncItem.Id);
    }

    public void Consumable_Btn_Pressed()
    {
        storeController.InitiatePurchase(cItem.Id);
    } 




    void SetupBuilder()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(cItem.Id, ProductType.Consumable, new IDs { { cItem.Id, GooglePlay.Name }, { cItem.Id, AppleAppStore.Name } });
        builder.AddProduct(ncItem.Id, ProductType.NonConsumable, new IDs
        {
            { ncItem.Id, GooglePlay.Name },
            { ncItem.Id, AppleAppStore.Name }
        });

        UnityPurchasing.Initialize(this, builder);
    }
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("ShopScript: OnInitialized called");
        storeController = controller;
        if (storeController != null && storeController.products != null)
        {
            Debug.Log("ShopScript: Products initialized successfully.");
            CheckNonConsumable(ncItem.Id);
            receiptChecked = true;
        }
        else
        {
            Debug.LogError("ShopScript: m_controller or products is null after initialization.");
        }
    }

   
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError("ShopScript: OnInitializeFailed called with error: " + error + " and message: " + message);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("OnPurchaseFailed" + product.definition.id + failureReason);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var product = purchaseEvent.purchasedProduct;
        if (product.definition.id == ncItem.Id)
        {
            Debug.Log("Purchase successful: " + product.definition.id);
            return PurchaseProcessingResult.Complete;
        }
        
        else if(product.definition.id == cItem.Id)
        {
            string receipt = product.receipt;
            data = JsonUtility.FromJson<Data>(receipt);
            payload = JsonUtility.FromJson<Payload>(data.Payload);
            payloadData = JsonUtility.FromJson<PayloadData>(payload.json);

            int quantity = payloadData.quantity;
            for (int i = 0; i < quantity; i++)
            {
                AddCoins(100); // Assuming each consumable purchase gives 100 coins
            }
            return PurchaseProcessingResult.Complete;
        }
        else
        {
            Debug.Log("Purchase failed: Unknown product ID");
            return PurchaseProcessingResult.Pending;
        }

    }

   void Awake()
    {
        if (PlayerPrefs.HasKey("showAds"))
        {
            int adsPref = PlayerPrefs.GetInt("showAds");
            if (adsPref == 0)
                nonConsumableBtn.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        SetupBuilder();
        int coins = PlayerPrefs.GetInt("totalCoins",0);
        coinTxt.text = coins.ToString();
    }
    void Update()
    {
        if(!showAds)
            PlayerManager.Instance.showAds = false;
    }
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("ShopScript: OnInitializeFailed called with error: " + error);
    }

    void CheckNonConsumable(string id)
    {
        if(storeController != null && storeController.products != null)
        {
            var product = storeController.products.WithID(id);
            if (product != null && product.hasReceipt)
            {
                RemoveAds();
                Debug.Log("Non-consumable item already purchased: " + id);
            }
            else
            {
                ShowAds();
                Debug.Log("Non-consumable item not purchased: " + id);
            }
        }
        else
        {
            Debug.LogError("StoreController or products is null");
        }
    }
    void RemoveAds()
    {
        nonConsumableBtn.gameObject.SetActive(false);
        showAds = false;
        PlayerManager.Instance.showAds = false; // Update PlayerManager to reflect ad removal
        PlayerManager.Instance.adsRemoved = true;
        PlayerPrefs.SetInt("showAds", 0);
        Debug.Log("Ads removed");
    }
    void ShowAds()
    {
        showAds = true;
        PlayerManager.Instance.showAds = true; // Update PlayerManager to reflect ad showing
        PlayerManager.Instance.adsRemoved = false;
        PlayerPrefs.SetInt("showAds", 1);
        Debug.Log("Ads shown");
    }

    void AddCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt("totalCoins", 0);
        currentCoins += amount;
        PlayerPrefs.SetInt("totalCoins", currentCoins);
        coinTxt.text = currentCoins.ToString();
        Debug.Log("Added " + amount + " coins. Total now: " + currentCoins);
    }



}
[Serializable]
public class SkuDetails
{
    public string productId;
    public string type;
    public string title;
    public string name;
    public string iconUrl;
    public string price;
    public long price_amount_micros;
    public string price_currency_code;
    public string description;
    public string skuDetailsToken;
}
[Serializable]
public class Data
{
    public string Payload;
    public string Store;
    public string TransactionID;
}
[Serializable]
public class Payload
{
    public string json;
    public string signature;
    public List<SkuDetails> skuDetails;
    public PayloadData payloadData;
}
[Serializable]
public class PayloadData
{
    public string orderId;
    public string packageName;
    public string productId;
    public long purchaseTime;
    public int purchaseState;
    public string purchaseToken;
    public int quantity;
    public bool acknowledged;
}


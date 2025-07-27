using System;
using UnityEngine;
using UnityEngine.Purchasing;

[Serializable]
public class NonConsumableItem
{
    public string Name;
    public string Id;
    public string desc;
    public float price;
}

public class ShopScript : MonoBehaviour , IStoreListener
{
    public bool showAds = true;
    public NonConsumableItem ncItem;
    IStoreController storeController;

    public void NonConsumable_Btn_Pressed()
    {
        storeController.InitiatePurchase(ncItem.Id);
    }
    void SetupBuilder()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
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
        else
        {
            Debug.LogWarning("Purchase failed: Unknown product ID");
            return PurchaseProcessingResult.Pending;
        }

    }

   
    void Start()
    {
        SetupBuilder();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        showAds = false;
        PlayerManager.Instance.showAds = false; // Update PlayerManager to reflect ad removal
        Debug.Log("Ads removed");
    }
    void ShowAds()
    {
        showAds = true;
        PlayerManager.Instance.showAds = true; // Update PlayerManager to reflect ad showing
        Debug.Log("Ads shown");
    }
}

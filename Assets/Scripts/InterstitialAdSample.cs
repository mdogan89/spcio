using System;
using UnityEngine;

using com.unity3d.mediation;

public class InterstitialAdSample : MonoBehaviour
{
    private LevelPlayInterstitialAd interstitialAd;

    void Awake()
    {
        IronSource.Agent.setAdaptersDebug(true);
        LevelPlayAdFormat[] levelPlayAdFormats = new[] { LevelPlayAdFormat.INTERSTITIAL };
        ////Initialize the LevelPlay SDK
        //IronSource.Agent.init("5799531", IronSourceAdUnits.INTERSTITIAL);
        // IronSource.Agent.setMetaData("is_test_suite", "enable");

        LevelPlay.Init("228cc500d", null, levelPlayAdFormats);
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
        CreateInterstitialAd();
    }
    //hJTSK_GMJdko09wKelaedxRo_ljQnkAf

    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }
    public void CreateInterstitialAd()
    {
        //Create InterstitialAd instance
        interstitialAd = new LevelPlayInterstitialAd("l1pfk653qlyw8seu");

        //Subscribe InterstitialAd events
        interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
        interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
        interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
        interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
        interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
        interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
        interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;

    }
    private void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
        Debug.Log(error);
    }

    private void SdkInitializationCompletedEvent(LevelPlayConfiguration configuration)
    {
        Debug.Log("LevelPlay SDK initialized successfully." + configuration);
        LoadInterstitialAd();
        //  IronSource.Agent.launchTestSuite();
    }

    public void LoadInterstitialAd()
    {
        //Load or reload InterstitialAd 	
        interstitialAd.LoadAd();
    }
    public void ShowInterstitialAd()
    {
        //Show InterstitialAd, check if the ad is ready before showing
        if (interstitialAd.IsAdReady())
        {
            interstitialAd.ShowAd();
        }
    }

    //Implement InterstitialAd events
    void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Interstitial ad loaded successfully: " + adInfo.AdUnitId);
        //ShowInterstitialAd();
    }
    void InterstitialOnAdLoadFailedEvent(LevelPlayAdError ironSourceError)
    {
        Debug.LogError("Interstitial ad load failed: " + ironSourceError.ErrorMessage);
    }
    void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo) { }
    void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo) { }
    void InterstitialOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError adInfoError)
    {
        Debug.Log(adInfoError.ToString());
    }
    void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo) { }
    void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo) { }
}
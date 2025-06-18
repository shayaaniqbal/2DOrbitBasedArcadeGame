using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

public class AdMobManager : MonoBehaviour
{
    [Header("Ad Unit IDs - Leave empty for test ads")]
    [SerializeField] private string androidBannerAdUnitId = "";
    [SerializeField] private string iosBannerAdUnitId = "";
    [SerializeField] private string androidInterstitialAdUnitId = "";
    [SerializeField] private string iosInterstitialAdUnitId = "";
    [SerializeField] private string androidRewardedAdUnitId = "";
    [SerializeField] private string iosRewardedAdUnitId = "";
    [SerializeField] private string androidRewardedInterstitialAdUnitId = "";
    [SerializeField] private string iosRewardedInterstitialAdUnitId = "";

    [Header("Settings")]
    [SerializeField] private bool enableTestMode = true;
    [SerializeField] private bool showBannerOnStart = false;
    [SerializeField] private AdPosition bannerPosition = AdPosition.Bottom;

    // Test Ad Unit IDs
    private const string TEST_BANNER_AD_UNIT_ID = "ca-app-pub-3940256099942544/6300978111";
    private const string TEST_INTERSTITIAL_AD_UNIT_ID = "ca-app-pub-3940256099942544/1033173712";
    private const string TEST_REWARDED_AD_UNIT_ID = "ca-app-pub-3940256099942544/5224354917";
    private const string TEST_REWARDED_INTERSTITIAL_AD_UNIT_ID = "ca-app-pub-3940256099942544/5354046379";

    // Ad instances
    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private RewardedInterstitialAd rewardedInterstitialAd;

    // Events
    public static event Action OnInterstitialAdClosed;
    public static event Action OnInterstitialAdFailedToShow;
    public static event Action<Reward> OnRewardedAdRewarded;
    public static event Action OnRewardedAdClosed;
    public static event Action OnRewardedAdFailedToShow;
    public static event Action<Reward> OnRewardedInterstitialAdRewarded;
    public static event Action OnRewardedInterstitialAdClosed;

    // Status properties
    public bool IsBannerLoaded { get; private set; }
    public bool IsInterstitialLoaded { get; private set; }
    public bool IsRewardedLoaded { get; private set; }
    public bool IsRewardedInterstitialLoaded { get; private set; }

    private void Start()
    {
        InitializeAds();
    }

    #region Initialization
    private void InitializeAds()
    {
        Debug.Log("AdMobManager: Initializing Google Mobile Ads...");

        // Initialize the Google Mobile Ads SDK
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMobManager: Google Mobile Ads initialized successfully");
            LogAdapterStatus(initStatus);

            // Load ads after initialization
            LoadAllAds();

            if (showBannerOnStart)
            {
                ShowBanner();
            }
        });

        // Set test device IDs for testing
        if (enableTestMode)
        {
            var requestConfiguration = new RequestConfiguration
            {
                TestDeviceIds = new List<string>() { "ABCDEF012345" } // Add your test device IDs here
            };
            MobileAds.SetRequestConfiguration(requestConfiguration);
        }
    }

    private void LogAdapterStatus(InitializationStatus initStatus)
    {
        var adapterStatusMap = initStatus.getAdapterStatusMap();
        foreach (var adapterStatus in adapterStatusMap)
        {
            Debug.Log($"Adapter: {adapterStatus.Key}, Status: {adapterStatus.Value.InitializationState}, Description: {adapterStatus.Value.Description}");
        }
    }

    private void LoadAllAds()
    {
        LoadBanner();
        LoadInterstitial();
        LoadRewarded();
        LoadRewardedInterstitial();
    }
    #endregion

    #region Ad Unit ID Helpers
    private string GetBannerAdUnitId()
    {
        if (!string.IsNullOrEmpty(androidBannerAdUnitId) && Application.platform == RuntimePlatform.Android)
            return androidBannerAdUnitId;
        if (!string.IsNullOrEmpty(iosBannerAdUnitId) && Application.platform == RuntimePlatform.IPhonePlayer)
            return iosBannerAdUnitId;
        return TEST_BANNER_AD_UNIT_ID;
    }

    private string GetInterstitialAdUnitId()
    {
        if (!string.IsNullOrEmpty(androidInterstitialAdUnitId) && Application.platform == RuntimePlatform.Android)
            return androidInterstitialAdUnitId;
        if (!string.IsNullOrEmpty(iosInterstitialAdUnitId) && Application.platform == RuntimePlatform.IPhonePlayer)
            return iosInterstitialAdUnitId;
        return TEST_INTERSTITIAL_AD_UNIT_ID;
    }

    private string GetRewardedAdUnitId()
    {
        if (!string.IsNullOrEmpty(androidRewardedAdUnitId) && Application.platform == RuntimePlatform.Android)
            return androidRewardedAdUnitId;
        if (!string.IsNullOrEmpty(iosRewardedAdUnitId) && Application.platform == RuntimePlatform.IPhonePlayer)
            return iosRewardedAdUnitId;
        return TEST_REWARDED_AD_UNIT_ID;
    }

    private string GetRewardedInterstitialAdUnitId()
    {
        if (!string.IsNullOrEmpty(androidRewardedInterstitialAdUnitId) && Application.platform == RuntimePlatform.Android)
            return androidRewardedInterstitialAdUnitId;
        if (!string.IsNullOrEmpty(iosRewardedInterstitialAdUnitId) && Application.platform == RuntimePlatform.IPhonePlayer)
            return iosRewardedInterstitialAdUnitId;
        return TEST_REWARDED_INTERSTITIAL_AD_UNIT_ID;
    }
    #endregion

    #region Banner Ads
    public void LoadBanner()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        string adUnitId = GetBannerAdUnitId();
        Debug.Log($"AdMobManager: Loading banner ad with ID: {adUnitId}");

        bannerView = new BannerView(adUnitId, AdSize.Banner, bannerPosition);

        // Register for banner events
        bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
        bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;
        bannerView.OnAdPaid += OnBannerAdPaid;

        // Create ad request
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
    }

    public void ShowBanner()
    {
        if (bannerView != null)
        {
            bannerView.Show();
            Debug.Log("AdMobManager: Banner ad shown");
        }
        else
        {
            Debug.LogWarning("AdMobManager: Banner ad not loaded yet");
            LoadBanner();
        }
    }

    public void HideBanner()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
            Debug.Log("AdMobManager: Banner ad hidden");
        }
    }

    public void DestroyBanner()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
            IsBannerLoaded = false;
            Debug.Log("AdMobManager: Banner ad destroyed");
        }
    }

    // Banner Events
    private void OnBannerAdLoaded()
    {
        IsBannerLoaded = true;
        Debug.Log("AdMobManager: Banner ad loaded successfully");
    }

    private void OnBannerAdLoadFailed(LoadAdError error)
    {
        IsBannerLoaded = false;
        Debug.LogError($"AdMobManager: Banner ad failed to load: {error}");
    }

    private void OnBannerAdPaid(AdValue adValue)
    {
        Debug.Log($"AdMobManager: Banner ad paid: {adValue.Value} {adValue.CurrencyCode}");
    }
    #endregion

    #region Interstitial Ads
    public void LoadInterstitial()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        string adUnitId = GetInterstitialAdUnitId();
        Debug.Log($"AdMobManager: Loading interstitial ad with ID: {adUnitId}");

        AdRequest request = new AdRequest();

        InterstitialAd.Load(adUnitId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                IsInterstitialLoaded = false;
                Debug.LogError($"AdMobManager: Interstitial ad failed to load: {error}");
                return;
            }

            Debug.Log("AdMobManager: Interstitial ad loaded successfully");
            interstitialAd = ad;
            IsInterstitialLoaded = true;
            RegisterInterstitialEvents(interstitialAd);
        });
    }

    public void ShowInterstitial()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            Debug.Log("AdMobManager: Showing interstitial ad");
            interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning("AdMobManager: Interstitial ad not ready to show");
            LoadInterstitial(); // Auto-reload
        }
    }

    private void RegisterInterstitialEvents(InterstitialAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"AdMobManager: Interstitial ad paid: {adValue.Value} {adValue.CurrencyCode}");
        };

        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("AdMobManager: Interstitial ad impression recorded");
        };

        ad.OnAdClicked += () =>
        {
            Debug.Log("AdMobManager: Interstitial ad clicked");
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("AdMobManager: Interstitial ad opened");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("AdMobManager: Interstitial ad closed");
            OnInterstitialAdClosed?.Invoke();
            LoadInterstitial(); // Auto-reload
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"AdMobManager: Interstitial ad failed to show: {error}");
            OnInterstitialAdFailedToShow?.Invoke();
            LoadInterstitial(); // Auto-reload
        };
    }
    #endregion

    #region Rewarded Ads
    public void LoadRewarded()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        string adUnitId = GetRewardedAdUnitId();
        Debug.Log($"AdMobManager: Loading rewarded ad with ID: {adUnitId}");

        AdRequest request = new AdRequest();

        RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                IsRewardedLoaded = false;
                Debug.LogError($"AdMobManager: Rewarded ad failed to load: {error}");
                return;
            }

            Debug.Log("AdMobManager: Rewarded ad loaded successfully");
            rewardedAd = ad;
            IsRewardedLoaded = true;
            RegisterRewardedEvents(rewardedAd);
        });
    }

    public void ShowRewarded()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            Debug.Log("AdMobManager: Showing rewarded ad");
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"AdMobManager: Rewarded ad reward received: {reward.Amount} {reward.Type}");
                OnRewardedAdRewarded?.Invoke(reward);
            });
        }
        else
        {
            Debug.LogWarning("AdMobManager: Rewarded ad not ready to show");
            LoadRewarded(); // Auto-reload
        }
    }

    private void RegisterRewardedEvents(RewardedAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"AdMobManager: Rewarded ad paid: {adValue.Value} {adValue.CurrencyCode}");
        };

        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("AdMobManager: Rewarded ad impression recorded");
        };

        ad.OnAdClicked += () =>
        {
            Debug.Log("AdMobManager: Rewarded ad clicked");
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("AdMobManager: Rewarded ad opened");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("AdMobManager: Rewarded ad closed");
            OnRewardedAdClosed?.Invoke();
            LoadRewarded(); // Auto-reload
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"AdMobManager: Rewarded ad failed to show: {error}");
            OnRewardedAdFailedToShow?.Invoke();
            LoadRewarded(); // Auto-reload
        };
    }
    #endregion

    #region Rewarded Interstitial Ads
    public void LoadRewardedInterstitial()
    {
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }

        string adUnitId = GetRewardedInterstitialAdUnitId();
        Debug.Log($"AdMobManager: Loading rewarded interstitial ad with ID: {adUnitId}");

        AdRequest request = new AdRequest();

        RewardedInterstitialAd.Load(adUnitId, request, (RewardedInterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                IsRewardedInterstitialLoaded = false;
                Debug.LogError($"AdMobManager: Rewarded interstitial ad failed to load: {error}");
                return;
            }

            Debug.Log("AdMobManager: Rewarded interstitial ad loaded successfully");
            rewardedInterstitialAd = ad;
            IsRewardedInterstitialLoaded = true;
            RegisterRewardedInterstitialEvents(rewardedInterstitialAd);
        });
    }

    public void ShowRewardedInterstitial()
    {
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            Debug.Log("AdMobManager: Showing rewarded interstitial ad");
            rewardedInterstitialAd.Show((Reward reward) =>
            {
                Debug.Log($"AdMobManager: Rewarded interstitial ad reward received: {reward.Amount} {reward.Type}");
                OnRewardedInterstitialAdRewarded?.Invoke(reward);
            });
        }
        else
        {
            Debug.LogWarning("AdMobManager: Rewarded interstitial ad not ready to show");
            LoadRewardedInterstitial(); // Auto-reload
        }
    }

    private void RegisterRewardedInterstitialEvents(RewardedInterstitialAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"AdMobManager: Rewarded interstitial ad paid: {adValue.Value} {adValue.CurrencyCode}");
        };

        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("AdMobManager: Rewarded interstitial ad impression recorded");
        };

        ad.OnAdClicked += () =>
        {
            Debug.Log("AdMobManager: Rewarded interstitial ad clicked");
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("AdMobManager: Rewarded interstitial ad opened");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("AdMobManager: Rewarded interstitial ad closed");
            OnRewardedInterstitialAdClosed?.Invoke();
            LoadRewardedInterstitial(); // Auto-reload
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"AdMobManager: Rewarded interstitial ad failed to show: {error}");
            LoadRewardedInterstitial(); // Auto-reload
        };
    }
    #endregion

    #region Public Utility Methods
    public void ReloadAllAds()
    {
        Debug.Log("AdMobManager: Reloading all ads");
        LoadAllAds();
    }

    public void SetBannerPosition(AdPosition position)
    {
        bannerPosition = position;
        if (bannerView != null)
        {
            bannerView.SetPosition(position);
        }
    }

    public bool IsAnyAdLoaded()
    {
        return IsBannerLoaded || IsInterstitialLoaded || IsRewardedLoaded || IsRewardedInterstitialLoaded;
    }

    public void LogAdStatus()
    {
        Debug.Log($"AdMobManager Status - Banner: {IsBannerLoaded}, Interstitial: {IsInterstitialLoaded}, Rewarded: {IsRewardedLoaded}, RewardedInterstitial: {IsRewardedInterstitialLoaded}");
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        DestroyBanner();

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }
    }
    #endregion

    #region Editor Testing (Only works in Unity Editor)
#if UNITY_EDITOR
    [Header("Editor Testing")]
    [SerializeField] private bool enableEditorTesting = true;

    [ContextMenu("Test Show Banner")]
    private void TestShowBanner()
    {
        if (enableEditorTesting)
        {
            Debug.Log("Editor Test: Showing Banner Ad");
            ShowBanner();
        }
    }

    [ContextMenu("Test Show Interstitial")]
    private void TestShowInterstitial()
    {
        if (enableEditorTesting)
        {
            Debug.Log("Editor Test: Showing Interstitial Ad");
            ShowInterstitial();
        }
    }

    [ContextMenu("Test Show Rewarded")]
    private void TestShowRewarded()
    {
        if (enableEditorTesting)
        {
            Debug.Log("Editor Test: Showing Rewarded Ad");
            ShowRewarded();
        }
    }

    [ContextMenu("Test Show Rewarded Interstitial")]
    private void TestShowRewardedInterstitial()
    {
        if (enableEditorTesting)
        {
            Debug.Log("Editor Test: Showing Rewarded Interstitial Ad");
            ShowRewardedInterstitial();
        }
    }

    [ContextMenu("Test Load All Ads")]
    private void TestLoadAllAds()
    {
        if (enableEditorTesting)
        {
            Debug.Log("Editor Test: Loading All Ads");
            LoadAllAds();
        }
    }

    [ContextMenu("Test Log Ad Status")]
    private void TestLogAdStatus()
    {
        if (enableEditorTesting)
        {
            LogAdStatus();
        }
    }
#endif
    #endregion
}
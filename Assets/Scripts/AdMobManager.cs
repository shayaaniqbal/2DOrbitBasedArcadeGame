using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

public class AdMobManager : MonoBehaviour
{
    [System.Serializable]
    public class PlatformAdSettings
    {
        [Header("Banner Ad")]
        public bool enableBanner = false;
        [SerializeField] private string bannerAdUnitId = "";

        [Header("Interstitial Ad")]
        public bool enableInterstitial = false;
        [SerializeField] private string interstitialAdUnitId = "";

        [Header("Rewarded Ad")]
        public bool enableRewarded = false;
        [SerializeField] private string rewardedAdUnitId = "";

        [Header("Rewarded Interstitial Ad")]
        public bool enableRewardedInterstitial = false;
        [SerializeField] private string rewardedInterstitialAdUnitId = "";

        public string GetBannerAdUnitId() => bannerAdUnitId;
        public string GetInterstitialAdUnitId() => interstitialAdUnitId;
        public string GetRewardedAdUnitId() => rewardedAdUnitId;
        public string GetRewardedInterstitialAdUnitId() => rewardedInterstitialAdUnitId;
    }

    [Header("Android Settings")]
    [SerializeField] private PlatformAdSettings androidSettings = new PlatformAdSettings();

    [Header("iOS Settings")]
    [SerializeField] private PlatformAdSettings iosSettings = new PlatformAdSettings();

    [Header("General Settings")]
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

    // Current platform settings
    private PlatformAdSettings CurrentPlatformSettings
    {
        get
        {
#if UNITY_ANDROID
            return androidSettings;
#elif UNITY_IOS
            return iosSettings;
#else
            return Application.platform == RuntimePlatform.Android ? androidSettings : iosSettings;
#endif
        }
    }
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
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

            // Load ads after initialization based on platform settings
            LoadEnabledAds();

            if (showBannerOnStart && CurrentPlatformSettings.enableBanner)
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

    private void LoadEnabledAds()
    {
        var settings = CurrentPlatformSettings;

        if (settings.enableBanner)
            LoadBanner();
        else
            Debug.Log("AdMobManager: Banner ads disabled for current platform");

        if (settings.enableInterstitial)
            LoadInterstitial();
        else
            Debug.Log("AdMobManager: Interstitial ads disabled for current platform");

        if (settings.enableRewarded)
            LoadRewarded();
        else
            Debug.Log("AdMobManager: Rewarded ads disabled for current platform");

        if (settings.enableRewardedInterstitial)
            LoadRewardedInterstitial();
        else
            Debug.Log("AdMobManager: Rewarded Interstitial ads disabled for current platform");
    }
    #endregion

    #region Ad Unit ID Helpers
    private string GetBannerAdUnitId()
    {
        var settings = CurrentPlatformSettings;
        if (!settings.enableBanner) return null;

        string adUnitId = settings.GetBannerAdUnitId();
        return !string.IsNullOrEmpty(adUnitId) ? adUnitId : TEST_BANNER_AD_UNIT_ID;
    }

    private string GetInterstitialAdUnitId()
    {
        var settings = CurrentPlatformSettings;
        if (!settings.enableInterstitial) return null;

        string adUnitId = settings.GetInterstitialAdUnitId();
        return !string.IsNullOrEmpty(adUnitId) ? adUnitId : TEST_INTERSTITIAL_AD_UNIT_ID;
    }

    private string GetRewardedAdUnitId()
    {
        var settings = CurrentPlatformSettings;
        if (!settings.enableRewarded) return null;

        string adUnitId = settings.GetRewardedAdUnitId();
        return !string.IsNullOrEmpty(adUnitId) ? adUnitId : TEST_REWARDED_AD_UNIT_ID;
    }

    private string GetRewardedInterstitialAdUnitId()
    {
        var settings = CurrentPlatformSettings;
        if (!settings.enableRewardedInterstitial) return null;

        string adUnitId = settings.GetRewardedInterstitialAdUnitId();
        return !string.IsNullOrEmpty(adUnitId) ? adUnitId : TEST_REWARDED_INTERSTITIAL_AD_UNIT_ID;
    }
    #endregion

    #region Banner Ads
    public void LoadBanner()
    {
        if (!CurrentPlatformSettings.enableBanner)
        {
            Debug.LogWarning("AdMobManager: Banner ads are disabled for current platform");
            return;
        }

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
        if (!CurrentPlatformSettings.enableBanner)
        {
            Debug.LogWarning("AdMobManager: Banner ads are disabled for current platform");
            return;
        }

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
        if (!CurrentPlatformSettings.enableInterstitial)
        {
            Debug.LogWarning("AdMobManager: Interstitial ads are disabled for current platform");
            return;
        }

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
        if (!CurrentPlatformSettings.enableInterstitial)
        {
            Debug.LogWarning("AdMobManager: Interstitial ads are disabled for current platform");
            return;
        }

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
        if (!CurrentPlatformSettings.enableRewarded)
        {
            Debug.LogWarning("AdMobManager: Rewarded ads are disabled for current platform");
            return;
        }

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
        if (!CurrentPlatformSettings.enableRewarded)
        {
            Debug.LogWarning("AdMobManager: Rewarded ads are disabled for current platform");
            return;
        }

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
        if (!CurrentPlatformSettings.enableRewardedInterstitial)
        {
            Debug.LogWarning("AdMobManager: Rewarded Interstitial ads are disabled for current platform");
            return;
        }

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
        if (!CurrentPlatformSettings.enableRewardedInterstitial)
        {
            Debug.LogWarning("AdMobManager: Rewarded Interstitial ads are disabled for current platform");
            return;
        }

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
        Debug.Log("AdMobManager: Reloading all enabled ads");
        LoadEnabledAds();
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
        var settings = CurrentPlatformSettings;
        string platform = Application.platform == RuntimePlatform.Android ? "Android" : "iOS";

        Debug.Log($"AdMobManager Status ({platform}):");
        Debug.Log($"  Banner: Enabled={settings.enableBanner}, Loaded={IsBannerLoaded}");
        Debug.Log($"  Interstitial: Enabled={settings.enableInterstitial}, Loaded={IsInterstitialLoaded}");
        Debug.Log($"  Rewarded: Enabled={settings.enableRewarded}, Loaded={IsRewardedLoaded}");
        Debug.Log($"  RewardedInterstitial: Enabled={settings.enableRewardedInterstitial}, Loaded={IsRewardedInterstitialLoaded}");
    }

    public PlatformAdSettings GetCurrentPlatformSettings()
    {
        return CurrentPlatformSettings;
    }

    public bool IsAdTypeEnabled(AdType adType)
    {
        var settings = CurrentPlatformSettings;
        switch (adType)
        {
            case AdType.Banner:
                return settings.enableBanner;
            case AdType.Interstitial:
                return settings.enableInterstitial;
            case AdType.Rewarded:
                return settings.enableRewarded;
            case AdType.RewardedInterstitial:
                return settings.enableRewardedInterstitial;
            default:
                return false;
        }
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
            Debug.Log("Editor Test: Loading All Enabled Ads");
            LoadEnabledAds();
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

    #region Enums
    public enum AdType
    {
        Banner,
        Interstitial,
        Rewarded,
        RewardedInterstitial
    }
    #endregion
}
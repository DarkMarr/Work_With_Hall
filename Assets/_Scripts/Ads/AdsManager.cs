using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using QuizGame.Utilities;
using Unity.Services.LevelPlay;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace QuizGame.Ads
{
    public class AdsManager : MonoSingleton<AdsManager>
    {
        public class RewardedAdShowCallbacks
        {
            public Action<LevelPlayAdInfo> OnAdDisplayed;
            public Action<LevelPlayAdInfo, LevelPlayAdError> OnAdDisplayFailed;
            public Action<LevelPlayAdInfo, LevelPlayReward> OnAdRewarded;
            public Action<LevelPlayAdInfo> OnAdClicked;
            public Action<LevelPlayAdInfo> OnAdClosed;
            public Action<LevelPlayAdInfo> OnAdInfoChanged;
        }

        public class InterstitialAdShowCallbacks
        {
            public Action<LevelPlayAdInfo> OnAdDisplayed;
            public Action<LevelPlayAdInfo, LevelPlayAdError> OnAdDisplayFailed;
            public Action<LevelPlayAdInfo> OnAdClicked;
            public Action<LevelPlayAdInfo> OnAdClosed;
            public Action<LevelPlayAdInfo> OnAdInfoChanged;
        }

        public class BannerAdShowCallbacks
        {
            public Action<LevelPlayAdInfo> OnAdClicked;
            public Action<LevelPlayAdInfo> OnAdDisplayed;
            public Action<LevelPlayAdInfo, LevelPlayAdError> OnAdDisplayFailed;
            public Action<LevelPlayAdInfo> OnAdCollapsed;
            public Action<LevelPlayAdInfo> OnAdLeftApplication;
            public Action<LevelPlayAdInfo> OnAdExpanded;
        }
        public bool isAdsEnabled { get; private set; } = false;

        [SerializeField]
        private bool isProductionAds = true;

        [SerializeField]
        private AdsConfigSO adsConfigSO;

        [SerializeField]
        private AdsConfigSO mockupAdsConfigSO;

        private LevelPlayRewardedAd rewardedVideoAd;
        private LevelPlayInterstitialAd interstitialAd;
        private LevelPlayBannerAd bannerAd;

        private RewardedAdShowCallbacks rewardedAdShowCallbacks = new();
        private InterstitialAdShowCallbacks interstitialAdShowCallbacks = new();
        private BannerAdShowCallbacks bannerAdShowCallbacks = new();

        protected override void Awake()
        {
            base.Awake();
            InitializeSDK();
        }

        public bool IsRewardedAdAvailable()
        {
            return isAdsEnabled && rewardedVideoAd.IsAdReady();
        }

        public bool IsInterstitialAdAvailable()
        {
            return isAdsEnabled && interstitialAd.IsAdReady();
        }

        public bool IsShowBannerAdAvailable()
        {
            return isAdsEnabled;
        }

        public void ShowRewardedAd(RewardedAdShowCallbacks callbacks)
        {
            if (!isAdsEnabled)
            {
                Debug.LogWarning("[AdsManager] Ads not enabled yet.");
                return;
            }

            if (!rewardedVideoAd.IsAdReady())
            {
                Debug.LogWarning("[AdsManager] Rewarded video ad is not ready yet.");
                TryLoadRewardedAd();
                return;
            }
            rewardedAdShowCallbacks = null;
            rewardedVideoAd.ShowAd();

            if (callbacks != null)
            {
                rewardedAdShowCallbacks = callbacks;
            }
        }

        public void ShowInterstitialAd(InterstitialAdShowCallbacks callbacks)
        {
            if (!isAdsEnabled)
            {
                Debug.LogWarning("[AdsManager] Ads not enabled yet.");
                return;
            }

            if (!interstitialAd.IsAdReady())
            {
                Debug.LogWarning("[AdsManager] Interstitial ad is not ready yet.");
                TryLoadInterstitialAd();
                return;
            }
            interstitialAdShowCallbacks = null;
            interstitialAd.ShowAd();

            if (callbacks != null)
            {
                interstitialAdShowCallbacks = callbacks;
            }
        }

        public void ShowBannerAd(BannerAdShowCallbacks callbacks)
        {
            if (!isAdsEnabled)
            {
                Debug.LogWarning("[AdsManager] Ads not enabled yet.");
                return;
            }
            bannerAdShowCallbacks = null;
            Debug.Log("[AdsManager] LoadBannerAd");
            bannerAd.LoadAd();
            bannerAd.ShowAd();

            if (callbacks != null)
            {
                bannerAdShowCallbacks = callbacks;
            }
        }

        public void HideBannerAd()
        {
            if (!isAdsEnabled)
            {
                Debug.LogWarning("[AdsManager] Ads not enabled yet.");
                return;
            }

            bannerAd.HideAd();
        }

        public void TryLoadRewardedAd()
        {
            Debug.Log("[AdsManager] TryLoadRewardedAd");
            if (!rewardedVideoAd.IsAdReady())
            {
                rewardedVideoAd.LoadAd();
            }
        }

        public void TryLoadInterstitialAd()
        {
            Debug.Log("[AdsManager] TryLoadInterstitialAd");
            if (!interstitialAd.IsAdReady())
            {
                interstitialAd.LoadAd();
            }
        }

        private void InitializeSDK()
        {
            Debug.Log("[AdsManager] LevelPlay.ValidateIntegration");
            LevelPlay.ValidateIntegration();

            LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

            LevelPlay.Init(isProductionAds ? adsConfigSO.GetAppKey() : mockupAdsConfigSO.GetAppKey());
        }

        private void EnableAds()
        {
            Debug.Log("[AdsManager] Enable Ads");
            LevelPlay.OnImpressionDataReady += ImpressionDataReadyEvent;

            rewardedVideoAd = new LevelPlayRewardedAd(isProductionAds ? adsConfigSO.GetRewardedVideoAdUnitId() : mockupAdsConfigSO.GetRewardedVideoAdUnitId());
            rewardedVideoAd.OnAdLoaded += RewardedVideoOnLoadedEvent;
            rewardedVideoAd.OnAdLoadFailed += RewardedVideoOnAdLoadFailedEvent;
            rewardedVideoAd.OnAdDisplayed += RewardedVideoOnAdDisplayedEvent;
            rewardedVideoAd.OnAdDisplayFailed += RewardedVideoOnAdDisplayedFailedEvent;
            rewardedVideoAd.OnAdRewarded += RewardedVideoOnAdRewardedEvent;
            rewardedVideoAd.OnAdClicked += RewardedVideoOnAdClickedEvent;
            rewardedVideoAd.OnAdClosed += RewardedVideoOnAdClosedEvent;
            rewardedVideoAd.OnAdInfoChanged += RewardedVideoOnAdInfoChangedEvent;
            TryLoadRewardedAd();


            interstitialAd = new LevelPlayInterstitialAd(isProductionAds ? adsConfigSO.GetInterstitialAdUnitId() : mockupAdsConfigSO.GetInterstitialAdUnitId());
            interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
            interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
            interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
            interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
            interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
            interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
            interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;
            TryLoadInterstitialAd();

            bannerAd = new LevelPlayBannerAd(isProductionAds ? adsConfigSO.GetBannerAdUnitId() : mockupAdsConfigSO.GetBannerAdUnitId());
            bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
            bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
            bannerAd.OnAdClicked += BannerOnAdClickedEvent;
            bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
            bannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
            bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
            bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
            bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;
        }

        #region SDK Initialization Callbacks

        void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
        {
            Debug.Log($"[AdsManager] Received SdkInitializationCompletedEvent with Config: {config}");
            EnableAds();
            isAdsEnabled = true;
        }

        void SdkInitializationFailedEvent(LevelPlayInitError error)
        {
            Debug.Log($"[AdsManager] Received SdkInitializationFailedEvent with Error: {error}");
        }

        #endregion

        #region Rewarded Video Ad Callbacks

        void RewardedVideoOnLoadedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received RewardedVideoOnLoadedEvent With AdInfo: {adInfo}");
        }

        void RewardedVideoOnAdLoadFailedEvent(LevelPlayAdError error)
        {
            Debug.Log($"[AdsManager] Received RewardedVideoOnAdLoadFailedEvent With Error: {error}");
            TryLoadRewardedAd();
        }

        void RewardedVideoOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received RewardedVideoOnAdDisplayedEvent With AdInfo: {adInfo}");
            rewardedAdShowCallbacks?.OnAdDisplayed?.Invoke(adInfo);
        }

        void RewardedVideoOnAdDisplayedFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            Debug.Log($"[AdsManager] Received RewardedVideoOnAdDisplayedFailedEvent With AdInfo: {adInfo} and Error: {error}");
            rewardedAdShowCallbacks?.OnAdDisplayFailed?.Invoke(adInfo, error);
        }

        void RewardedVideoOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward)
        {
            Debug.Log($"[AdsManager] Received RewardedVideoOnAdRewardedEvent With AdInfo: {adInfo} and Reward: {reward}");
            rewardedAdShowCallbacks?.OnAdRewarded?.Invoke(adInfo, reward);
        }

        void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received RewardedVideoOnAdClickedEvent With AdInfo: {adInfo}");
            rewardedAdShowCallbacks?.OnAdClicked?.Invoke(adInfo);
        }

        void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received RewardedVideoOnAdClosedEvent With AdInfo: {adInfo}");
            rewardedAdShowCallbacks?.OnAdClosed?.Invoke(adInfo);
            TryLoadRewardedAd();
        }

        void RewardedVideoOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received RewardedVideoOnAdInfoChangedEvent With AdInfo {adInfo}");
            rewardedAdShowCallbacks?.OnAdInfoChanged?.Invoke(adInfo);
        }

        #endregion

        #region Interstitial Ad Callbacks

        void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received InterstitialOnAdLoadedEvent With AdInfo: {adInfo}");
        }

        void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
        {
            Debug.Log($"[AdsManager] Received InterstitialOnAdLoadFailedEvent With Error: {error}");
            TryLoadInterstitialAd();
        }

        void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received InterstitialOnAdDisplayedEvent With AdInfo: {adInfo}");
            interstitialAdShowCallbacks?.OnAdDisplayed?.Invoke(adInfo);
        }

        void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            Debug.Log($"[AdsManager] Received InterstitialOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
            interstitialAdShowCallbacks?.OnAdDisplayFailed?.Invoke(adInfo, error);
        }

        void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received InterstitialOnAdClickedEvent With AdInfo: {adInfo}");
            interstitialAdShowCallbacks?.OnAdClicked?.Invoke(adInfo);
        }

        void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received InterstitialOnAdClosedEvent With AdInfo: {adInfo}");
            interstitialAdShowCallbacks?.OnAdClosed?.Invoke(adInfo);
            TryLoadInterstitialAd();
        }

        void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received InterstitialOnAdInfoChangedEvent With AdInfo: {adInfo}");
            interstitialAdShowCallbacks?.OnAdInfoChanged?.Invoke(adInfo);
        }

        #endregion

        #region Banner Ad Callbacks

        void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received BannerOnAdLoadedEvent With AdInfo: {adInfo}");
        }

        void BannerOnAdLoadFailedEvent(LevelPlayAdError error)
        {
            Debug.Log($"[AdsManager] Received BannerOnAdLoadFailedEvent With Error: {error}");
        }

        void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received BannerOnAdClickedEvent With AdInfo: {adInfo}");
            bannerAdShowCallbacks?.OnAdClicked?.Invoke(adInfo);
        }

        void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received BannerOnAdDisplayedEvent With AdInfo: {adInfo}");
            bannerAdShowCallbacks?.OnAdDisplayed?.Invoke(adInfo);
        }

        void BannerOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            Debug.Log($"[AdsManager] Received BannerOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
            bannerAdShowCallbacks?.OnAdDisplayFailed?.Invoke(adInfo, error);
        }

        void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received BannerOnAdCollapsedEvent With AdInfo: {adInfo}");
            bannerAdShowCallbacks?.OnAdCollapsed?.Invoke(adInfo);
        }

        void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received BannerOnAdLeftApplicationEvent With AdInfo: {adInfo}");
            bannerAdShowCallbacks?.OnAdLeftApplication?.Invoke(adInfo);
        }

        void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo)
        {
            Debug.Log($"[AdsManager] Received BannerOnAdExpandedEvent With AdInfo: {adInfo}");
            bannerAdShowCallbacks?.OnAdExpanded?.Invoke(adInfo);
        }

        #endregion

        void ImpressionDataReadyEvent(LevelPlayImpressionData impressionData)
        {
            Debug.Log($"[AdsManager] Received ImpressionDataReadyEvent ToString(): {impressionData}");
            Debug.Log($"[AdsManager] Received ImpressionDataReadyEvent allData: {impressionData.AllData}");
        }
    }
}

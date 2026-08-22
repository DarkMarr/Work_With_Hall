using System;
using NaughtyAttributes;
using UnityEngine;

namespace QuizGame.Ads
{
    [CreateAssetMenu(fileName = "AdsConfigDataSO", menuName = "QuizGame/AdsConfigDataSO")]
    public class AdsConfigSO : ScriptableObject
    {
        [Serializable]
        public struct AdsConfigData
        {
            public string appKey;
            public string bannerAdUnitId;
            public string interstitialAdUnitId;
            public string rewardedVideoAdUnitId;
        }

        [SerializeField]
        private AdsConfigData androidAdsConfig;

        [SerializeField]
        private AdsConfigData iosAdsConfig;

        public AdsConfigData GetAdsConfigData()
        {
#if UNITY_ANDROID
            return androidAdsConfig;
#elif UNITY_IPHONE
            return iosAdsConfig;
#else
            return androidAdsConfig;
#endif
        }

        public string GetAppKey()
        {
#if UNITY_ANDROID
            return androidAdsConfig.appKey;
#elif UNITY_IPHONE
            return iosAdsConfig.appKey;
#else
            return androidAdsConfig.appKey;
#endif
        }

        public string GetBannerAdUnitId()
        {
#if UNITY_ANDROID
            return androidAdsConfig.bannerAdUnitId;
#elif UNITY_IPHONE
            return iosAdsConfig.bannerAdUnitId;
#else
            return androidAdsConfig.bannerAdUnitId;
#endif
        }

        public string GetInterstitialAdUnitId()
        {
#if UNITY_ANDROID
            return androidAdsConfig.interstitialAdUnitId;
#elif UNITY_IPHONE
            return iosAdsConfig.interstitialAdUnitId;
#else
            return androidAdsConfig.interstitialAdUnitId;
#endif
        }

        public string GetRewardedVideoAdUnitId()
        {
#if UNITY_ANDROID
            return androidAdsConfig.rewardedVideoAdUnitId;
#elif UNITY_IPHONE
            return iosAdsConfig.rewardedVideoAdUnitId;
#else
            return androidAdsConfig.rewardedVideoAdUnitId;
#endif
        }
    }
}
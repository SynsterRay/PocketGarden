using UnityEngine;
using PocketGarden.Core;
using PocketGarden.Quests;

namespace PocketGarden.Ads
{
    /// <summary>
    /// Ads facade. Compiles with or without the Google Mobile Ads SDK:
    ///   • Define <c>POCKETGARDEN_ADS</c> (Project Settings → Player → Scripting Define Symbols)
    ///     AFTER importing the Google Mobile Ads SDK to enable real ads.
    ///   • Without the define, all methods are safe no-ops: rewarded calls invoke onFailed so the
    ///     UI shows a graceful "not ready" state.
    /// Ad Unit IDs come from <see cref="ShopCatalog"/> (AD_BANNER / AD_INTERSTITIAL / AD_REWARDED).
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        private const string QuestsSinceAdKey = "PG_QuestsSinceAd";
        private const int QuestsBeforeAd = 3;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            QuestManager.OnQuestComplete += OnQuestComplete;
            InitAds();
        }

        private void OnDestroy()
        {
            QuestManager.OnQuestComplete -= OnQuestComplete;
            DisposeAds();
        }

        private void OnQuestComplete(Quest q)
        {
            int n = PlayerPrefs.GetInt(QuestsSinceAdKey, 0) + 1;
            if (n >= QuestsBeforeAd) { n = 0; ShowInterstitial(); }
            PlayerPrefs.SetInt(QuestsSinceAdKey, n);
            PlayerPrefs.Save();
        }

#if POCKETGARDEN_ADS
        private GoogleMobileAds.Api.BannerView _banner;
        private GoogleMobileAds.Api.InterstitialAd _interstitial;
        private GoogleMobileAds.Api.RewardedAd _rewarded;

        public bool IsRewardedReady => _rewarded != null && _rewarded.CanShowAd();

        private void InitAds()
        {
            GoogleMobileAds.Api.MobileAds.Initialize(_ =>
            {
                LoadInterstitial();
                LoadRewarded();
                ShowBanner();
            });
        }

        private void DisposeAds()
        {
            _banner?.Destroy();
            _interstitial?.Destroy();
            _rewarded?.Destroy();
        }

        public void ShowBanner()
        {
            if (_banner != null) { _banner.Show(); return; }
            _banner = new GoogleMobileAds.Api.BannerView(ShopCatalog.AD_BANNER,
                GoogleMobileAds.Api.AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(GoogleMobileAds.Api.AdSize.FullWidth),
                GoogleMobileAds.Api.AdPosition.Bottom);
            _banner.LoadAd(new GoogleMobileAds.Api.AdRequest());
        }

        public void HideBanner() => _banner?.Hide();

        private void LoadInterstitial()
        {
            _interstitial?.Destroy();
            _interstitial = null;
            GoogleMobileAds.Api.InterstitialAd.Load(ShopCatalog.AD_INTERSTITIAL, new GoogleMobileAds.Api.AdRequest(), (ad, err) =>
            {
                if (err != null || ad == null) return;
                _interstitial = ad;
                ad.OnAdFullScreenContentClosed += LoadInterstitial;
                ad.OnAdFullScreenContentFailed += _ => LoadInterstitial();
            });
        }

        public void ShowInterstitial()
        {
            if (_interstitial != null && _interstitial.CanShowAd()) _interstitial.Show();
            else LoadInterstitial();
        }

        private void LoadRewarded()
        {
            _rewarded?.Destroy();
            _rewarded = null;
            GoogleMobileAds.Api.RewardedAd.Load(ShopCatalog.AD_REWARDED, new GoogleMobileAds.Api.AdRequest(), (ad, err) =>
            {
                if (err != null || ad == null) return;
                _rewarded = ad;
                ad.OnAdFullScreenContentClosed += LoadRewarded;
                ad.OnAdFullScreenContentFailed += _ => LoadRewarded();
            });
        }

        public void ShowRewarded(System.Action onReward, System.Action onFailed = null)
        {
            if (_rewarded != null && _rewarded.CanShowAd())
                _rewarded.Show(_ => onReward?.Invoke());
            else
                onFailed?.Invoke();
        }
#else
        // Fallback (SDK not installed): safe no-ops.
        public bool IsRewardedReady => false;
        private void InitAds() { }
        private void DisposeAds() { }
        public void ShowBanner() { }
        public void HideBanner() { }
        public void ShowInterstitial() { }
        public void ShowRewarded(System.Action onReward, System.Action onFailed = null) => onFailed?.Invoke();
#endif
    }
}

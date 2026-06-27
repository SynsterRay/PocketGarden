using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace PocketGarden.Core
{
    /// <summary>
    /// Unity Purchasing integration. Products are declared from <see cref="ShopCatalog"/> so the
    /// catalog stays the single source of truth. On a validated purchase, contents are granted via
    /// <see cref="ShopCatalog.GrantPurchase"/>.
    ///
    /// Product types:
    ///   Energy / Gem / growth_bundle → Consumable (repeatable)
    ///   starter                      → Non-consumable (one-time, ownership tracked by the store)
    ///   vip                          → Subscription
    /// </summary>
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        public static IAPManager Instance { get; private set; }

        private IStoreController _store;
        private Action<bool> _pendingCallback;

        public bool IsInitialized => _store != null;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var item in ShopCatalog.Items)
            {
                if (string.IsNullOrEmpty(item.iapProductId)) continue;
                builder.AddProduct(item.iapProductId, ProductTypeFor(item));
            }
            UnityPurchasing.Initialize(this, builder);
        }

        private static ProductType ProductTypeFor(ShopItem item)
        {
            if (item.type == ShopItemType.VIP) return ProductType.Subscription;
            if (item.id == "starter") return ProductType.NonConsumable;
            return ProductType.Consumable;
        }

        /// <summary>Starts a purchase. <paramref name="callback"/> receives success/failure.</summary>
        public void BuyProduct(string productId, Action<bool> callback)
        {
            if (_store == null || string.IsNullOrEmpty(productId)) { callback?.Invoke(false); return; }
            _pendingCallback = callback;
            _store.InitiatePurchase(productId);
        }

        /// <summary>True if a non-consumable / subscription is already owned (e.g. the Starter Pack).</summary>
        public bool IsOwned(string productId)
        {
            var p = _store?.products?.WithID(productId);
            return p != null && p.hasReceipt;
        }

        public string GetLocalizedPrice(string productId)
        {
            var p = _store?.products?.WithID(productId);
            return p?.metadata?.localizedPriceString;
        }

        // --- IStoreListener ---

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) => _store = controller;
        public void OnInitializeFailed(InitializationFailureReason error) => Debug.LogWarning($"[IAP] Init failed: {error}");
        public void OnInitializeFailed(InitializationFailureReason error, string message) => Debug.LogWarning($"[IAP] Init failed: {error} - {message}");

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string id = args.purchasedProduct.definition.id;
            var item = ShopCatalog.Items.Find(i => i.iapProductId == id);
            if (item != null)
            {
                ShopCatalog.GrantPurchase(item);
                if (item.id == "starter")
                {
                    PlayerPrefs.SetInt("PG_StarterOffered", 1); // never re-offer once owned
                    PlayerPrefs.Save();
                }
            }
            _pendingCallback?.Invoke(true);
            _pendingCallback = null;
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} - {reason}");
            _pendingCallback?.Invoke(false);
            _pendingCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription desc)
        {
            Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} - {desc.message}");
            _pendingCallback?.Invoke(false);
            _pendingCallback = null;
        }
    }
}

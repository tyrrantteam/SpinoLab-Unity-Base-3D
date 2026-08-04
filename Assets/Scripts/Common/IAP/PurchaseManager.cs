using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Base.Core.Debug;
using Common.IAP;
using DataAccount;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

internal class PurchaseManager : MonoBehaviour
{
    [CanBeNull] private    StoreController                           _controller;
    public static readonly PurchaseManager                           Instance               = new();
    private readonly       Dictionary<string, Action<PurchaseState>> _purchaseStates        = new();
    public                 List<string>                              processProductsSuccess = new();

    [SerializeField] private DataBundleController dataBundle;
    [SerializeField] private DataPurchase         dataPurchase;

    
    private PurchaseManager()
    {
    }

    /// <summary>
    /// Khởi tạo PurchaseManager: kết nối tới store (Google Play / Apple),
    /// đăng ký toàn bộ callback và fetch danh sách sản phẩm cần bán.
    /// </summary>
    /// <param name="productDefinitions">Danh sách ProductDefinition build từ DataBundleController</param>
    public void Init(List<ProductDefinition> productDefinitions)
    {
        StartCoroutine(StartInitPurchase(productDefinitions));
    }

    /// <summary>
    /// Coroutine khởi tạo:
    /// 1. Lấy StoreController từ UnityIAPServices
    /// 2. Connect() tới store (hiện popup đăng nhập nếu cần)
    /// 3. Đăng ký toàn bộ callback (fetch sản phẩm, purchase, entitlement...)
    /// 4. FetchProducts() gửi danh sách ProductDefinition lên store để lấy metadata
    /// </summary>
    private IEnumerator StartInitPurchase(List<ProductDefinition> productDefinitions)
    {
        _controller = UnityIAPServices.StoreController();
        yield return _controller.Connect();
        _controller.OnProductsFetched      += OnProductsFetched;
        _controller.OnProductsFetchFailed  += OnProductsFetchFailed;
        _controller.OnPurchasesFetched     += OnPurchasesFetched;
        _controller.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
        _controller.OnStoreDisconnected    += OnStoreDisconnected;

        _controller.OnPurchasePending   += OnPurchasePending;
        _controller.OnPurchaseFailed    += OnPurchaseFailed;
        _controller.OnPurchaseConfirmed += OnPurchaseConfirmed;
        _controller.OnPurchaseDeferred  += OnPurchaseDeferred;
        _controller.OnCheckEntitlement  += OnCheckEntitlement;

        _controller.FetchProducts(productDefinitions);
    }

    /// <summary>
    /// Callback khi store trả về danh sách sản phẩm thành công.
    /// 1. FetchPurchases() để lấy lịch sử giao dịch chưa confirm
    /// 2. Đồng bộ localizedPrice (vd: "1.99$") từ store vào từng bundle data để UI hiển thị
    /// </summary>
    private void OnProductsFetched(List<Product> products)
    {
        GameDebug.Log("PurchaseManager: On product fetched successfully!");
        _controller?.FetchPurchases();
        var allBundle = dataBundle.GetAllBundle();
        foreach (var product in products)
        {
            GameDebug.Log(
                $"PurchaseManager: Product ID: {product.definition.id}, price local {product.metadata.localizedPriceString}");
            foreach (var bundle in allBundle)
            {
                var bundleData = bundle[0];
                if (bundleData.idBundle == product.definition.id)
                {
                    bundleData.InitLocalPrice(product.metadata.localizedPriceString);
                }
            }
        }
    }

    /// <summary>
    /// Callback khi user đã hoàn tất thanh toán trên store (Google/Apple popup).
    /// 1. Validate receipt bằng CrossPlatformValidator (chống gian lận)
    /// 2. Nếu valid: gọi callback PurchaseState.Success và log analytic
    ///    Nếu invalid: gọi callback PurchaseState.Failed
    /// 3. ConfirmPurchase() để xác nhận giao dịch với store
    /// </summary>
    private void OnPurchasePending(PendingOrder pendingOrder)
    {
        var product = GetFirstProductInOrder(pendingOrder);
        var productId = product.definition.id;
        GameDebug.Log($"PurchaseManager: On purchase pending description {productId}");
        if (_controller == null)
        {
            GameDebug.LogError("PurchaseManager: Purchase pending but controller is null");
            return;
        }

        var receipt = pendingOrder.Info.Receipt;
        var validPurchase = true;
        var validator = new CrossPlatformValidator(dataPurchase.GoogleData(),
            dataPurchase.AppleData(), Application.identifier);

        try
        {
            validator.Validate(receipt);
        }
        catch (IAPSecurityException)
        {
            GameDebug.LogError("PurchaseManager:Invalid receipt, not unlocking content");
            validPurchase = false;
        }

        if (validPurchase)
        {
            if (product.definition.type != ProductType.Consumable)
            {
                // PreferenceService.Instance.SetIsPurchased(productId, true);
            }

            if (_purchaseStates.TryGetValue(productId, out var purchaseState))
            {
                AnlyticManager.Instance.LogPurchase(product.metadata.isoCurrencyCode, product.metadata.localizedPrice);

                purchaseState.Invoke(PurchaseState.Success);
                _purchaseStates.Remove(productId);
            }
            else
            {
                processProductsSuccess.Add(productId);
            }
        }
        else
        {
            if (_purchaseStates.TryGetValue(productId, out var purchaseState))
            {
                purchaseState.Invoke(PurchaseState.Failed);
                _purchaseStates.Remove(productId);
            }
        }

        _controller?.ConfirmPurchase(pendingOrder);
    }


    /// <summary>
    /// Callback khi fetch lịch sử giao dịch từ store thành công.
    /// Duyệt toàn bộ sản phẩm NonConsumable và gọi CheckEntitlement()
    /// để kiểm tra user có sở hữu sản phẩm nào không.
    /// </summary>
    private void OnPurchasesFetched(Orders orders)
    {
        var confirmedOrders = orders.ConfirmedOrders;
        GameDebug.Log($"PurchaseManager: On Purchases Fetched successfully with {confirmedOrders.Count} confirmed orders!, checking entitlement");
        if (_controller == null)
        {
            GameDebug.LogError("PurchaseManager: Purchase fetched but controller is null");
            return;
        }

        foreach (var product in _controller.GetProducts())
        {
            if (product.definition.type == ProductType.Consumable) continue;
            _controller.CheckEntitlement(product);
        }
    }

    /// <summary>
    /// Callback khi store trả về kết quả xác nhận giao dịch (sau ConfirmPurchase).
    /// Phân biệt ConfirmedOrder (thành công) và FailedOrder (thất bại).
    /// </summary>
    private static void OnPurchaseConfirmed(Order order)
    {
        GameDebug.Log("PurchaseManager: On purchase confirmed");
        switch (order)
        {
            case ConfirmedOrder confirmedOrder:
                OnPurchaseConfirmed(confirmedOrder);
                break;
            case FailedOrder failedOrder:
                OnPurchaseConfirmationFailed(failedOrder);
                break;
            default:
                GameDebug.LogWarning("PurchaseManager: Unknown OnPurchaseConfirmed result.");
                break;
        }
    }

    /// <summary>
    /// Log lỗi khi store từ chối xác nhận giao dịch.
    /// </summary>
    private static void OnPurchaseConfirmationFailed(FailedOrder order)
    {
        var product = GetFirstProductInOrder(order);
        if (product == null)
        {
            GameDebug.LogWarning("PurchaseManager: Could not find product in failed confirmation.");
            return;
        }

        GameDebug.LogWarning($"PurchaseManager: Confirmation failed - Product: '{product.definition.id}'," +
                             $"PurchaseFailureReason: {order.FailureReason.ToString()},"
                           + $"Confirmation Failure Details: {order.Details}");
    }

    /// <summary>
    /// Callback khi giao dịch thất bại (user hủy, lỗi thanh toán...).
    /// Gọi callback PurchaseState.Failed và xóa khỏi danh sách chờ.
    /// </summary>
    private void OnPurchaseFailed(FailedOrder order)
    {
        GameDebug.LogError($"PurchaseManager: On purchase failed description {order.FailureReason} and message {order.Details}");
        var productId = GetFirstProductInOrder(order).definition.id;
        if (!_purchaseStates.TryGetValue(productId, out var purchaseState)) return;
        purchaseState.Invoke(PurchaseState.Failed);
        _purchaseStates.Remove(productId);
    }

    /// <summary>
    /// Lấy sản phẩm đầu tiên trong một đơn hàng.
    /// </summary>
    private static Product GetFirstProductInOrder(Order order)
    {
        return order.CartOrdered.Items().First()?.Product;
    }

    /// <summary>
    /// Callback kiểm tra quyền sở hữu sản phẩm (chỉ NonConsumable & Subscription).
    /// isEntitled = true nếu user đã mua và sở hữu sản phẩm này.
    /// Sau đó gọi CheckNoAdsOnProduct() để xử lý logic nghiệp vụ tương ứng.
    /// </summary>
    private void OnCheckEntitlement(Entitlement entitlement)
    {
        var product = entitlement.Product;
        if (product == null)
        {
            GameDebug.LogError("PurchaseManager: On check entitlement but product is null");
            return;
        }

        var status = entitlement.Status;
        var isEntitled = status is EntitlementStatus.FullyEntitled or EntitlementStatus.EntitledButNotFinished;
        
        // _preferenceService.SetIsPurchased(product.definition.id, isEntitled);
        CheckNoAdsOnProduct(product.definition.id, isEntitled);
        
        if (isEntitled)
        {
            GameDebug.Log($"PurchaseManager: Unity purchase entitlement user owns {product.definition.id}");
        }
        else
        {
            GameDebug.LogWarning($"PurchaseManager: Unity purchase entitlement not user owns {product.definition.id}");
        }
    }
    
    /// <summary>
    /// Nếu bundle là no-ads và user đã entitled (đã mua), kích hoạt trạng thái no-ads vĩnh viễn.
    /// Hiện tại mới check no-ads, sau này có thể mở rộng cho các item IAP khác.
    /// </summary>
    private void CheckNoAdsOnProduct(string productId, bool isEntitled)
    {
        if (!isEntitled) return;
        if (dataBundle.IsNoAdsBundle(productId))
        {
            DataAccountPlayer.PlayerResourceData.ChangeNoAdsStatus(true);
        }
        
    }
    
    /// <summary>
    /// Callback khi fetch danh sách sản phẩm từ store thất bại.
    /// </summary>
    private static void OnProductsFetchFailed(ProductFetchFailed fetchFailed)
    {
        GameDebug.LogError($"PurchaseManager: On Products fetch failed {fetchFailed.FailureReason}");
    }

    /// <summary>
    /// Callback khi fetch lịch sử giao dịch từ store thất bại.
    /// </summary>
    private static void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failureDescription)
    {
        GameDebug.LogWarning(
            $"PurchaseManager: On Purchases Fetch failed description {failureDescription.failureReason} and message {failureDescription.message}");
    }

    /// <summary>
    /// Callback khi mất kết nối tới store.
    /// </summary>
    private static void OnStoreDisconnected(StoreConnectionFailureDescription failureDescription)
    {
        GameDebug.LogWarning($"PurchaseManager: On store disconnected {failureDescription.Message}");
    }

    /// <summary>
    /// Callback khi giao dịch bị hoãn (deferred) — thường xảy ra với tài khoản trẻ em
    /// cần phụ huynh phê duyệt.
    /// </summary>
    private static void OnPurchaseDeferred(DeferredOrder order)
    {
        GameDebug.LogWarning($"PurchaseManager: On purchase deferred description on product id: {order.Info.PurchasedProductInfo[0].productId}");
    }

    /// <summary>
    /// Bắt đầu quy trình mua một sản phẩm.
    /// 1. Kiểm tra StoreController đã sẵn sàng chưa
    /// 2. Lưu callback vào Dictionary _purchaseStates (productId → callback)
    /// 3. Gọi StoreController.PurchaseProduct() để hiển thị popup thanh toán
    /// Kết quả sẽ được trả về qua callback onPurchaseState (Success / Failed / NotAvailable).
    /// </summary>
    /// <param name="productId">ID sản phẩm trên store (idBundle)</param>
    /// <param name="onPurchaseState">Callback nhận kết quả giao dịch</param>
    public void StartPurchase(string productId, Action<PurchaseState> onPurchaseState)
    {
        GameDebug.Log($"PurchaseManager: Start initializing purchase {productId}");

        if (_controller == null)
        {
            onPurchaseState.Invoke(PurchaseState.NotAvailable);
            return;
        }

        if (_purchaseStates.ContainsKey(productId))
        {
            _purchaseStates.Remove(productId);
        }

        _purchaseStates.Add(productId, onPurchaseState);
        _controller.PurchaseProduct(productId);
    }
}
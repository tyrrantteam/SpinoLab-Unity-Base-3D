using System;
using System.Collections.Generic;
using Base.Core.Debug;
using JinGroup.Base.LoadData;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Common.IAP
{
    /// <summary>
    /// Trạng thái trả về sau khi thực hiện giao dịch IAP
    /// </summary>
    public enum PurchaseState
    {
        Success,
        Failed,
        NotAvailable
    }

    public class IAPController : SingletonMonoDontDestroy<IAPController>
    {
        private const string Environment = "production";

        private DataBundleController dataBundleController;

        public IAPController(string className) : base(className)
        {
        }

        private void Start()
        {
            Init();
        }

        /// <summary>
        /// Chuyển đổi chuỗi loại sản phẩm sang ProductType enum của Unity IAP
        /// </summary>
        /// <param name="productType">"Consumable" | "NonConsumable" | mặc định khác → Subscription</param>
        public ProductType GetProductType(string productType)
        {
            if (productType == "Consumable")
                return ProductType.Consumable;
            if (productType == "NonConsumable")
                return ProductType.NonConsumable;
            return ProductType.Subscription;
        }

        /// <summary>
        /// Khởi tạo Unity Gaming Services và Unity IAP:
        /// 1. InitializeAsync Unity Services (environment: production)
        /// 2. Lấy toàn bộ bundle từ DataBundleController
        /// 3. Build danh sách ProductDefinition (idBundle + productType)
        /// 4. Gọi PurchaseManager.Init() để kết nối store và fetch sản phẩm
        /// </summary>
        private async void Init()
        {
            try
            {
                var options = new InitializationOptions()
                    .SetEnvironmentName(Environment);
                GameDebug.Log("Start initialization Unity Services");
                await UnityServices.InitializeAsync(options);
                GameDebug.Log("Start initialization Unity Purchasing Services");

                var allIAP = dataBundleController.GetAllBundle();
                var listProduct = new List<ProductDefinition>();
                foreach (var bundle in allIAP)
                {
                    var bundleData = bundle[0];
                    listProduct.Add(new ProductDefinition(bundleData.idBundle, GetProductType(bundleData.productType)));
                }
                PurchaseManager.Instance.Init(listProduct);
            }
            catch (Exception e)
            {
                GameDebug.Log($"IAP Controller initialization error: {e.Message}");
            }
        }

        /// <summary>
        /// Bắt đầu quy trình mua một sản phẩm IAP.
        /// Ủy quyền trực tiếp cho PurchaseManager xử lý.
        /// </summary>
        /// <param name="productId">ID sản phẩm trên store (idBundle)</param>
        /// <param name="onPurchaseState">Callback nhận kết quả: Success / Failed / NotAvailable</param>
        public void StartPurchase(string productId, Action<PurchaseState> onPurchaseState)
        {
            var gameConfig = LoadResourceController.Instance.GameConfig();
            if (gameConfig == null)
            {
                GameDebug.LogError("Game Config not found");
            }

            if (gameConfig.isProduction)
            {
                PurchaseManager.Instance.StartPurchase(productId, onPurchaseState);
            }
            else
            {
                onPurchaseState?.Invoke(PurchaseState.Success);
            }
            
        }
    }
}
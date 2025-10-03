using UnityEngine;
using UniFP;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace UniFP.Examples.RealWorld
{
    /// <summary>
    /// Real-world example: game item purchase system.
    /// Covers currency validation, inventory management, and transaction processing.
    /// </summary>
    public class ItemPurchaseExample : MonoBehaviour
    {
        [SerializeField] private int _playerGold = 1000;
        [SerializeField] private int _playerGems = 50;

        private Dictionary<int, Item> _itemCatalog;
        private List<int> _inventory;

        void Start()
        {
            Debug.Log("=== Real World Example: Item Purchase System ===\n");

            InitializeData();

            // Execute multiple purchase scenarios
            PurchaseScenarios();
        }

        void InitializeData()
        {
            _inventory = new List<int>();

            _itemCatalog = new Dictionary<int, Item>
            {
                { 1, new Item { Id = 1, Name = "Health Potion", Price = 50, Currency = CurrencyType.Gold, MaxStack = 99 } },
                { 2, new Item { Id = 2, Name = "Magic Sword", Price = 500, Currency = CurrencyType.Gold, MaxStack = 1 } },
                { 3, new Item { Id = 3, Name = "Rare Gem", Price = 10, Currency = CurrencyType.Gems, MaxStack = 999 } },
                { 4, new Item { Id = 4, Name = "Legendary Armor", Price = 1000, Currency = CurrencyType.Gold, MaxStack = 1 } }
            };
        }

        #region Purchase Scenarios

        void PurchaseScenarios()
        {
            Debug.Log($"Initial: Gold={_playerGold}, Gems={_playerGems}\n");

            // Scenario 1: normal purchase
            Debug.Log("--- Scenario 1: Normal Purchase ---");
            PurchaseItem(1, 5).Match(
                onSuccess: receipt => Debug.Log($"✓ Purchased: {receipt}"),
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ Failed: {error}")
            );

            // Scenario 2: insufficient currency
            Debug.Log("\n--- Scenario 2: Insufficient Currency ---");
            PurchaseItem(4, 1).Match(
                onSuccess: receipt => Debug.Log($"✓ Purchased: {receipt}"),
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ Failed: {error}")
            );

            // Scenario 3: duplicate unique item attempt
            Debug.Log("\n--- Scenario 3: Duplicate Unique Item ---");
            _inventory.Add(2); // already owned
            PurchaseItem(2, 1).Match(
                onSuccess: receipt => Debug.Log($"✓ Purchased: {receipt}"),
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ Failed: {error}")
            );

            // Scenario 4: bulk purchase
            Debug.Log("\n--- Scenario 4: Bulk Purchase ---");
            BulkPurchase(new[] { 1, 1, 1, 3, 3 });

            Debug.Log($"\nFinal: Gold={_playerGold}, Gems={_playerGems}");
            Debug.Log($"Inventory: {string.Join(", ", _inventory.Select(id => _itemCatalog[id].Name))}");
        }

        #endregion

        #region Purchase Flow

        /// <summary>
        /// Complete item purchase flow.
        /// </summary>
        Result<PurchaseReceipt> PurchaseItem(int itemId, int quantity)
        {
            return ValidateItemId(itemId)
                .Then(item => ValidateQuantity(item, quantity))
                .Then(purchase => CheckCurrency(purchase))
                .Then(purchase => CheckInventorySpace(purchase))
                .Then(purchase => ProcessTransaction(purchase))
                .Map(purchase => GenerateReceipt(purchase));
        }

        #endregion

        #region 1. Validate Item

        Result<Item> ValidateItemId(int itemId)
        {
            Debug.Log($"1️⃣ Validating item ID {itemId}...");

            if (_itemCatalog.TryGetValue(itemId, out var item))
            {
                Debug.Log($"  ✓ Found: {item.Name}");
                return Result<Item>.Success(item);
            }

            Debug.LogWarning($"  ✗ Item not found");
            return Result<Item>.Failure(ErrorCode.NotFound);
        }

        #endregion

        #region 2. Validate Quantity

        Result<PurchaseRequest> ValidateQuantity(Item item, int quantity)
        {
            Debug.Log($"2️⃣ Validating quantity {quantity}...");

            return Result.FromValue(quantity)
                .Filter(q => q > 0, ErrorCode.InvalidInput)
                .Filter(q => q <= item.MaxStack, ErrorCode.ValidationFailed)
                .Map(validQty => new PurchaseRequest
                {
                    Item = item,
                    Quantity = validQty,
                    TotalCost = item.Price * validQty
                })
                .Do(req => Debug.Log($"  ✓ Total cost: {req.TotalCost} {item.Currency}"));
        }

        #endregion

        #region 3. Check Currency

        Result<PurchaseRequest> CheckCurrency(PurchaseRequest request)
        {
            Debug.Log($"3️⃣ Checking currency...");

            var currentBalance = request.Item.Currency switch
            {
                CurrencyType.Gold => _playerGold,
                CurrencyType.Gems => _playerGems,
                _ => 0
            };

            if (currentBalance >= request.TotalCost)
            {
                Debug.Log($"  ✓ Sufficient balance: {currentBalance} >= {request.TotalCost}");
                return Result<PurchaseRequest>.Success(request);
            }

            Debug.LogWarning($"  ✗ Insufficient: {currentBalance} < {request.TotalCost}");
            return Result<PurchaseRequest>.Failure(ErrorCode.InsufficientResources);
        }

        #endregion

        #region 4. Check Inventory

        Result<PurchaseRequest> CheckInventorySpace(PurchaseRequest request)
        {
            Debug.Log($"4️⃣ Checking inventory space...");

            // Unique items cannot be owned more than once
            if (request.Item.MaxStack == 1 && _inventory.Contains(request.Item.Id))
            {
                Debug.LogWarning($"  ✗ Already owned (unique item)");
                return Result<PurchaseRequest>.Failure(ErrorCode.AlreadyExists);
            }

            // Verify inventory capacity (simplified example)
            if (_inventory.Count + request.Quantity > 100)
            {
                Debug.LogWarning($"  ✗ Inventory full");
                return Result<PurchaseRequest>.Failure(ErrorCode.Capacity);
            }

            Debug.Log($"  ✓ Space available");
            return Result<PurchaseRequest>.Success(request);
        }

        #endregion

        #region 5. Process Transaction

        Result<PurchaseRequest> ProcessTransaction(PurchaseRequest request)
        {
            Debug.Log($"5️⃣ Processing transaction...");

            return Result.TryFromValue(() =>
            {
                // Deduct currency
                switch (request.Item.Currency)
                {
                    case CurrencyType.Gold:
                        _playerGold -= request.TotalCost;
                        break;
                    case CurrencyType.Gems:
                        _playerGems -= request.TotalCost;
                        break;
                }

                // Add items to inventory
                for (int i = 0; i < request.Quantity; i++)
                {
                    _inventory.Add(request.Item.Id);
                }

                Debug.Log($"  ✓ Transaction complete");
                return request;
            });
        }

        #endregion

        #region Receipt Generation

        PurchaseReceipt GenerateReceipt(PurchaseRequest request)
        {
            return new PurchaseReceipt
            {
                ItemName = request.Item.Name,
                Quantity = request.Quantity,
                Cost = request.TotalCost,
                Currency = request.Item.Currency,
                Timestamp = System.DateTime.Now
            };
        }

        #endregion

        #region Bulk Purchase

        void BulkPurchase(int[] itemIds)
        {
            var results = itemIds.SelectResults(id => PurchaseItem(id, 1));

            results.Match(
                onSuccess: receipts =>
                {
                    Debug.Log($"✓ Bulk purchase complete: {receipts.Count} items");
                    foreach (var receipt in receipts)
                    {
                        Debug.Log($"  - {receipt}");
                    }
                },
                onFailure: (ErrorCode error) =>
                {
                    Debug.LogError($"✗ Bulk purchase failed at: {error}");
                }
            );
        }

        #endregion

        #region Data Models

        enum CurrencyType { Gold, Gems }

        class Item
        {
            public int Id;
            public string Name;
            public int Price;
            public CurrencyType Currency;
            public int MaxStack;
        }

        class PurchaseRequest
        {
            public Item Item;
            public int Quantity;
            public int TotalCost;
        }

        class PurchaseReceipt
        {
            public string ItemName;
            public int Quantity;
            public int Cost;
            public CurrencyType Currency;
            public System.DateTime Timestamp;

            public override string ToString()
            {
                return $"{ItemName} x{Quantity} for {Cost} {Currency} at {Timestamp:HH:mm:ss}";
            }
        }

        #endregion
    }
}

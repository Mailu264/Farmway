using System;
using System.Collections.Generic;
using Farmway.Gameplay.Farm;
using Farmway.Gameplay.UI;
using Farmway.Infrastructure;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace Farmway.Gameplay.Player
{
    public class ShopPresenter : IInitializable, IDisposable
    {
        private readonly PlayerShopService _playerShopService;
        private readonly ShopService _shopService;
        private readonly MoneyModel _moneyModel;
        private readonly IInventorySlotsModel _inventorySlotsModel;
        private readonly IHotbarSlotsModel _hotbarSlotsModel;
        private readonly FarmConfig _farmConfig;
        private readonly CompositeDisposable _disposables = new();

        private Canvas _canvas;
        private Image _panel;
        private Text _moneyText;
        private RectTransform _buyColumn;
        private RectTransform _sellColumn;

        private bool _isOpen;

        public ShopPresenter(
            PlayerShopService playerShopService,
            ShopService shopService,
            MoneyModel moneyModel,
            IInventorySlotsModel inventorySlotsModel,
            IHotbarSlotsModel hotbarSlotsModel,
            IConfigProvider configProvider)
        {
            _playerShopService = playerShopService;
            _shopService = shopService;
            _moneyModel = moneyModel;
            _inventorySlotsModel = inventorySlotsModel;
            _hotbarSlotsModel = hotbarSlotsModel;
            _farmConfig = configProvider.GetConfig<FarmConfig>();
        }

        public void Initialize()
        {
            BuildShop();

            _playerShopService.OnToggleRequested
                .Subscribe(_ => Toggle())
                .AddTo(_disposables);

            // Отошёл от торговца — магазин закрывается
            _playerShopService.IsNearTrader
                .Where(isNear => !isNear && _isOpen)
                .Subscribe(_ => Toggle())
                .AddTo(_disposables);

            _moneyModel.Amount
                .Subscribe(amount =>
                {
                    if (_moneyText != null)
                        _moneyText.text = $"Деньги: {amount}";
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();

            if (_canvas != null)
                UnityEngine.Object.Destroy(_canvas.gameObject);
        }

        private void Toggle()
        {
            _isOpen = !_isOpen;
            _panel.gameObject.SetActive(_isOpen);

            if (_isOpen)
                Refresh();
        }

        private void BuildShop()
        {
            _canvas = UiBuilder.CreateCanvas("RuntimeShop", 200);

            _panel = UiBuilder.CreatePanel(_canvas.transform, "ShopPanel", new Vector2(760f, 560f), new Color(0.12f, 0.12f, 0.16f, 0.97f));

            UiBuilder.CreateText(
                _panel.transform, "Title",
                new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(700f, 40f),
                30, TextAnchor.UpperCenter, Color.white)
                .text = "Магазин   [E] — закрыть";

            _moneyText = UiBuilder.CreateText(
                _panel.transform, "Money",
                new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(700f, 30f),
                22, TextAnchor.UpperCenter, new Color(1f, 0.9f, 0.4f));

            _buyColumn = CreateColumn("BuyColumn", -190f, "Купить");
            _sellColumn = CreateColumn("SellColumn", 190f, "Продать");

            _panel.gameObject.SetActive(false);
        }

        private RectTransform CreateColumn(string name, float offsetX, string header)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_panel.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(offsetX, -100f);
            rect.sizeDelta = new Vector2(340f, 420f);

            UiBuilder.CreateText(
                go.transform, "Header",
                new Vector2(0.5f, 1f), Vector2.zero, new Vector2(340f, 30f),
                24, TextAnchor.UpperCenter, new Color(0.7f, 0.9f, 1f))
                .text = header;

            return rect;
        }

        private void Refresh()
        {
            ClearRows(_buyColumn);
            ClearRows(_sellColumn);

            float y = -44f;
            foreach (var item in _farmConfig.ShopItems)
            {
                if (item == null || item.BuyPrice <= 0)
                    continue;

                var captured = item;
                UiBuilder.CreateButton(
                    _buyColumn, $"Buy_{item.name}",
                    new Vector2(0f, y), new Vector2(320f, 40f),
                    $"{item.Name} — {item.BuyPrice}",
                    () => { _shopService.Buy(captured); Refresh(); });

                y -= 48f;
            }

            y = -44f;
            foreach (var (item, count) in CollectSellables())
            {
                var captured = item;
                UiBuilder.CreateButton(
                    _sellColumn, $"Sell_{item.name}",
                    new Vector2(0f, y), new Vector2(320f, 40f),
                    $"{item.Name} x{count} — {item.SellPrice}",
                    () => { _shopService.Sell(captured); Refresh(); });

                y -= 48f;
            }
        }

        private void ClearRows(RectTransform column)
        {
            for (int i = column.childCount - 1; i >= 0; i--)
            {
                var child = column.GetChild(i);

                if (child.name == "Header")
                    continue;

                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        private IEnumerable<(ItemDefinition item, int count)> CollectSellables()
        {
            var counts = new Dictionary<ItemDefinition, int>();

            CollectFrom(_inventorySlotsModel, counts);
            CollectFrom(_hotbarSlotsModel, counts);

            foreach (var pair in counts)
                if (pair.Key.SellPrice > 0)
                    yield return (pair.Key, pair.Value);
        }

        private static void CollectFrom(IItemSlotsModel model, Dictionary<ItemDefinition, int> counts)
        {
            foreach (var slot in model.Slots)
            {
                if (slot.IsEmpty)
                    continue;

                counts.TryGetValue(slot.Item, out int current);
                counts[slot.Item] = current + slot.Count;
            }
        }
    }
}

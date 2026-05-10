using System;
using Farmway.Infrastructure;
using UniRx;
using UnityEngine.EventSystems;
using VContainer.Unity;

namespace Farmway.Gameplay.Player
{
    public class InventoryPresenter : IInitializable, IDisposable
    {
        private readonly IInventorySlotsModel _inventorySlotsModel;
        private readonly ItemsConfig _itemsConfig;
        private readonly InventoryView _inventoryView;
        private readonly CompositeDisposable _disposables = new();
        private int _draggedSlotIndex = -1;
        private bool _dropSucceeded;

        public InventoryPresenter(
            IInventorySlotsModel inventorySlotsModel,
            IConfigProvider configProvider,
            GameplaySceneView gameplaySceneView)
        {
            _inventorySlotsModel = inventorySlotsModel;
            _itemsConfig = configProvider.GetConfig<ItemsConfig>();
            _inventoryView = gameplaySceneView.InventoryView;
        }

        public void Initialize()
        {
            _inventoryView.Initialize();
            SubscribeSlots();

            DrawAllSlots();

            _inventorySlotsModel.Slots.ObserveReplace()
                .Subscribe(slot => DrawSlot(slot.Index, slot.NewValue))
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            UnsubscribeSlots();
            _disposables.Dispose();
        }

        private void SubscribeSlots()
        {
            for (int i = 0; i < _inventoryView.SlotsCount; i++)
            {
                InventorySlotView slot = _inventoryView.GetSlot(i);

                if (slot == null)
                    continue;

                slot.BeginDragged += OnSlotBeginDragged;
                slot.Dragged += OnSlotDragged;
                slot.EndDragged += OnSlotEndDragged;
                slot.Dropped += OnSlotDropped;
            }
        }

        private void UnsubscribeSlots()
        {
            for (int i = 0; i < _inventoryView.SlotsCount; i++)
            {
                InventorySlotView slot = _inventoryView.GetSlot(i);

                if (slot == null)
                    continue;

                slot.BeginDragged -= OnSlotBeginDragged;
                slot.Dragged -= OnSlotDragged;
                slot.EndDragged -= OnSlotEndDragged;
                slot.Dropped -= OnSlotDropped;
            }
        }

        private void OnSlotBeginDragged(int index, PointerEventData eventData)
        {
            InventorySlotData slotData = _inventorySlotsModel.Slots[index];

            if (slotData.IsEmpty || !_itemsConfig.TryGetItem(slotData.ItemId, out ItemData itemData))
                return;

            _draggedSlotIndex = index;
            _dropSucceeded = false;
            _inventoryView.ShowDrag(itemData.Icon, slotData.Count, index, eventData);
        }

        private void OnSlotDragged(PointerEventData eventData) =>
            _inventoryView.MoveDrag(eventData);

        private void OnSlotEndDragged()
        {
            _inventoryView.HideDrag();

            if (!_dropSucceeded && _draggedSlotIndex >= 0)
                _inventoryView.SetSlotVisualsVisible(_draggedSlotIndex, true);

            _draggedSlotIndex = -1;
            _dropSucceeded = false;
        }

        private void OnSlotDropped(int toIndex)
        {
            if (_draggedSlotIndex < 0 || _draggedSlotIndex == toIndex)
                return;

            _dropSucceeded = _inventorySlotsModel.TryMove(_draggedSlotIndex, toIndex);
        }

        private void DrawAllSlots()
        {
            for (int i = 0; i < _inventorySlotsModel.Slots.Count; i++)
                DrawSlot(i, _inventorySlotsModel.Slots[i]);
        }

        private void DrawSlot(int index, InventorySlotData slotData)
        {
            if (slotData.IsEmpty || !_itemsConfig.TryGetItem(slotData.ItemId, out ItemData itemData))
            {
                _inventoryView.ClearSlot(index);
                return;
            }

            _inventoryView.SetSlot(index, slotData, itemData);
        }
    }
}

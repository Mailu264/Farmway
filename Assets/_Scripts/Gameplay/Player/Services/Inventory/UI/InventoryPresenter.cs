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
        private readonly IHotbarSlotsModel _hotbarSlotsModel;
        private readonly IItemSlotTransferService _itemSlotTransferService;
        private readonly ItemsConfig _itemsConfig;
        private readonly InventoryView _inventoryView;
        private readonly HotbarView _hotbarView;
        private readonly CompositeDisposable _disposables = new();
        private readonly CompositeDisposable _slotSubscriptions = new();

        private IItemSlotsModel _draggedSlotsModel;
        private IItemSlotsView _draggedSlotsView;
        private int _draggedSlotIndex = -1;
        private bool _dropSucceeded;

        public InventoryPresenter(
            IInventorySlotsModel inventorySlotsModel,
            IHotbarSlotsModel hotbarSlotsModel,
            IItemSlotTransferService itemSlotTransferService,
            IConfigProvider configProvider,
            GameplaySceneView gameplaySceneView)
        {
            _inventorySlotsModel = inventorySlotsModel;
            _hotbarSlotsModel = hotbarSlotsModel;
            _itemSlotTransferService = itemSlotTransferService;
            _itemsConfig = configProvider.GetConfig<ItemsConfig>();
            _inventoryView = gameplaySceneView.InventoryView;
            _hotbarView = gameplaySceneView.HotbarView;
        }

        public void Initialize()
        {
            InitializeSlots(_inventorySlotsModel, _inventoryView);
            InitializeSlots(_hotbarSlotsModel, _hotbarView);
        }

        public void Dispose()
        {
            _slotSubscriptions.Dispose();
            _disposables.Dispose();
        }

        private void InitializeSlots(IItemSlotsModel slotsModel, IItemSlotsView slotsView)
        {
            slotsView.Initialize(slotsModel.Slots.Count);

            SubscribeSlots(slotsModel, slotsView);
            DrawAllSlots(slotsModel, slotsView);

            slotsModel.Slots.ObserveReplace()
                .Subscribe(slot => DrawSlot(slotsView, slot.Index, slot.NewValue))
                .AddTo(_disposables);
        }

        private void SubscribeSlots(IItemSlotsModel slotsModel, IItemSlotsView slotsView)
        {
            for (int i = 0; i < slotsView.SlotsCount; i++)
            {
                InventorySlotView slot = slotsView.GetSlot(i);

                if (slot == null)
                    continue;

                SubscribeSlot(slot, slotsModel, slotsView).AddTo(_slotSubscriptions);
            }
        }

        private IDisposable SubscribeSlot(InventorySlotView slot, IItemSlotsModel slotsModel, IItemSlotsView slotsView)
        {
            void BeginDragged(int index, PointerEventData eventData) =>
                OnSlotBeginDragged(slotsModel, slotsView, index, eventData);

            void Dragged(PointerEventData eventData) =>
                OnSlotDragged(eventData);

            void EndDragged() =>
                OnSlotEndDragged();

            void Dropped(int index) =>
                OnSlotDropped(slotsModel, index);

            slot.BeginDragged += BeginDragged;
            slot.Dragged += Dragged;
            slot.EndDragged += EndDragged;
            slot.Dropped += Dropped;

            return Disposable.Create(() =>
            {
                slot.BeginDragged -= BeginDragged;
                slot.Dragged -= Dragged;
                slot.EndDragged -= EndDragged;
                slot.Dropped -= Dropped;
            });
        }

        private void OnSlotBeginDragged(
            IItemSlotsModel slotsModel,
            IItemSlotsView slotsView,
            int index,
            PointerEventData eventData)
        {
            InventorySlotData slotData = slotsModel.GetSlot(index);

            if (slotData.IsEmpty || !_itemsConfig.TryGetItem(slotData.ItemId, out ItemData itemData))
                return;

            _draggedSlotsModel = slotsModel;
            _draggedSlotsView = slotsView;
            _draggedSlotIndex = index;
            _dropSucceeded = false;
            slotsView.ShowDrag(itemData.Icon, slotData.Count, slotsView.GetSlotSize(index), eventData);
            slotsView.SetSlotVisualsVisible(index, false);
        }

        private void OnSlotDragged(PointerEventData eventData)
        {
            if (_draggedSlotIndex < 0)
                return;

            _draggedSlotsView.MoveDrag(eventData);
        }

        private void OnSlotEndDragged()
        {
            if (_draggedSlotIndex < 0)
                return;

            _draggedSlotsView.HideDrag();

            if (!_dropSucceeded && _draggedSlotIndex >= 0)
                _draggedSlotsView.SetSlotVisualsVisible(_draggedSlotIndex, true);

            _draggedSlotsModel = null;
            _draggedSlotsView = null;
            _draggedSlotIndex = -1;
            _dropSucceeded = false;
        }

        private void OnSlotDropped(IItemSlotsModel targetSlotsModel, int toIndex)
        {
            if (_draggedSlotIndex < 0)
                return;

            _dropSucceeded = _itemSlotTransferService.TryMove(
                _draggedSlotsModel,
                _draggedSlotIndex,
                targetSlotsModel,
                toIndex);
        }

        private void DrawAllSlots(IItemSlotsModel slotsModel, IItemSlotsView slotsView)
        {
            for (int i = 0; i < slotsModel.Slots.Count; i++)
                DrawSlot(slotsView, i, slotsModel.Slots[i]);
        }

        private void DrawSlot(IItemSlotsView slotsView, int index, InventorySlotData slotData)
        {
            if (slotData.IsEmpty || !_itemsConfig.TryGetItem(slotData.ItemId, out ItemData itemData))
            {
                slotsView.ClearSlot(index);
                return;
            }

            slotsView.SetSlot(index, slotData, itemData);
        }
    }
}

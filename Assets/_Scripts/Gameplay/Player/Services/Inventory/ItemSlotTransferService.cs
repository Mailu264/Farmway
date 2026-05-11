using Farmway.Infrastructure;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public interface IItemSlotTransferService
    {
        bool TryMove(IItemSlotsModel source, int sourceIndex, IItemSlotsModel target, int targetIndex);
    }

    public class ItemSlotTransferService : IItemSlotTransferService
    {
        private readonly ItemsConfig _itemsConfig;

        public ItemSlotTransferService(IConfigProvider configProvider)
        {
            _itemsConfig = configProvider.GetConfig<ItemsConfig>();
        }

        public bool TryMove(IItemSlotsModel source, int sourceIndex, IItemSlotsModel target, int targetIndex)
        {
            if (!source.IsValidIndex(sourceIndex) || !target.IsValidIndex(targetIndex))
                return false;

            if (ReferenceEquals(source, target) && sourceIndex == targetIndex)
                return false;

            InventorySlotData from = source.GetSlot(sourceIndex);
            InventorySlotData to = target.GetSlot(targetIndex);

            if (from.IsEmpty)
                return false;

            if (to.IsEmpty)
            {
                target.SetSlot(targetIndex, from);
                source.SetSlot(sourceIndex, InventorySlotData.Empty);
                return true;
            }

            if (CanMerge(from, to))
                return Merge(source, sourceIndex, target, targetIndex, from, to);

            target.SetSlot(targetIndex, from);
            source.SetSlot(sourceIndex, to);
            return true;
        }

        private bool CanMerge(InventorySlotData from, InventorySlotData to)
        {
            if (from.ItemId != to.ItemId)
                return false;

            if (!_itemsConfig.TryGetItem(from.ItemId, out ItemData itemData))
                return false;

            return itemData.IsStackable && to.Count < Mathf.Max(1, itemData.MaxStackSize);
        }

        private bool Merge(
            IItemSlotsModel source,
            int sourceIndex,
            IItemSlotsModel target,
            int targetIndex,
            InventorySlotData from,
            InventorySlotData to)
        {
            if (!_itemsConfig.TryGetItem(from.ItemId, out ItemData itemData))
                return false;

            int freeSpace = Mathf.Max(1, itemData.MaxStackSize) - to.Count;
            int movedCount = Mathf.Min(freeSpace, from.Count);
            int remainingCount = from.Count - movedCount;

            target.SetSlot(targetIndex, new InventorySlotData(to.ItemId, to.Count + movedCount));
            source.SetSlot(sourceIndex, remainingCount > 0
                ? new InventorySlotData(from.ItemId, remainingCount)
                : InventorySlotData.Empty);

            return true;
        }
    }
}

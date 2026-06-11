using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public interface IItemSlotTransferService
    {
        bool TryMove(IItemSlotsModel source, int sourceIndex, IItemSlotsModel target, int targetIndex);
    }

    public class ItemSlotTransferService : IItemSlotTransferService
    {
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

        private static bool CanMerge(InventorySlotData from, InventorySlotData to)
        {
            if (from.Item != to.Item)
                return false;

            return from.Item.IsStackable && to.Count < Mathf.Max(1, from.Item.MaxStackSize);
        }

        private static bool Merge(
            IItemSlotsModel source,
            int sourceIndex,
            IItemSlotsModel target,
            int targetIndex,
            InventorySlotData from,
            InventorySlotData to)
        {
            int freeSpace = Mathf.Max(1, from.Item.MaxStackSize) - to.Count;
            int movedCount = Mathf.Min(freeSpace, from.Count);
            int remainingCount = from.Count - movedCount;

            target.SetSlot(targetIndex, new InventorySlotData(to.Item, to.Count + movedCount));
            source.SetSlot(sourceIndex, remainingCount > 0
                ? new InventorySlotData(from.Item, remainingCount)
                : InventorySlotData.Empty);

            return true;
        }
    }
}

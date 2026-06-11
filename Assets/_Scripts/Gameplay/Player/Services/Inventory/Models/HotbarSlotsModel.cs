using UniRx;

namespace Farmway.Gameplay.Player
{
    public interface IHotbarSlotsModel : IItemSlotsModel
    {
        IReadOnlyReactiveProperty<int> CurrentSlotIndex { get; }
        IReadOnlyReactiveProperty<InventorySlotData> CurrentItem { get; }

        void SelectSlot(int index);
        void SelectNext();
        void SelectPrevious();
    }

    public class HotbarSlotsModel : IHotbarSlotsModel
    {
        private readonly ItemSlotsCollection _slots = new();
        private readonly ReactiveProperty<int> _currentSlotIndex = new(0);
        private readonly ReactiveProperty<InventorySlotData> _currentItem = new(InventorySlotData.Empty);

        public IReadOnlyReactiveCollection<InventorySlotData> Slots => _slots.Slots;
        public IReadOnlyReactiveProperty<int> CurrentSlotIndex => _currentSlotIndex;
        public IReadOnlyReactiveProperty<InventorySlotData> CurrentItem => _currentItem;

        public void Initialize(int slotsCount)
        {
            _slots.Initialize(slotsCount);
            _currentSlotIndex.Value = 0;
            UpdateCurrentItem();
        }

        public bool IsValidIndex(int index) =>
            _slots.IsValidIndex(index);

        public InventorySlotData GetSlot(int index) =>
            _slots.GetSlot(index);

        public void SetSlot(int index, InventorySlotData slotData)
        {
            _slots.SetSlot(index, slotData);
            UpdateCurrentItem();
        }

        public int GetItemCount(ItemDefinition item) =>
            _slots.GetItemCount(item);

        public bool RemoveItem(ItemDefinition item, int count)
        {
            bool removed = _slots.RemoveItem(item, count);

            if (removed)
                UpdateCurrentItem();

            return removed;
        }

        public int StackExisting(ItemDefinition item, int count)
        {
            int remaining = _slots.StackExisting(item, count);

            if (remaining != count)
                UpdateCurrentItem();

            return remaining;
        }

        public void SelectSlot(int index)
        {
            if (!IsValidIndex(index))
                return;

            _currentSlotIndex.Value = index;
            UpdateCurrentItem();
        }

        public void SelectNext()
        {
            if (Slots.Count == 0)
                return;

            int nextIndex = (_currentSlotIndex.Value + 1) % Slots.Count;
            SelectSlot(nextIndex);
        }

        public void SelectPrevious()
        {
            if (Slots.Count == 0)
                return;

            int nextIndex = _currentSlotIndex.Value - 1;

            if (nextIndex < 0)
                nextIndex = Slots.Count - 1;

            SelectSlot(nextIndex);
        }

        private void UpdateCurrentItem()
        {
            int index = _currentSlotIndex.Value;
            _currentItem.Value = GetSlot(index);
        }
    }
}

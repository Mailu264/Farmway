using System;
using System.Collections.Generic;

namespace Farmway.Infrastructure
{
    [Serializable]
    public class SaveData
    {
        public int Day = 1;
        public int Money;
        public List<SlotSave> InventorySlots = new();
        public List<SlotSave> HotbarSlots = new();
        public List<CellSave> Cells = new();
    }

    [Serializable]
    public class SlotSave
    {
        public int Index;
        public string ItemName;
        public int Count;
    }

    [Serializable]
    public class CellSave
    {
        public int X;
        public int Y;
        public bool Watered;
        public int PlantType; // 0 = нет растения
        public int Stage;
        public bool Ready;
    }
}

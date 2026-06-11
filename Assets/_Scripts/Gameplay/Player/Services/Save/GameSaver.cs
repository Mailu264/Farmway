using Farmway.Gameplay.Farm;
using Farmway.Gameplay.Services;
using Farmway.Infrastructure;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    // Собирает состояние игры и пишет сейв. Вызывается при каждом сне.
    public class GameSaver
    {
        private readonly ISaveService _saveService;
        private readonly IGameTimeService _gameTimeService;
        private readonly MoneyModel _moneyModel;
        private readonly IInventorySlotsModel _inventorySlotsModel;
        private readonly IHotbarSlotsModel _hotbarSlotsModel;
        private readonly FarmGrid _farmGrid;

        public GameSaver(
            ISaveService saveService,
            IGameTimeService gameTimeService,
            MoneyModel moneyModel,
            IInventorySlotsModel inventorySlotsModel,
            IHotbarSlotsModel hotbarSlotsModel,
            FarmGrid farmGrid)
        {
            _saveService = saveService;
            _gameTimeService = gameTimeService;
            _moneyModel = moneyModel;
            _inventorySlotsModel = inventorySlotsModel;
            _hotbarSlotsModel = hotbarSlotsModel;
            _farmGrid = farmGrid;
        }

        public void Save()
        {
            var data = new SaveData
            {
                Day = _gameTimeService.GameTime.Day.Value,
                Money = _moneyModel.Amount.Value
            };

            CollectSlots(_inventorySlotsModel, data.InventorySlots);
            CollectSlots(_hotbarSlotsModel, data.HotbarSlots);
            CollectCells(data);

            _saveService.Save(data);
            Debug.Log($"[Save] Сохранено: день {data.Day}, {data.Cells.Count} клеток");
        }

        private static void CollectSlots(IItemSlotsModel model, System.Collections.Generic.List<SlotSave> target)
        {
            for (int i = 0; i < model.Slots.Count; i++)
            {
                var slot = model.GetSlot(i);

                if (slot.IsEmpty)
                    continue;

                target.Add(new SlotSave { Index = i, ItemName = slot.Item.name, Count = slot.Count });
            }
        }

        private void CollectCells(SaveData data)
        {
            foreach (var (pos, cell) in _farmGrid.GetAllCells())
            {
                if (cell.IsEmpty)
                    continue;

                data.Cells.Add(new CellSave
                {
                    X = pos.x,
                    Y = pos.y,
                    Watered = cell.IsWatered,
                    PlantType = cell.HasPlant ? (int)cell.Plant.Type : 0,
                    Stage = cell.HasPlant ? cell.Plant.Stage.Value : 0,
                    Ready = cell.HasPlant && cell.Plant.IsReadyToHarvest.Value
                });
            }
        }
    }
}

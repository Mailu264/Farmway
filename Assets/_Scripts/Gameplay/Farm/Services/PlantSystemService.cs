using Farmway.Gameplay.Services;
using Farmway.Infrastructure;
using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class PlantSystemService : GameServiceBase
    {
        private readonly FarmGrid _farmGrid;
        private readonly PlantFactory _plantFactory;
        private readonly PlantsConfig _plantsConfig;

        // Конфиг в конструкторе: не зависит от порядка вызова OnInitialize
        public PlantSystemService(FarmGrid farmGrid, PlantFactory plantFactory, IConfigProvider configProvider)
        {
            _farmGrid = farmGrid;
            _plantFactory = plantFactory;
            _plantsConfig = configProvider.GetConfig<PlantsConfig>();
        }

        public bool PlantSeed(Vector2Int pos, PlantType type)
        {
            if (!_farmGrid.TryGetCell(pos, out var cell) || !cell.IsTilled)
            {
                Debug.LogWarning($"[PlantSystemService] Cannot plant at {pos} — not tilled");
                return false;
            }

            if (cell.HasPlant)
            {
                Debug.LogWarning($"[PlantSystemService] Cannot plant at {pos} — already has plant");
                return false;
            }

            if (!_plantFactory.TryCreate(type, pos, out var plant))
                return false;

            cell.Plant = plant;
            return _farmGrid.TrySetCell(pos, cell);
        }

        public bool TryHarvest(Vector2Int pos, out PlantData plantData)
        {
            plantData = null;

            if (!_farmGrid.TryGetCell(pos, out var cell) || !cell.HasPlant)
                return false;

            if (!cell.Plant.IsReadyToHarvest.Value)
                return false;

            if (!_plantsConfig.TryGetPlant(cell.Plant.Type, out plantData))
                return false;

            cell.Plant.Dispose();
            cell.Plant = null;
            _farmGrid.TrySetCell(pos, cell);
            return true;
        }

        // Скип ночи: политые растения переходят в следующую фазу
        public void AdvanceDay()
        {
            foreach (var (_, cell) in _farmGrid.GetAllCells())
            {
                if (!cell.HasPlant || !cell.IsWatered)
                    continue;

                cell.Plant.GetService<PlantGrowthService>()?.AdvanceDay();
            }
        }
    }
}

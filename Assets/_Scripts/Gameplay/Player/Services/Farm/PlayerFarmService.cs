using Farmway.Gameplay.Farm;
using Farmway.Gameplay.Player.Services.ItemsService;
using Farmway.Gameplay.Services;
using Farmway.Infrastructure;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Farmway.Gameplay.Player
{
    public class PlayerFarmService : PlayerService
    {
        private readonly IInputService _inputService;
        private readonly IGameTimeService _gameTimeService;
        private readonly FarmService _farmService;
        private readonly PlantSystemService _plantSystemService;
        private readonly IHotbarSlotsModel _hotbarSlotsModel;
        private readonly IPlayerInventoryService _inventoryService;
        private readonly PlayerItemsService _playerItemsService;
        private readonly FatigueModel _fatigueModel;
        private readonly FarmConfig _farmConfig;
        private readonly PlantsConfig _plantsConfig;
        private readonly Grid _grid;

        private readonly CompositeDisposable _disposables = new();

        public PlayerFarmService(
            IInputService inputService,
            IGameTimeService gameTimeService,
            FarmService farmService,
            PlantSystemService plantSystemService,
            IHotbarSlotsModel hotbarSlotsModel,
            IPlayerInventoryService inventoryService,
            PlayerItemsService playerItemsService,
            FatigueModel fatigueModel,
            IConfigProvider configProvider,
            FarmGridView farmGridView)
        {
            _inputService = inputService;
            _gameTimeService = gameTimeService;
            _farmService = farmService;
            _plantSystemService = plantSystemService;
            _hotbarSlotsModel = hotbarSlotsModel;
            _inventoryService = inventoryService;
            _playerItemsService = playerItemsService;
            _fatigueModel = fatigueModel;
            _farmConfig = configProvider.GetConfig<FarmConfig>();
            _plantsConfig = configProvider.GetConfig<PlantsConfig>();
            _grid = farmGridView.Grid;
        }

        public override void OnEnable()
        {
            _inputService.OnInteract
                .Subscribe(OnInteract)
                .AddTo(_disposables);
        }

        public override void OnDisable() => _disposables.Clear();

        public override void OnDispose() => _disposables.Dispose();

        private void OnInteract(Vector2 screenPosition)
        {
            if (Camera.main == null)
                return;

            // клик по UI — не действие в мире
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // ночью или вымотанным игрок не работает
            if (_gameTimeService.GameTime.DayPhase.Value == DayPhase.Night || _fatigueModel.IsExhausted)
                return;

            var worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
            var cellPos3 = _grid.WorldToCell(worldPos);

            if (!IsWithinReach(cellPos3))
                return;

            var currentItem = _hotbarSlotsModel.CurrentItem.Value;
            if (currentItem.IsEmpty)
                return;

            var cellPos = new Vector2Int(cellPos3.x, cellPos3.y);

            if (HandleAction(cellPos, currentItem.Item))
            {
                _playerItemsService.PlayUseAnimation();
                _fatigueModel.AddAction(_farmConfig.FatiguePerAction);
            }
        }

        private bool IsWithinReach(Vector3Int cellPos)
        {
            var cellCenter = _grid.GetCellCenterWorld(cellPos);
            var playerPos = PlayerView.transform.position;
            var maxDistance = _farmConfig.InteractionRadius * _grid.cellSize.x;
            return Vector2.Distance(playerPos, cellCenter) <= maxDistance;
        }

        private bool HandleAction(Vector2Int cellPos, ItemDefinition item)
        {
            if (item == _farmConfig.HoeItem)
                return _farmService.TillCell(cellPos);

            if (item == _farmConfig.WateringCanItem)
                return _farmService.WaterCell(cellPos);

            if (item == _farmConfig.ScytheItem)
                return TryHarvest(cellPos);

            if (_plantsConfig.TryGetPlantBySeed(item, out var plantData))
                return TryPlant(cellPos, plantData);

            return false;
        }

        private bool TryPlant(Vector2Int cellPos, PlantData plantData)
        {
            if (!_plantSystemService.PlantSeed(cellPos, plantData.PlantType))
                return false;

            _inventoryService.RemoveItem(plantData.SeedItem, 1);
            return true;
        }

        private bool TryHarvest(Vector2Int cellPos)
        {
            if (!_plantSystemService.TryHarvest(cellPos, out var plantData))
                return false;

            _inventoryService.AddItem(plantData.HarvestItem, plantData.HarvestCount);
            return true;
        }
    }
}

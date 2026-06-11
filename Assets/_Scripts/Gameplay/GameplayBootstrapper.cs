using System.Threading;
using Farmway.Gameplay.Farm;
using Farmway.Gameplay.Player;
using Farmway.Gameplay.Services;
using Farmway.Infrastructure;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer.Unity;

namespace Farmway.Gameplay
{
    public class GameplayBootstrapper : IAsyncStartable
    {
        private readonly IPlayerFactory _playerFactory;
        private readonly GameServices _gameServices;
        private readonly IConfigProvider _configProvider;
        private readonly ISaveService _saveService;
        private readonly FarmGridView _farmGridView;
        private readonly FarmGrid _farmGrid;
        private readonly FarmService _farmService;
        private readonly PlantSystemService _plantSystemService;

        public GameplayBootstrapper(
            IPlayerFactory playerFactory,
            GameServices gameServices,
            IConfigProvider configProvider,
            ISaveService saveService,
            FarmGridView farmGridView,
            FarmGrid farmGrid,
            FarmService farmService,
            PlantSystemService plantSystemService)
        {
            _playerFactory = playerFactory;
            _gameServices = gameServices;
            _configProvider = configProvider;
            _saveService = saveService;
            _farmGridView = farmGridView;
            _farmGrid = farmGrid;
            _farmService = farmService;
            _plantSystemService = plantSystemService;
        }

        public async Awaitable StartAsync(CancellationToken cancellation = default)
        {
            try
            {
                Debug.Log("[Gameplay] StartAsync: начало");
                var farmConfig = _configProvider.GetConfig<FarmConfig>();

                _farmGridView.Grid.cellSize = new Vector3(farmConfig.CellSize, farmConfig.CellSize, 1f);

                SpawnHouse(farmConfig);
                SpawnTrader(farmConfig);
                SpawnMapBounds(farmConfig);
                Debug.Log("[Gameplay] Дом, торговец и границы созданы");

                // Жизненный цикл GameServices вручную + раннер на тиках Unity
                _gameServices.Initialize();
                _gameServices.Start();
                new GameObject("GameServicesRunner").AddComponent<GameServicesRunner>().Construct(_gameServices);
                _gameServices.EnableServices();
                Debug.Log("[Gameplay] GameServices включены");

                RestoreFarmFromSave();

                await _playerFactory.Spawn(Vector2.zero, cancellation);
                Debug.Log("[Gameplay] Игрок заспавнен — старт завершён");
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        private void RestoreFarmFromSave()
        {
            var save = _saveService.Pending;

            if (save == null)
                return;

            foreach (var cell in save.Cells)
            {
                var pos = new Vector2Int(cell.X, cell.Y);
                _farmService.TillCell(pos);

                if (cell.PlantType != 0)
                {
                    _plantSystemService.PlantSeed(pos, (PlantType)cell.PlantType);

                    if (_farmGrid.TryGetCell(pos, out var farmCell) && farmCell.HasPlant)
                    {
                        farmCell.Plant.SetStage(cell.Stage);

                        if (cell.Ready)
                            farmCell.Plant.SetReadyToHarvest();
                    }
                }

                if (cell.Watered)
                    _farmService.WaterCell(pos);
            }

            Debug.Log($"[Save] Ферма восстановлена: {save.Cells.Count} клеток, день {save.Day}");
        }

        private static void SpawnHouse(FarmConfig config)
        {
            var house = new GameObject("House");
            house.transform.position = config.HousePosition;

            var renderer = house.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = FarmSortingOrder.House;

            if (config.HouseSprite != null)
            {
                renderer.sprite = config.HouseSprite;
            }
            else
            {
                renderer.sprite = SpriteLibrary.House;
                house.transform.localScale = Vector3.one * 2f;
            }

            // Сквозь дом не ходим: коллайдер по нижней половине (вход "перед" домом)
            var collider = house.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(3f, 2f);
            collider.offset = new Vector2(0f, -0.9f);
        }

        private static void SpawnTrader(FarmConfig config)
        {
            var trader = new GameObject("Trader");
            trader.transform.position = config.TraderPosition;
            trader.transform.localScale = Vector3.one * 0.5f;

            var renderer = trader.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = FarmSortingOrder.House;
            renderer.sprite = config.TraderSprite != null
                ? config.TraderSprite
                : SpriteLibrary.Get(new Color(0.4f, 0.5f, 0.9f));

            // Локальные координаты: умножаются на scale 0.5
            var collider = trader.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.6f, 1.4f);
            collider.offset = new Vector2(0f, -0.6f);
        }

        private static void SpawnMapBounds(FarmConfig config)
        {
            // Если в сцене есть тайлмапы — границы по ним, иначе из конфига
            if (!TryGetTilemapBounds(out var min, out var max))
            {
                min = config.MapMin;
                max = config.MapMax;
            }

            Debug.Log($"[Gameplay] Границы карты: {min} .. {max}");

            var bounds = new GameObject("MapBounds");
            var width = max.x - min.x;
            var height = max.y - min.y;
            var center = (min + max) * 0.5f;

            CreateWall(bounds.transform, "Left", new Vector2(min.x - 0.5f, center.y), new Vector2(1f, height + 2f));
            CreateWall(bounds.transform, "Right", new Vector2(max.x + 0.5f, center.y), new Vector2(1f, height + 2f));
            CreateWall(bounds.transform, "Bottom", new Vector2(center.x, min.y - 0.5f), new Vector2(width + 2f, 1f));
            CreateWall(bounds.transform, "Top", new Vector2(center.x, max.y + 0.5f), new Vector2(width + 2f, 1f));
        }

        private static bool TryGetTilemapBounds(out Vector2 min, out Vector2 max)
        {
            min = Vector2.positiveInfinity;
            max = Vector2.negativeInfinity;
            bool found = false;

            foreach (var tilemap in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                tilemap.CompressBounds();

                if (tilemap.cellBounds.size.x <= 0 || tilemap.cellBounds.size.y <= 0)
                    continue;

                var worldMin = tilemap.CellToWorld(tilemap.cellBounds.min);
                var worldMax = tilemap.CellToWorld(tilemap.cellBounds.max);

                min = Vector2.Min(min, worldMin);
                max = Vector2.Max(max, worldMax);
                found = true;
            }

            return found;
        }

        private static void CreateWall(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent);
            wall.transform.position = position;

            var collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;
        }
    }
}

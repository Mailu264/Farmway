using Farmway.Infrastructure;
using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class PlantFactory
    {
        private readonly PlantsConfig _plantsConfig;
        private readonly PlantViewFactory _plantViewFactory;

        public PlantFactory(IConfigProvider configProvider, PlantViewFactory plantViewFactory)
        {
            _plantsConfig = configProvider.GetConfig<PlantsConfig>();
            _plantViewFactory = plantViewFactory;
        }

        public bool TryCreate(PlantType type, Vector2Int pos, out Plant plant)
        {
            plant = null;

            if (!_plantsConfig.TryGetPlant(type, out var data))
            {
                Debug.LogError($"[PlantFactory] No config for plant {type}");
                return false;
            }

            var view = _plantViewFactory.Create(pos);

            plant = new Plant(type);
            plant.AddService(new PlantGrowthService(data));
            plant.AddBehaviour(new PlantAnimationBehaviour(view, data));
            plant.OnPlanted();

            return true;
        }
    }
}

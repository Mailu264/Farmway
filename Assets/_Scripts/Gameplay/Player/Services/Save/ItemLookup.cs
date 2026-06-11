using System.Collections.Generic;
using Farmway.Gameplay.Farm;
using Farmway.Infrastructure;

namespace Farmway.Gameplay.Player
{
    // Имя ассета → ItemDefinition. Собирается из конфигов, нужен сейвам.
    public static class ItemLookup
    {
        public static Dictionary<string, ItemDefinition> Build(IConfigProvider configProvider)
        {
            var lookup = new Dictionary<string, ItemDefinition>();
            var farmConfig = configProvider.GetConfig<FarmConfig>();
            var plantsConfig = configProvider.GetConfig<PlantsConfig>();

            Add(lookup, farmConfig.HoeItem);
            Add(lookup, farmConfig.WateringCanItem);
            Add(lookup, farmConfig.ScytheItem);

            foreach (var item in farmConfig.ShopItems)
                Add(lookup, item);

            foreach (var starting in farmConfig.StartingItems)
                Add(lookup, starting.Item);

            foreach (var type in System.Enum.GetValues(typeof(PlantType)))
            {
                if (plantsConfig.TryGetPlant((PlantType)type, out var plantData))
                {
                    Add(lookup, plantData.SeedItem);
                    Add(lookup, plantData.HarvestItem);
                }
            }

            return lookup;
        }

        private static void Add(Dictionary<string, ItemDefinition> lookup, ItemDefinition item)
        {
            if (item != null)
                lookup.TryAdd(item.name, item);
        }
    }
}

using System;
using System.Collections.Generic;
using Farmway.Gameplay.Player;
using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    [CreateAssetMenu(fileName = "PlantsConfig", menuName = "Configs/Gameplay/PlantsConfig")]
    public class PlantsConfig : ScriptableObject
    {
        [SerializeField] private PlantData[] _plants;

        private Dictionary<PlantType, PlantData> _lookup;
        private Dictionary<ItemDefinition, PlantData> _seedLookup;

        public bool TryGetPlant(PlantType type, out PlantData data)
        {
            _lookup ??= BuildLookup();
            return _lookup.TryGetValue(type, out data);
        }

        public bool TryGetPlantBySeed(ItemDefinition seedItem, out PlantData data)
        {
            data = null;
            if (seedItem == null)
                return false;

            _seedLookup ??= BuildSeedLookup();
            return _seedLookup.TryGetValue(seedItem, out data);
        }

        private Dictionary<PlantType, PlantData> BuildLookup()
        {
            var dict = new Dictionary<PlantType, PlantData>();
            foreach (var plant in _plants)
                dict[plant.PlantType] = plant;
            return dict;
        }

        private Dictionary<ItemDefinition, PlantData> BuildSeedLookup()
        {
            var dict = new Dictionary<ItemDefinition, PlantData>();
            foreach (var plant in _plants)
                if (plant.SeedItem != null)
                    dict[plant.SeedItem] = plant;
            return dict;
        }
    }

    [Serializable]
    public class PlantData
    {
        public PlantType PlantType;
        public ItemDefinition SeedItem;
        public ItemDefinition HarvestItem;
        public int HarvestCount = 1;
        // Каждая стадия = одна ночь. Количество стадий = сколько ночей растёт.
        public PlantStageData[] Stages;
    }

    [Serializable]
    public class PlantStageData
    {
        public Sprite Sprite;
    }
}

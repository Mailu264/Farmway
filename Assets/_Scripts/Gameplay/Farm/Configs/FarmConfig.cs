using System;
using System.Collections.Generic;
using Farmway.Gameplay.Player;
using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    [CreateAssetMenu(fileName = "FarmConfig", menuName = "Configs/Gameplay/FarmConfig")]
    public class FarmConfig : ScriptableObject
    {
        [field: SerializeField] public float InteractionRadius { get; private set; } = 5f;
        [field: SerializeField, Tooltip("Размер клетки грядки в юнитах")]
        public float CellSize { get; private set; } = 1.5f;

        [field: Header("Tool Items")]
        [field: SerializeField] public ItemDefinition HoeItem { get; private set; }
        [field: SerializeField] public ItemDefinition WateringCanItem { get; private set; }
        [field: SerializeField] public ItemDefinition ScytheItem { get; private set; }

        [field: Header("Sprites")]
        [field: SerializeField] public Sprite TilledSprite { get; private set; }
        [field: SerializeField] public Sprite WateredSprite { get; private set; }
        [field: SerializeField] public Sprite HouseSprite { get; private set; }
        [field: SerializeField] public Sprite TraderSprite { get; private set; }

        [field: Header("House / Sleep")]
        [field: SerializeField] public Vector2 HousePosition { get; private set; } = new(5f, 3f);
        [field: SerializeField] public float SleepRadius { get; private set; } = 2.5f;

        [field: Header("Trader")]
        [field: SerializeField] public Vector2 TraderPosition { get; private set; } = new(-5f, 2f);
        [field: SerializeField] public float TraderRadius { get; private set; } = 2.5f;

        [field: Header("Map Bounds")]
        [field: SerializeField] public Vector2 MapMin { get; private set; } = new(-12f, -8f);
        [field: SerializeField] public Vector2 MapMax { get; private set; } = new(12f, 8f);

        [field: Header("Fatigue")]
        [field: SerializeField, Tooltip("Сколько усталости добавляет одно действие (0..1)")]
        public float FatiguePerAction { get; private set; } = 0.03f;

        [field: Header("Economy")]
        [field: SerializeField] public int StartMoney { get; private set; } = 50;
        [field: SerializeField] public List<ItemDefinition> ShopItems { get; private set; } = new();

        [field: Header("Starting Items (кладутся в хотбар)")]
        [field: SerializeField] public List<StartingItemData> StartingItems { get; private set; } = new();
    }

    [Serializable]
    public class StartingItemData
    {
        public ItemDefinition Item;
        public int Count = 1;
    }
}

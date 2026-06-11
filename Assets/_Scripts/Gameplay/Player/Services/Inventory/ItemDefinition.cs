using UnityEngine;

namespace Farmway.Gameplay.Player
{
    [CreateAssetMenu(fileName = "Item", menuName = "Configs/Items/Item")]
    public class ItemDefinition : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public ItemTypesEnum Type { get; private set; }
        [field: SerializeField] public int MaxStackSize { get; private set; } = 1;
        [field: SerializeField] public bool IsStackable { get; private set; }

        [field: Header("Shop")]
        [field: SerializeField] public int BuyPrice { get; private set; }
        [field: SerializeField] public int SellPrice { get; private set; }
    }
}

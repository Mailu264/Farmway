using TMPro;
using UnityEngine;

namespace Farmway.Gameplay.Player.Services.Inventory
{
    public class InventorySlotView : MonoBehaviour
    {
        [field: SerializeField] public SpriteRenderer IconPlacement { get; private set; }
        [field: SerializeField] public TextMeshProUGUI Count { get; private set; }
    }
}
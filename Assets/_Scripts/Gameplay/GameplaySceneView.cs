using Farmway.Gameplay.Player;
using UnityEngine;

namespace Farmway.Gameplay
{
    public class GameplaySceneView : MonoBehaviour
    {
        [field: SerializeField] public CameraView CameraView { get; private set; }
        [field: SerializeField] public InventoryView InventoryView { get; private set; }
        [field: SerializeField] public HotbarView HotbarView { get; private set; }
    }
}

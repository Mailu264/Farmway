using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Farmway.Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/Player/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [field: SerializeField] public AssetReferenceGameObject Prefab { get; private set; }
        [field: SerializeField] public float Speed { get; private set; }
    }
}

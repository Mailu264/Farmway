using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Farmway.Infrastructure
{
    [CreateAssetMenu(fileName = "ScenesConfig", menuName = "Configs/ScenesConfig")]
    public class ScenesConfig : ScriptableObject
    {
        [field: SerializeField] public AssetReference GameScene { get; private set; }
    }
}

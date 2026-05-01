using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Farmway.Infrastructure
{
    public interface ISceneLoader
    {
        UniTask Load(AssetReference sceneReference, CancellationToken cancellation);
    }
}

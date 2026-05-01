using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Farmway.Infrastructure
{
    public class SceneLoader : ISceneLoader
    {
        public async UniTask Load(AssetReference sceneReference, CancellationToken cancellation)
        {
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneReference);
            await handle.WithCancellation(cancellation);
        }
    }
}

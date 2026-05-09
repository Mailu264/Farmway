using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Farmway.Infrastructure
{
    public interface IAssetProvider
    {
        UniTask WarmupAsync(CancellationToken ct);
        UniTask<GameObject> LoadAssetAsync(AssetReference path, CancellationToken ct);
        UniTask<TObject> LoadAssetAsync<TObject>(AssetReference path, CancellationToken ct) where TObject : Component;
        UniTask<List<T>> LoadAssetsByLabelAsync<T>(string label, CancellationToken ct) where T : class;
        void Cleanup();
    }
}

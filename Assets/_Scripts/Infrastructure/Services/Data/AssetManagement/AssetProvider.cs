using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Farmway.Infrastructure
{
    public class AssetProvider : IAssetProvider
    {
        private readonly Dictionary<string, object> _cache = new();
        private readonly Dictionary<string, AsyncOperationHandle> _handles = new();

        private const string WarmupLabel = "Warmup";

        public async UniTask WarmupAsync(CancellationToken ct) =>
            await LoadAssetsByLabelAsync<GameObject>(WarmupLabel, ct);

        public async UniTask<GameObject> LoadAssetAsync(AssetReference asset, CancellationToken ct)
        {
            var key = asset.RuntimeKey.ToString();

            if (_cache.TryGetValue(key, out var cached))
                return (GameObject)cached;

            var handle = Addressables.LoadAssetAsync<GameObject>(asset);
            await handle.ToUniTask(cancellationToken: ct);

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load asset: {asset}");
                return null;
            }

            _cache[key] = handle.Result;
            _handles[key] = handle;
            return handle.Result;
        }

        public async UniTask<TObject> LoadAssetAsync<TObject>(AssetReference asset, CancellationToken ct) where TObject : Component
        {
            var key = asset.RuntimeKey.ToString();

            if (_cache.TryGetValue(key, out var cached))
            {
                ((GameObject)cached).TryGetComponent(out TObject component);
                return component;
            }

            var handle = Addressables.LoadAssetAsync<GameObject>(asset);
            await handle.ToUniTask(cancellationToken: ct);

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load component {typeof(TObject).Name}: {asset}");
                return null;
            }

            _cache[key] = handle.Result;
            _handles[key] = handle;
            handle.Result.TryGetComponent(out TObject result);
            return result;
        }

        public async UniTask<List<T>> LoadAssetsByLabelAsync<T>(string label, CancellationToken ct) where T : class
        {
            if (_cache.TryGetValue(label, out var cached))
                return (List<T>)cached;

            var handle = Addressables.LoadAssetsAsync<T>(label);
            await handle.ToUniTask(cancellationToken: ct);

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load assets by label: {label}");
                return new List<T>();
            }

            var list = handle.Result.ToList();
            _cache[label] = list;
            _handles[label] = handle;
            return list;
        }

        public void Cleanup()
        {
            foreach (var handle in _handles.Values)
                if (handle.IsValid())
                    Addressables.Release(handle);

            _handles.Clear();
            _cache.Clear();
        }
    }
}

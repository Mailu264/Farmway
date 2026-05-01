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
        private readonly Dictionary<string, List<AsyncOperationHandle>> _usedResources = new();

        public async UniTask<GameObject> LoadAssetAsync(AssetReference path, CancellationToken ct)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(path);
            await handle.ToUniTask(cancellationToken: ct);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                RegisterForCleanup(path.RuntimeKey.ToString(), handle);
                return handle.Result;
            }

            Debug.LogError($"Load prefab by path: {path} error!");
            return null;
        }

        public async UniTask<TObject> LoadAssetAsync<TObject>(AssetReference path, CancellationToken ct) where TObject : Component
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(path);
            await handle.ToUniTask(cancellationToken: ct);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                handle.Result.TryGetComponent(out TObject component);
                if (component != null)
                {
                    RegisterForCleanup(path.RuntimeKey.ToString(), handle);
                    return component;
                }
            }

            Debug.LogError($"Load component {typeof(TObject).Name} from prefab by path: {path} error!");
            return null;
        }

        public async UniTask<List<T>> LoadAssetsByLabelAsync<T>(string label, CancellationToken ct) where T : class
        {
            var handle = Addressables.LoadAssetsAsync<T>(label);
            await handle.ToUniTask(cancellationToken: ct);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                RegisterForCleanup(label, handle);
                return handle.Result.ToList();
            }

            Debug.LogError($"Load asset by label: {label} error");
            return new List<T>();
        }

        public void Cleanup()
        {
            foreach (var key in _usedResources.Keys.ToList())
            {
                for (int i = _usedResources[key].Count - 1; i >= 0; i--)
                {
                    var handle = _usedResources[key][i];

                    if (handle.IsValid())
                        Addressables.Release(handle);

                    _usedResources[key].RemoveAt(i);
                }

                _usedResources.Remove(key);
            }
        }

        private void RegisterForCleanup<T>(string key, AsyncOperationHandle<T> handle)
        {
            if (!handle.IsValid())
                return;

            if (!_usedResources.TryGetValue(key, out var resourceHandles))
            {
                resourceHandles = new List<AsyncOperationHandle>();
                _usedResources[key] = resourceHandles;
            }

            resourceHandles.Add(handle);
        }
    }
}

using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Farmway.Infrastructure
{
    public class ObjectPool : IObjectPool, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pooledObjects = new();
        private readonly CompositeDisposable _returnInstancesTimersDisposable = new();
        private readonly HashSet<GameObject> _returnInstances = new();

        private const int DefaultPrewarmCount = 10;

        public ObjectPool(IObjectResolver resolver) => _resolver = resolver;

        public void Warmup(GameObject prefab, Vector3 position = default, Quaternion rotation = default,
                           Transform root = null, int count = DefaultPrewarmCount)
        {
            if (!_pooledObjects.ContainsKey(prefab))
                RegisterPrefabInternal(prefab, position, rotation, root, true, count);
        }

        public GameObject GetGameObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform root = null,
                                        bool useBatchSpawn = true, int count = DefaultPrewarmCount)
        {
            if (!_pooledObjects.ContainsKey(prefab))
                RegisterPrefabInternal(prefab, position, rotation, root, useBatchSpawn, count);

            var instance = _pooledObjects[prefab].Get();
            var transform = instance.transform;

            transform.position = position;
            transform.rotation = rotation;
            transform.SetParent(root);

            instance.SetActive(true);
            return instance;
        }

        public T GetGameObject<T>(T prefab, Vector3 position, Quaternion rotation, Transform root = null,
                                  bool useBatchSpawn = true, int count = DefaultPrewarmCount) where T : MonoBehaviour
        {
            return GetGameObject<T>(prefab.gameObject, position, rotation, root, useBatchSpawn, count);
        }

        public T GetGameObject<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform root = null,
                                  bool useBatchSpawn = true, int count = DefaultPrewarmCount) where T : MonoBehaviour
        {
            var instance = GetGameObject(prefab, position, rotation, root, useBatchSpawn, count);
            return instance.GetComponent<T>();
        }

        public void ReturnGameObject(GameObject instance, GameObject prefab)
        {
            if (_pooledObjects.TryGetValue(prefab, out var pool))
                pool.Release(instance);
        }

        public void ReturnGameObject<T>(T instance, GameObject prefab) where T : MonoBehaviour
        {
            if (_pooledObjects.TryGetValue(prefab, out var pool))
                pool.Release(instance.gameObject);
        }

        public void ReturnGameObject<T>(T instance, T prefab) where T : MonoBehaviour
        {
            var key = prefab.gameObject;
            if (_pooledObjects.TryGetValue(key, out var pool))
                pool.Release(instance.gameObject);
        }

        public void ReturnGameObject(GameObject instance, GameObject prefab, float seconds, Action<GameObject> beforeReturn = null, CompositeDisposable disposable = null)
        {
            if (!_returnInstances.Add(instance))
                return;

            _returnInstancesTimersDisposable.Add(disposable);

            Observable
                .Timer(TimeSpan.FromSeconds(seconds))
                .Subscribe(_ =>
                {
                    beforeReturn?.Invoke(instance);
                    ReturnGameObject(instance, prefab);
                    _returnInstances.Remove(instance);
                })
                .AddTo(_returnInstancesTimersDisposable);
        }

        public void ReturnGameObject<T>(T instance, GameObject prefab, float seconds, Action<T> beforeReturn = null, CompositeDisposable disposable = null) where T : MonoBehaviour
        {
            if (!_returnInstances.Add(instance.gameObject))
                return;

            _returnInstancesTimersDisposable.Add(disposable);

            Observable
                .Timer(TimeSpan.FromSeconds(seconds))
                .Subscribe(_ =>
                {
                    beforeReturn?.Invoke(instance);
                    ReturnGameObject(instance, prefab);
                    _returnInstances.Remove(instance.gameObject);
                })
                .AddTo(_returnInstancesTimersDisposable);
        }

        public void ReturnGameObject<T>(T instance, T prefab, float seconds, Action<T> beforeReturn = null, CompositeDisposable disposable = null) where T : MonoBehaviour
        {
            if (!_returnInstances.Add(instance.gameObject))
                return;

            _returnInstancesTimersDisposable.Add(disposable);

            Observable
                .Timer(TimeSpan.FromSeconds(seconds))
                .Subscribe(_ =>
                {
                    beforeReturn?.Invoke(instance);
                    ReturnGameObject(instance, prefab);
                    _returnInstances.Remove(instance.gameObject);
                })
                .AddTo(_returnInstancesTimersDisposable);
        }

        private void RegisterPrefabInternal(GameObject prefab, Vector3 position, Quaternion rotation, Transform root,
                                            bool useBatchSpawn = false, int prewarmCount = 0)
        {
            GameObject CreateFunc() => _resolver.Instantiate(prefab, position, rotation, root);
            void OnGet(GameObject go) => go.SetActive(false);
            void OnRelease(GameObject go) => go.SetActive(false);
            void OnDestroy(GameObject go) => Object.Destroy(go);

            var pool = new ObjectPool<GameObject>(CreateFunc, OnGet, OnRelease, OnDestroy,
                defaultCapacity: useBatchSpawn ? prewarmCount : 0);
            _pooledObjects.Add(prefab, pool);

            if (!useBatchSpawn)
                return;

            var objs = new GameObject[prewarmCount];

            for (var i = 0; i < prewarmCount; i++)
                objs[i] = pool.Get();

            foreach (var obj in objs)
                pool.Release(obj);
        }

        public void Dispose()
        {
            foreach (var pool in _pooledObjects.Values)
                pool.Clear();

            _pooledObjects.Clear();
            _returnInstancesTimersDisposable.Dispose();
        }
    }
}

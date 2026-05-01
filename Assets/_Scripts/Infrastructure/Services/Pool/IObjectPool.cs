using System;
using UniRx;
using UnityEngine;

namespace Farmway.Infrastructure
{
    public interface IObjectPool
    {
        void Warmup(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform root = null, int count = 0);
        GameObject GetGameObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform root = null, bool useBatchSpawn = true, int overridePrewarmCount = 0);
        T GetGameObject<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform root = null, bool useBatchSpawn = true, int overridePrewarmCount = 0) where T : MonoBehaviour;
        T GetGameObject<T>(T prefab, Vector3 position, Quaternion rotation, Transform root = null, bool useBatchSpawn = true, int overridePrewarmCount = 0) where T : MonoBehaviour;

        void ReturnGameObject(GameObject instance, GameObject prefab);
        void ReturnGameObject<T>(T instance, GameObject prefab) where T : MonoBehaviour;
        void ReturnGameObject<T>(T instance, T prefab) where T : MonoBehaviour;

        void ReturnGameObject(GameObject instance, GameObject prefab, float seconds, Action<GameObject> beforeReturn = null, CompositeDisposable disposable = null);
        void ReturnGameObject<T>(T instance, GameObject prefab, float seconds, Action<T> beforeReturn = null, CompositeDisposable disposable = null) where T : MonoBehaviour;
        void ReturnGameObject<T>(T instance, T prefab, float seconds, Action<T> beforeReturn = null, CompositeDisposable disposable = null) where T : MonoBehaviour;
    }
}

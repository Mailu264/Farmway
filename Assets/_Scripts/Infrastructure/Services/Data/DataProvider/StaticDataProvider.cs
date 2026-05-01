using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Farmway.Infrastructure
{
    public class StaticDataProvider : IStaticDataProvider
    {
        private const string Config = "Config";

        private readonly IAssetProvider _assetProvider;
        private List<ScriptableObject> _staticData;

        public StaticDataProvider(IAssetProvider assetProvider) =>
            _assetProvider = assetProvider;

        public async UniTask Initialize(CancellationToken ct) =>
            _staticData = await _assetProvider.LoadAssetsByLabelAsync<ScriptableObject>(Config, ct);

        public TData GetConfig<TData>() =>
            GetFirstDataOfType<TData>();

        private TData GetFirstDataOfType<TData>()
        {
            foreach (ScriptableObject data in _staticData)
            {
                if (data is TData dataOfType)
                    return dataOfType;
            }

            throw new ArgumentNullException($"Config {typeof(TData).Name} not found!");
        }
    }
}

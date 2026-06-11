using System;
using Farmway.Gameplay.Farm;
using Farmway.Infrastructure;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    // Магазин открывается у торговца по E, не хоткеем
    public class PlayerShopService : PlayerService
    {
        private readonly IInputService _inputService;
        private readonly FarmConfig _farmConfig;

        private readonly ReactiveProperty<bool> _isNearTrader = new();
        private readonly Subject<Unit> _toggleRequested = new();
        private readonly CompositeDisposable _disposables = new();

        public IReadOnlyReactiveProperty<bool> IsNearTrader => _isNearTrader;
        public IObservable<Unit> OnToggleRequested => _toggleRequested;

        public PlayerShopService(IInputService inputService, IConfigProvider configProvider)
        {
            _inputService = inputService;
            _farmConfig = configProvider.GetConfig<FarmConfig>();
        }

        public override void OnEnable()
        {
            _inputService.OnUsePressed
                .Subscribe(_ => TryToggleShop())
                .AddTo(_disposables);
        }

        public override void OnDisable() => _disposables.Clear();
        public override void OnDispose() => _disposables.Dispose();

        public override void OnUpdate()
        {
            Vector2 playerPos = PlayerView.transform.position;
            _isNearTrader.Value = Vector2.Distance(playerPos, _farmConfig.TraderPosition) <= _farmConfig.TraderRadius;
        }

        private void TryToggleShop()
        {
            if (_isNearTrader.Value)
                _toggleRequested.OnNext(Unit.Default);
        }
    }
}

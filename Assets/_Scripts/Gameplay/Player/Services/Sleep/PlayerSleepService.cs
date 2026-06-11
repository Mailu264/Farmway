using System;
using Farmway.Gameplay.Farm;
using Farmway.Gameplay.Services;
using Farmway.Infrastructure;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public class PlayerSleepService : PlayerService
    {
        private readonly IInputService _inputService;
        private readonly IGameTimeService _gameTimeService;
        private readonly FarmService _farmService;
        private readonly PlantSystemService _plantSystemService;
        private readonly FatigueModel _fatigueModel;
        private readonly GameSaver _gameSaver;
        private readonly FarmConfig _farmConfig;

        private readonly ReactiveProperty<bool> _isNearHouse = new();
        private readonly Subject<Unit> _sleepRequested = new();
        private readonly CompositeDisposable _disposables = new();

        private bool _isSleeping;

        public IReadOnlyReactiveProperty<bool> IsNearHouse => _isNearHouse;
        // HUD слушает запрос, играет затемнение и дёргает CompleteSleep/FinishSleep
        public IObservable<Unit> OnSleepRequested => _sleepRequested;

        public PlayerSleepService(
            IInputService inputService,
            IGameTimeService gameTimeService,
            FarmService farmService,
            PlantSystemService plantSystemService,
            FatigueModel fatigueModel,
            GameSaver gameSaver,
            IConfigProvider configProvider)
        {
            _inputService = inputService;
            _gameTimeService = gameTimeService;
            _farmService = farmService;
            _plantSystemService = plantSystemService;
            _fatigueModel = fatigueModel;
            _gameSaver = gameSaver;
            _farmConfig = configProvider.GetConfig<FarmConfig>();
        }

        public override void OnEnable()
        {
            _inputService.OnUsePressed
                .Subscribe(_ => TrySleep())
                .AddTo(_disposables);
        }

        public override void OnDisable() => _disposables.Clear();
        public override void OnDispose() => _disposables.Dispose();

        public override void OnUpdate()
        {
            Vector2 playerPos = PlayerView.transform.position;
            _isNearHouse.Value = Vector2.Distance(playerPos, _farmConfig.HousePosition) <= _farmConfig.SleepRadius;
        }

        // Вызывается HUD'ом на пике затемнения: мир меняется пока экран чёрный
        public void CompleteSleep()
        {
            _plantSystemService.AdvanceDay();
            _farmService.ResetDailyWatering();
            _fatigueModel.Reset();
            _gameTimeService.StartNewDay();
            _gameSaver.Save(); // автосейв каждую ночь
        }

        public void FinishSleep() =>
            _isSleeping = false;

        private void TrySleep()
        {
            if (_isSleeping || !_isNearHouse.Value)
                return;

            _isSleeping = true;
            _sleepRequested.OnNext(Unit.Default);
        }
    }
}

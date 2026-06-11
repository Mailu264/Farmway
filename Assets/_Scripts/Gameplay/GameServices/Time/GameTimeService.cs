using Farmway.Infrastructure;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Services
{
    public interface IGameTimeService
    {
        IGameTimeData GameTime { get; }
        void StartNewDay();
    }

    public class GameTimeService : GameServiceBase, IGameTimeService
    {
        private const float FallbackDayTime = 120f;

        private readonly GameTimeData _gameTime = new();
        private readonly ISaveService _saveService;
        private float _totalVirtualMinutes;
        private float _dayTime;
        private int _hourStart;
        private bool _configured;

        public IGameTimeData GameTime => _gameTime;

        public GameTimeService(ISaveService saveService) =>
            _saveService = saveService;

        public override void OnInitialize()
        {
            if (_saveService.Pending != null)
                _gameTime.SetDay(_saveService.Pending.Day);

            EnsureConfigured();
        }

        public override void OnEnable()
        {
            EnsureConfigured();
            Debug.Log("[GameTime] Enabled — время пошло");
            _gameTime.SetTimeLeft(_dayTime);
            _gameTime.SetDayPhase(DayPhase.Day);
        }

        // Не зависит от порядка жизненного цикла: настройка при первом обращении
        private void EnsureConfigured()
        {
            if (_configured)
                return;

            _configured = true;
            _dayTime = TimeConfig != null ? TimeConfig.DayTime : 0f;
            _hourStart = TimeConfig != null ? TimeConfig.HourStartDay : 9;
            var totalVirtualHours = TimeConfig != null ? TimeConfig.HourEndDay - TimeConfig.HourStartDay : 0;

            // Конфиг с нулями молча останавливает время — подстраховка
            if (_dayTime <= 0f)
            {
                Debug.LogError($"[GameTime] DayTime не задан в TimeConfig — использую {FallbackDayTime}с");
                _dayTime = FallbackDayTime;
            }

            if (totalVirtualHours <= 0)
            {
                Debug.LogError("[GameTime] Часы дня заданы неверно — использую 9-18");
                _hourStart = 9;
                totalVirtualHours = 9;
            }

            _totalVirtualMinutes = totalVirtualHours * 60;
            Debug.Log($"[GameTime] Configured: день {_dayTime}с, {_hourStart}:00-{_hourStart + totalVirtualHours}:00");
        }

        public void StartNewDay()
        {
            _gameTime.NextDay();
            _gameTime.SetTimeLeft(_dayTime);
            _gameTime.SetTime(_hourStart, 0, 0);
            _gameTime.SetDayPhase(DayPhase.Day);

            if (!IsEnabled)
                Enable();
        }

        public override void OnUpdate()
        {
            _gameTime.SetTimeLeft(_gameTime.TimeLeft.Value - Time.deltaTime);

            var progress = Mathf.Clamp01(1f - _gameTime.TimeLeft.Value / _dayTime);
            var passedMinutes = Mathf.FloorToInt(progress * _totalVirtualMinutes);
            var currentMinutes = passedMinutes % 60 / 10 * 10; // шаг 10 минут, чтобы часы не мельтешили
            var currentHours = _hourStart + (passedMinutes / 60);

            _gameTime.SetTime(currentHours, currentMinutes, progress);

            if (!(_gameTime.TimeLeft.Value <= 0))
                return;

            _gameTime.SetDayPhase(DayPhase.Night);
            Disable();
        }
    }
    
    public class GameTimeData : IGameTimeData
    {
        private readonly ReactiveProperty<int> _day = new(1);
        private readonly ReactiveProperty<int> _hours = new();
        private readonly ReactiveProperty<int> _minutes = new();
        private readonly ReactiveProperty<float> _progress = new();
        private readonly ReactiveProperty<float> _timeLeft = new();
        private readonly ReactiveProperty<DayPhase> _dayPhase = new();

        public IReadOnlyReactiveProperty<int> Day => _day;
        public IReadOnlyReactiveProperty<int> Hours => _hours;
        public IReadOnlyReactiveProperty<int> Minutes => _minutes;
        public IReadOnlyReactiveProperty<float> Progress => _progress;
        public IReadOnlyReactiveProperty<float> TimeLeft => _timeLeft;
        public IReadOnlyReactiveProperty<DayPhase> DayPhase => _dayPhase;

        public void NextDay() =>
            _day.Value++;

        public void SetDay(int day) =>
            _day.Value = Mathf.Max(1, day);

        public void SetTimeLeft(float timeLeft) =>
            _timeLeft.Value = Mathf.Max(timeLeft, 0);

        public void SetDayPhase(DayPhase dayPhase) =>
            _dayPhase.Value = dayPhase;

        public void SetTime(int hours, int minutes, float progress)
        {
            _hours.Value = hours;
            _minutes.Value = minutes;
            _progress.Value = progress;
        }
    }

    public interface IGameTimeData
    {
        public IReadOnlyReactiveProperty<int> Day { get; }
        public IReadOnlyReactiveProperty<int> Hours { get; }
        public IReadOnlyReactiveProperty<int> Minutes { get; }
        public IReadOnlyReactiveProperty<float> Progress { get; }
        public IReadOnlyReactiveProperty<float> TimeLeft { get; }
        public IReadOnlyReactiveProperty<DayPhase> DayPhase { get; }
    }

    public enum DayPhase
    {
        None = 0,
        Day = 1,
        Night = 2,
    }
}
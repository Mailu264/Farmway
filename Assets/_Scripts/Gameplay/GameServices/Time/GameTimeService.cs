using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Services
{
    public interface IGameTimeService
    {
        IGameTimeData GameTime { get; }
    }

    public class GameTimeService : GameServiceBase, IGameTimeService
    {
        private readonly GameTimeData _gameTime = new();
        private float _totalVirtualMinutes;

        public IGameTimeData GameTime => _gameTime;

        public override void OnInitialize()
        {
            var totalVirtualHours = TimeConfig.HourEndDay - TimeConfig.HourStartDay;
            _totalVirtualMinutes = totalVirtualHours * 60;
        }

        public override void OnEnable()
        {
            _gameTime.SetTimeLeft(TimeConfig.DayTime);
            _gameTime.SetDayPhase(DayPhase.Day);
        }

        public override void OnUpdate()
        {
            _gameTime.SetTimeLeft(_gameTime.TimeLeft.Value - Time.deltaTime);

            var progress = Mathf.Clamp01(1f - _gameTime.TimeLeft.Value / TimeConfig.DayTime);
            var passedMinutes = Mathf.FloorToInt(progress * _totalVirtualMinutes);
            var currentMinutes = passedMinutes % 60;
            var currentHours = TimeConfig.HourStartDay + (passedMinutes / 60);

            _gameTime.SetTime(currentHours, currentMinutes, progress);

            if (!(_gameTime.TimeLeft.Value <= 0))
                return;
            
            _gameTime.SetDayPhase(DayPhase.Night);
            Disable();
        }
    }
    
    public class GameTimeData : IGameTimeData
    {
        private readonly ReactiveProperty<int> _hours = new();            
        private readonly ReactiveProperty<int> _minutes = new();
        private readonly ReactiveProperty<float> _progress = new();
        private readonly ReactiveProperty<float> _timeLeft = new();
        private readonly ReactiveProperty<DayPhase> _dayPhase = new();

        public IReadOnlyReactiveProperty<int> Hours => _hours;
        public IReadOnlyReactiveProperty<int> Minutes => _minutes;
        public IReadOnlyReactiveProperty<float> Progress => _progress;
        public IReadOnlyReactiveProperty<float> TimeLeft => _timeLeft;
        public IReadOnlyReactiveProperty<DayPhase> DayPhase => _dayPhase;

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
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    // Усталость 0..1: базово растёт с прогрессом дня, действия добавляют сверху
    public class FatigueModel
    {
        private readonly ReactiveProperty<float> _value = new();

        private float _actionFatigue;

        public IReadOnlyReactiveProperty<float> Value => _value;
        public bool IsExhausted => _value.Value >= 1f;

        public void UpdateBase(float dayProgress) =>
            _value.Value = Mathf.Clamp01(dayProgress + _actionFatigue);

        public void AddAction(float amount)
        {
            _actionFatigue += Mathf.Max(0f, amount);
            _value.Value = Mathf.Clamp01(_value.Value + amount);
        }

        public void Reset()
        {
            _actionFatigue = 0f;
            _value.Value = 0f;
        }
    }
}

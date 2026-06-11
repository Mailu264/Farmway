using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public class MoneyModel
    {
        private readonly ReactiveProperty<int> _amount = new();

        public IReadOnlyReactiveProperty<int> Amount => _amount;

        public void SetAmount(int amount) =>
            _amount.Value = Mathf.Max(0, amount);

        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            _amount.Value += amount;
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || _amount.Value < amount)
                return false;

            _amount.Value -= amount;
            return true;
        }
    }
}

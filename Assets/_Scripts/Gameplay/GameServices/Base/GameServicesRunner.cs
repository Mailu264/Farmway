using UnityEngine;

namespace Farmway.Gameplay.Services
{
    // Тикает GameServices через Unity-апдейты напрямую — не зависит от VContainer PlayerLoop
    public class GameServicesRunner : MonoBehaviour
    {
        private GameServices _services;

        public void Construct(GameServices services) =>
            _services = services;

        private void Update() => _services?.Tick();
        private void FixedUpdate() => _services?.FixedTick();
        private void LateUpdate() => _services?.LateTick();
    }
}

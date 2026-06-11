using Farmway.Gameplay.Services;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Farmway.Gameplay.Farm
{
    // Глобальный 2D-свет: рассвет → день → закат → ночь по прогрессу дня
    public class DayLightService : GameServiceBase
    {
        private static readonly Color DayColor = Color.white;
        private static readonly Color DawnColor = new(1f, 0.85f, 0.7f);
        private static readonly Color SunsetColor = new(1f, 0.62f, 0.4f);
        private static readonly Color NightColor = new(0.35f, 0.4f, 0.65f);

        private readonly IGameTimeService _gameTimeService;

        private Light2D _globalLight;
        private bool _lightCreated;

        public DayLightService(IGameTimeService gameTimeService) =>
            _gameTimeService = gameTimeService;

        private void EnsureLight()
        {
            if (_lightCreated)
                return;

            _lightCreated = true;
            _globalLight = FindGlobalLight();

            if (_globalLight == null)
            {
                var go = new GameObject("RuntimeGlobalLight");
                _globalLight = go.AddComponent<Light2D>();
                _globalLight.lightType = Light2D.LightType.Global;
            }
        }

        public override void OnUpdate()
        {
            EnsureLight();

            if (_globalLight == null)
                return;

            if (_gameTimeService.GameTime.DayPhase.Value == DayPhase.Night)
            {
                _globalLight.color = NightColor;
                _globalLight.intensity = 0.45f;
                return;
            }

            _globalLight.intensity = 1f;
            _globalLight.color = EvaluateColor(_gameTimeService.GameTime.Progress.Value);
        }

        private static Color EvaluateColor(float progress)
        {
            if (progress < 0.1f)
                return Color.Lerp(DawnColor, DayColor, progress / 0.1f);

            if (progress < 0.65f)
                return DayColor;

            if (progress < 0.9f)
                return Color.Lerp(DayColor, SunsetColor, (progress - 0.65f) / 0.25f);

            return Color.Lerp(SunsetColor, NightColor, (progress - 0.9f) / 0.1f);
        }

        private static Light2D FindGlobalLight()
        {
            foreach (var light in Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None))
                if (light.lightType == Light2D.LightType.Global)
                    return light;

            return null;
        }
    }
}

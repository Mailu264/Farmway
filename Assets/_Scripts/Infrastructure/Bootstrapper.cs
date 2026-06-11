using System.Threading;
using Farmway.Gameplay.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace Farmway.Infrastructure
{
    public class Bootstrapper : IAsyncStartable
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly IAssetProvider _assetProvider;
        private readonly IConfigProvider _configProvider;
        private readonly ISaveService _saveService;

        private bool _choiceMade;

        public Bootstrapper(
            ISceneLoader sceneLoader,
            IAssetProvider assetProvider,
            IConfigProvider configProvider,
            ISaveService saveService)
        {
            _sceneLoader = sceneLoader;
            _assetProvider = assetProvider;
            _configProvider = configProvider;
            _saveService = saveService;
        }

        public async Awaitable StartAsync(CancellationToken cancellation = default)
        {
            await _configProvider.Initialize(cancellation);
            await _assetProvider.WarmupAsync(cancellation);

            await ShowMainMenu(cancellation);
            _saveService.LoadPending();

            var scenesConfig = _configProvider.GetConfig<ScenesConfig>();
            await _sceneLoader.Load(scenesConfig.GameScene, cancellation);
        }

        private async Awaitable ShowMainMenu(CancellationToken ct)
        {
            var canvas = UiBuilder.CreateCanvas("MainMenu", 500);

            UiBuilder.CreateOverlay(canvas.transform, "Background", new Color(0.08f, 0.1f, 0.08f, 1f));

            UiBuilder.CreateText(
                canvas.transform, "Title",
                new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(600f, 80f),
                64, TextAnchor.MiddleCenter, new Color(0.95f, 0.85f, 0.5f))
                .text = "FARMWAY";

            var panel = UiBuilder.CreatePanel(canvas.transform, "Buttons", new Vector2(360f, 220f), new Color(0f, 0f, 0f, 0.4f));

            UiBuilder.CreateButton(
                panel.transform, "NewGame",
                new Vector2(0f, -30f), new Vector2(300f, 56f),
                "Новая игра",
                () =>
                {
                    _saveService.LoadRequested = false;
                    _choiceMade = true;
                });

            var continueButton = UiBuilder.CreateButton(
                panel.transform, "Continue",
                new Vector2(0f, -110f), new Vector2(300f, 56f),
                "Продолжить",
                () =>
                {
                    _saveService.LoadRequested = true;
                    _choiceMade = true;
                });

            continueButton.interactable = _saveService.HasSave;

            if (!_saveService.HasSave)
                continueButton.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 0.35f);

            while (!_choiceMade)
                await Awaitable.NextFrameAsync(ct);

            Object.Destroy(canvas.gameObject);
        }
    }
}

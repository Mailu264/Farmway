using System;
using DG.Tweening;
using Farmway.Gameplay.Services;
using Farmway.Gameplay.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace Farmway.Gameplay.Player
{
    public class HudPresenter : IInitializable, IDisposable
    {
        private readonly IGameTimeService _gameTimeService;
        private readonly MoneyModel _moneyModel;
        private readonly FatigueModel _fatigueModel;
        private readonly PlayerSleepService _sleepService;
        private readonly PlayerShopService _shopService;
        private readonly CompositeDisposable _disposables = new();

        private Text _timeText;
        private Text _moneyText;
        private Text _promptText;
        private Text _sleepText;
        private Image _nightOverlay;
        private RectTransform _fatigueFill;
        private Image _fatigueFillImage;
        private CanvasGroup _sleepFade;
        private Canvas _canvas;
        private Sequence _sleepSequence;

        private const float FatigueBarWidth = 246f;

        private bool _isNight;
        private bool _isNearHouse;
        private bool _isNearTrader;
        private bool _isExhausted;

        public HudPresenter(
            IGameTimeService gameTimeService,
            MoneyModel moneyModel,
            FatigueModel fatigueModel,
            PlayerSleepService sleepService,
            PlayerShopService shopService)
        {
            _gameTimeService = gameTimeService;
            _moneyModel = moneyModel;
            _fatigueModel = fatigueModel;
            _sleepService = sleepService;
            _shopService = shopService;
        }

        public void Initialize()
        {
            BuildHud();
            Subscribe();
        }

        public void Dispose()
        {
            _sleepSequence?.Kill();
            _disposables.Dispose();

            if (_canvas != null)
                UnityEngine.Object.Destroy(_canvas.gameObject);
        }

        private void BuildHud()
        {
            _canvas = UiBuilder.CreateCanvas("RuntimeHud", 100);

            _nightOverlay = UiBuilder.CreateOverlay(_canvas.transform, "NightOverlay", new Color(0.05f, 0.05f, 0.2f, 0.45f));
            _nightOverlay.gameObject.SetActive(false);

            _timeText = UiBuilder.CreateLabel(
                _canvas.transform, "Time",
                new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(250f, 44f),
                23, Color.white);

            _moneyText = UiBuilder.CreateLabel(
                _canvas.transform, "Money",
                new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(190f, 44f),
                23, new Color(1f, 0.9f, 0.4f));

            BuildGuide();

            _promptText = UiBuilder.CreateLabel(
                _canvas.transform, "Prompt",
                new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(640f, 44f),
                22, Color.white);
            _promptText.transform.parent.gameObject.SetActive(false);

            BuildFatigueBar();
            BuildSleepFade();
        }

        private void BuildFatigueBar()
        {
            var bg = UiBuilder.CreatePanel(_canvas.transform, "FatigueBar", new Vector2(FatigueBarWidth + 4f, 14f), new Color(0f, 0f, 0f, 0.6f));
            var bgRect = (RectTransform)bg.transform;
            bgRect.anchorMin = new Vector2(0f, 1f);
            bgRect.anchorMax = new Vector2(0f, 1f);
            bgRect.pivot = new Vector2(0f, 1f);
            bgRect.anchoredPosition = new Vector2(16f, -64f);
            bg.raycastTarget = false;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(bg.transform, false);
            _fatigueFill = fillGo.AddComponent<RectTransform>();
            _fatigueFill.anchorMin = new Vector2(0f, 0.5f);
            _fatigueFill.anchorMax = new Vector2(0f, 0.5f);
            _fatigueFill.pivot = new Vector2(0f, 0.5f);
            _fatigueFill.anchoredPosition = new Vector2(2f, 0f);
            _fatigueFill.sizeDelta = new Vector2(0f, 10f);

            _fatigueFillImage = fillGo.AddComponent<Image>();
            _fatigueFillImage.raycastTarget = false;
        }

        // Окно-гайд: показывается при старте, повторно — по кнопке «?»
        private void BuildGuide()
        {
            var guidePanel = UiBuilder.CreatePanel(_canvas.transform, "GuidePanel", new Vector2(620f, 460f), new Color(0.1f, 0.1f, 0.14f, 0.97f));

            UiBuilder.CreateText(
                guidePanel.transform, "Title",
                new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(560f, 40f),
                28, TextAnchor.UpperCenter, new Color(0.95f, 0.85f, 0.5f))
                .text = "Как играть";

            UiBuilder.CreateText(
                guidePanel.transform, "Body",
                new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(540f, 300f),
                19, TextAnchor.UpperLeft, Color.white)
                .text =
                "1. Выбери инструмент в хотбаре (1-5 или колесо мыши)\n" +
                "2. Мотыгой вскопай землю (ЛКМ по клетке)\n" +
                "3. Посади семена в грядку\n" +
                "4. Полей лейкой — без воды не растёт\n" +
                "5. Иди к дому и поспи [E] — растения растут за ночь\n" +
                "6. Поливай каждый день, пока не созреет\n" +
                "7. Созревшее (пульсирует) срежь косой\n" +
                "8. Продай урожай торговцу [E] и купи новые семена\n\n" +
                "Следи за усталостью (полоска слева сверху):\n" +
                "устал — работать не сможешь, иди спать.";

            UiBuilder.CreateButton(
                guidePanel.transform, "Close",
                new Vector2(0f, -395f), new Vector2(200f, 44f),
                "Понятно",
                () => guidePanel.gameObject.SetActive(false));

            UiBuilder.CreateButton(
                _canvas.transform, "GuideOpen",
                new Vector2(0f, -16f), new Vector2(44f, 44f),
                "?",
                () => guidePanel.gameObject.SetActive(true));
        }

        private void BuildSleepFade()
        {
            var fade = UiBuilder.CreateOverlay(_canvas.transform, "SleepFade", Color.black);
            _sleepFade = fade.gameObject.AddComponent<CanvasGroup>();
            _sleepFade.alpha = 0f;
            _sleepFade.blocksRaycasts = false;

            _sleepText = UiBuilder.CreateText(
                fade.transform, "DayLabel",
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 60f),
                42, TextAnchor.MiddleCenter, Color.white);
        }

        private void Subscribe()
        {
            var time = _gameTimeService.GameTime;

            time.Day.CombineLatest(time.Hours, time.Minutes, (d, h, m) => (d, h, m))
                .Subscribe(t => UpdateTimeText(t.d, t.h, t.m))
                .AddTo(_disposables);

            time.DayPhase
                .Subscribe(OnDayPhaseChanged)
                .AddTo(_disposables);

            _moneyModel.Amount
                .Subscribe(amount => _moneyText.text = $"Деньги: {amount}")
                .AddTo(_disposables);

            _fatigueModel.Value
                .Subscribe(OnFatigueChanged)
                .AddTo(_disposables);

            _sleepService.IsNearHouse
                .Subscribe(isNear =>
                {
                    _isNearHouse = isNear;
                    UpdatePrompt();
                })
                .AddTo(_disposables);

            _shopService.IsNearTrader
                .Subscribe(isNear =>
                {
                    _isNearTrader = isNear;
                    UpdatePrompt();
                })
                .AddTo(_disposables);

            _sleepService.OnSleepRequested
                .Subscribe(_ => PlaySleepTransition())
                .AddTo(_disposables);
        }

        private void OnFatigueChanged(float fatigue)
        {
            _fatigueFill.sizeDelta = new Vector2(FatigueBarWidth * fatigue, 10f);
            _fatigueFillImage.color = Color.Lerp(new Color(0.4f, 0.85f, 0.35f), new Color(0.9f, 0.25f, 0.2f), fatigue);

            bool exhausted = fatigue >= 1f;
            if (exhausted != _isExhausted)
            {
                _isExhausted = exhausted;
                UpdatePrompt();
            }
        }

        // Затемнение → смена дня под чёрным экраном → надпись "День N" → рассвет
        private void PlaySleepTransition()
        {
            _sleepSequence?.Kill();
            _sleepText.text = string.Empty;
            _sleepFade.blocksRaycasts = true;

            _sleepSequence = DOTween.Sequence()
                .Append(_sleepFade.DOFade(1f, 0.6f).SetEase(Ease.InQuad))
                .AppendCallback(() =>
                {
                    _sleepService.CompleteSleep();
                    _sleepText.text = $"День {_gameTimeService.GameTime.Day.Value}";
                })
                .AppendInterval(1.1f)
                .Append(_sleepFade.DOFade(0f, 0.8f).SetEase(Ease.OutQuad))
                .OnComplete(() =>
                {
                    _sleepFade.blocksRaycasts = false;
                    _sleepService.FinishSleep();
                });
        }

        private void UpdateTimeText(int day, int hours, int minutes) =>
            _timeText.text = _isNight ? $"День {day} — Ночь" : $"День {day}   {hours:00}:{minutes:00}";

        private void OnDayPhaseChanged(DayPhase phase)
        {
            _isNight = phase == DayPhase.Night;
            _nightOverlay.gameObject.SetActive(_isNight);

            var time = _gameTimeService.GameTime;
            UpdateTimeText(time.Day.Value, time.Hours.Value, time.Minutes.Value);
            UpdatePrompt();
        }

        private void UpdatePrompt()
        {
            string prompt;

            if (_isNearHouse)
                prompt = "[E] — Спать (начать новый день)";
            else if (_isNearTrader)
                prompt = "[E] — Магазин";
            else if (_isNight || _isExhausted)
                prompt = "Вы устали. Идите к дому и нажмите [E]";
            else
                prompt = string.Empty;

            _promptText.text = prompt;
            _promptText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
        }
    }
}

using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class PlantAnimationBehaviour : PlantBehaviour
    {
        private readonly PlantView _view;
        private readonly PlantData _data;
        private readonly CompositeDisposable _disposables = new();

        private Tween _readyPulse;

        public PlantAnimationBehaviour(PlantView view, PlantData data)
        {
            _view = view;
            _data = data;
        }

        public override void OnPlanted()
        {
            // Появление: лёгкий "вырост" из земли
            _view.transform.localScale = Vector3.zero;
            _view.transform.DOScale(_view.BaseScale, 0.25f).SetEase(Ease.OutBack).SetLink(_view.gameObject);

            Plant.Stage
                .Subscribe(UpdateVisual)
                .AddTo(_disposables);

            Plant.IsReadyToHarvest
                .Subscribe(OnReadyChanged)
                .AddTo(_disposables);
        }

        public override void Dispose()
        {
            _readyPulse?.Kill();
            _disposables.Dispose();

            if (_view != null)
                UnityEngine.Object.Destroy(_view.gameObject);
        }

        // Зрелое растение мягко пульсирует — видно что можно собирать
        private void OnReadyChanged(bool isReady)
        {
            if (!isReady)
                return;

            _readyPulse?.Kill();
            _readyPulse = _view.transform
                .DOScale(_view.BaseScale * 1.1f, 0.6f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(_view.gameObject);
        }

        private void UpdateVisual(int stage)
        {
            var sprite = GetStageSprite(stage);

            if (sprite != null)
            {
                _view.SetSprite(sprite);
                return;
            }

            // Спрайтов в конфиге нет — плейсхолдер: зелёный квадрат, растущий по стадиям
            int stagesCount = _data.Stages is { Length: > 0 } ? _data.Stages.Length : 1;
            float progress = stagesCount <= 1 ? 1f : (float)stage / (stagesCount - 1);
            _view.SetSprite(SpriteLibrary.Plant);
            _view.SetScaleMultiplier(Mathf.Lerp(0.35f, 1f, progress));
        }

        private Sprite GetStageSprite(int stage)
        {
            if (_data.Stages == null || _data.Stages.Length == 0)
                return null;

            int index = Mathf.Clamp(stage, 0, _data.Stages.Length - 1);
            return _data.Stages[index].Sprite;
        }
    }
}

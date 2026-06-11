using DG.Tweening;
using Farmway.Gameplay.Farm;
using Farmway.Infrastructure;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player.Services.ItemsService
{
    public class PlayerItemsService : PlayerService
    {
        private const float HandOffsetX = 0.45f;
        private const float HandOffsetY = -0.35f;

        private readonly IHotbarSlotsModel _hotbarSlotsModel;
        private readonly IInputService _inputService;

        private readonly SerialDisposable _serialDisposable = new();

        private SpriteRenderer _handRenderer;
        private Transform _handTransform;
        private Tween _useTween;
        private bool _facingRight = true;

        public PlayerItemsService(IHotbarSlotsModel hotbarSlotsModel, IInputService inputService)
        {
            _hotbarSlotsModel = hotbarSlotsModel;
            _inputService = inputService;
        }

        public override void OnInitialize()
        {
            CreateHand();

            _serialDisposable.Disposable = _hotbarSlotsModel.CurrentItem.Subscribe(slot =>
            {
                var icon = slot.IsEmpty ? null : slot.Item.Icon;

                if (PlayerView.SpawnItemPoint != null)
                    PlayerView.SpawnItemPoint.SetItem(icon);

                if (_handRenderer != null)
                    _handRenderer.sprite = icon;
            });
        }

        public override void OnUpdate()
        {
            if (_handTransform == null)
                return;

            float moveX = _inputService.MovementVector.x;

            if (Mathf.Abs(moveX) > 0.01f)
                _facingRight = moveX > 0;

            var localPos = _handTransform.localPosition;
            localPos.x = HandOffsetX * (_facingRight ? 1f : -1f);
            _handTransform.localPosition = localPos;

            if (_handRenderer != null)
                _handRenderer.flipX = !_facingRight;
        }

        // Взмах предметом при использовании
        public void PlayUseAnimation()
        {
            if (_handTransform == null)
                return;

            float direction = _facingRight ? -1f : 1f;

            _useTween?.Kill(true);
            _useTween = _handTransform
                .DOPunchRotation(new Vector3(0f, 0f, direction * 45f), 0.3f, vibrato: 1, elasticity: 0.4f)
                .SetLink(_handTransform.gameObject);
        }

        public override void OnDispose()
        {
            _useTween?.Kill();
            _serialDisposable.Dispose();
        }

        private void CreateHand()
        {
            // Если на префабе уже есть точка предмета — используем её
            if (PlayerView.SpawnItemPoint != null)
            {
                _handTransform = PlayerView.SpawnItemPoint.transform;
                return;
            }

            var hand = new GameObject("ItemInHand");
            hand.transform.SetParent(PlayerView.transform);
            hand.transform.localPosition = new Vector3(HandOffsetX, HandOffsetY, 0f);

            _handRenderer = hand.AddComponent<SpriteRenderer>();
            _handRenderer.sortingOrder = FarmSortingOrder.HandItem;
            _handTransform = hand.transform;
        }
    }
}

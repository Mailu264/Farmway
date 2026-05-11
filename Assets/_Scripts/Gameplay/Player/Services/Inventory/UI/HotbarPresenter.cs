using System;
using Farmway.Infrastructure;
using UniRx;
using VContainer.Unity;

namespace Farmway.Gameplay.Player
{
    public class HotbarPresenter : IInitializable, IDisposable
    {
        private readonly IHotbarSlotsModel _hotbarSlotsModel;
        private readonly IInputService _inputService;
        private readonly HotbarView _hotbarView;
        private readonly CompositeDisposable _disposables = new();
        private int _selectedIndex = -1;

        public HotbarPresenter(
            IHotbarSlotsModel hotbarSlotsModel,
            IInputService inputService,
            GameplaySceneView gameplaySceneView)
        {
            _hotbarSlotsModel = hotbarSlotsModel;
            _inputService = inputService;
            _hotbarView = gameplaySceneView.HotbarView;
        }

        public void Initialize()
        {
            _hotbarSlotsModel.CurrentSlotIndex
                .DistinctUntilChanged()
                .Subscribe(OnSelectedIndexChanged)
                .AddTo(_disposables);

            _inputService.OnHotbarSlotSelected
                .Subscribe(_hotbarSlotsModel.SelectSlot)
                .AddTo(_disposables);

            _inputService.OnHotbarScrolled
                .Subscribe(OnScrolled)
                .AddTo(_disposables);
        }

        private void OnScrolled(int direction)
        {
            if (direction > 0)
                _hotbarSlotsModel.SelectPrevious();
            else if (direction < 0)
                _hotbarSlotsModel.SelectNext();
        }

        private void OnSelectedIndexChanged(int index)
        {
            if (_selectedIndex >= 0)
                _hotbarView.SetSlotSelected(_selectedIndex, false);

            _selectedIndex = index;

            if (_selectedIndex >= 0)
                _hotbarView.SetSlotSelected(_selectedIndex, true);
        }

        public void Dispose() =>
            _disposables.Dispose();
    }
}

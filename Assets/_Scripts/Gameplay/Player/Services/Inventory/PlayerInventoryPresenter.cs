using System;
using Farmway.Infrastructure;
using UniRx;
using VContainer.Unity;

namespace Farmway.Gameplay.Player
{
    public class PlayerInventoryPresenter : IInitializable, IDisposable
    {
        private readonly IInputService _inputService;
        private readonly InventoryView _playerInventoryView;
        
        private IDisposable _inputSubscription;

        public PlayerInventoryPresenter(GameplaySceneView gameplaySceneView, IInputService inputService)
        {
            _inputService = inputService;
            _playerInventoryView = gameplaySceneView.InventoryView;
        }

        public void Initialize()
        {
            _inputSubscription = _inputService.OnInventoryOpened
                .Subscribe(isOpened =>
                {
                    if(!isOpened)
                        return;
                    
                    if (_playerInventoryView.IsOpen)
                        _playerInventoryView.Hide();
                    else
                        _playerInventoryView.Show();
                });
        }

        public void Dispose()
        {
            _inputSubscription?.Dispose();
        }
    }
}
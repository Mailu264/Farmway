using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using VContainer.Unity;

namespace Farmway.Infrastructure
{
    public class InputService : IInputService, IInitializable, ITickable, IDisposable
    {
        private readonly InputSystem_Actions _input;
        
        private readonly ReactiveProperty<bool> _isInventoryOpened = new();

        public Vector2 MovementVector { get; private set; }
        public bool IsSprint { get; private set; }

        public IObservable<bool> OnInventoryOpened => _isInventoryOpened;

        public InputService()
        {
            _input = new InputSystem_Actions();
            _input.Enable();
        }

        public void Initialize()
        {
            _input.Player.Sprint.performed += OnSprintPerformed;
            _input.Player.Sprint.canceled += OnSprintCanceled;
            
            _input.UI.OpenInventory.performed += OnOpenInventoryPerformed;
            _input.UI.OpenInventory.canceled += OnOpenInventoryCanceled;
        }

        private void OnSprintPerformed(InputAction.CallbackContext obj) => 
            IsSprint = true;
        
        private void OnSprintCanceled(InputAction.CallbackContext obj) => 
            IsSprint = false;
        
        private void OnOpenInventoryPerformed(InputAction.CallbackContext obj) =>
            _isInventoryOpened.Value = true;
        
        private void OnOpenInventoryCanceled(InputAction.CallbackContext obj) =>
            _isInventoryOpened.Value = false;

        public void Tick()
        {
            MovementVector = _input.Player.Move.ReadValue<Vector2>();
        }

        public void Dispose()
        {
            _input.Player.Sprint.performed -= OnSprintPerformed;
            _input.Player.Sprint.canceled -= OnOpenInventoryCanceled;
            
            _input.UI.OpenInventory.performed -= OnOpenInventoryPerformed;
            _input.UI.OpenInventory.canceled -= OnOpenInventoryCanceled;
            
            _input?.Dispose();
        }
    }

    public interface IInputService
    {
        Vector2 MovementVector { get; }
        bool IsSprint { get; }

        IObservable<bool> OnInventoryOpened { get; }
    }
}
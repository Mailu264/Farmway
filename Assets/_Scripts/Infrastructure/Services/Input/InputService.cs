using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Farmway.Infrastructure
{
    public class InputService : IInputService, IInitializable, ITickable, IDisposable
    {
        private const int HotbarKeyCount = 9;

        private readonly InputSystem_Actions _input;
        private readonly List<InputAction> _hotbarActions = new(HotbarKeyCount);

        private readonly ReactiveProperty<bool> _isInventoryOpened = new();
        private readonly Subject<int> _hotbarSlotSelected = new();
        private readonly Subject<int> _hotbarScrolled = new();
        private readonly Subject<Vector2> _interact = new();
        private readonly Subject<Unit> _usePressed = new();

        public Vector2 MovementVector { get; private set; }
        public bool IsSprint { get; private set; }

        public IObservable<bool> OnInventoryOpened => _isInventoryOpened;
        public IObservable<int> OnHotbarSlotSelected => _hotbarSlotSelected;
        public IObservable<int> OnHotbarScrolled => _hotbarScrolled;
        public IObservable<Vector2> OnInteract => _interact;
        public IObservable<Unit> OnUsePressed => _usePressed;

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

            RegisterHotbarActions();
        }

        public void Tick()
        {
            MovementVector = _input.Player.Move.ReadValue<Vector2>();

            float scroll = _input.UI.ScrollWheel.ReadValue<Vector2>().y;
            if (scroll > 0f) _hotbarScrolled.OnNext(1);
            else if (scroll < 0f) _hotbarScrolled.OnNext(-1);

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                _interact.OnNext(Mouse.current.position.ReadValue());

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                _usePressed.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _input.Player.Sprint.performed -= OnSprintPerformed;
            _input.Player.Sprint.canceled -= OnSprintCanceled;

            _input.UI.OpenInventory.performed -= OnOpenInventoryPerformed;
            _input.UI.OpenInventory.canceled -= OnOpenInventoryCanceled;

            foreach (InputAction action in _hotbarActions)
            {
                action.Disable();
                action.Dispose();
            }

            _hotbarActions.Clear();
            _isInventoryOpened.Dispose();
            _hotbarSlotSelected.Dispose();
            _hotbarScrolled.Dispose();
            _interact.Dispose();
            _usePressed.Dispose();
            _input?.Dispose();
        }

        private void RegisterHotbarActions()
        {
            for (int i = 0; i < HotbarKeyCount; i++)
            {
                int slotIndex = i;

                InputAction slotAction = new(
                    name: $"HotbarSlot{i}",
                    type: InputActionType.Button,
                    binding: $"<Keyboard>/{i + 1}");

                slotAction.performed += _ => _hotbarSlotSelected.OnNext(slotIndex);
                slotAction.Enable();
                _hotbarActions.Add(slotAction);
            }
        }

        private void OnSprintPerformed(InputAction.CallbackContext ctx) => IsSprint = true;
        private void OnSprintCanceled(InputAction.CallbackContext ctx) => IsSprint = false;

        private void OnOpenInventoryPerformed(InputAction.CallbackContext ctx) => _isInventoryOpened.Value = true;
        private void OnOpenInventoryCanceled(InputAction.CallbackContext ctx) => _isInventoryOpened.Value = false;

    }

    public interface IInputService
    {
        Vector2 MovementVector { get; }
        bool IsSprint { get; }

        IObservable<bool> OnInventoryOpened { get; }
        IObservable<int> OnHotbarSlotSelected { get; }
        IObservable<int> OnHotbarScrolled { get; }
        IObservable<Vector2> OnInteract { get; }
        IObservable<Unit> OnUsePressed { get; }
    }
}

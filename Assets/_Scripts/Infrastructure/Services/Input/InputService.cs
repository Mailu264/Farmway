using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Farmway.Infrastructure
{
    public class InputService : IInputService, IInitializable, ITickable, IDisposable
    {
        private readonly InputSystem_Actions _input;

        public Vector2 MovementVector { get; private set; }
        public bool IsSprint { get; private set; }

        public InputService()
        {
            _input = new InputSystem_Actions();
            _input.Enable();
        }

        public void Initialize()
        {
            _input.Player.Sprint.performed += OnSprintPerformed;
        }

        private void OnSprintPerformed(InputAction.CallbackContext obj) => 
            IsSprint = obj.ReadValueAsButton();

        public void Tick()
        {
            MovementVector = _input.Player.Move.ReadValue<Vector2>();
        }

        public void Dispose()
        {
            _input.Player.Sprint.performed -= OnSprintPerformed;
            _input?.Dispose();
        }
    }

    public interface IInputService
    {
        Vector2 MovementVector { get; }
        bool IsSprint { get; }
    }
}
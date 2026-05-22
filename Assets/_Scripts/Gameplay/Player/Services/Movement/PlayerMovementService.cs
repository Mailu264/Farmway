using Farmway.Infrastructure;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public class PlayerMovementService : PlayerService
    {
        private readonly IInputService _inputService;

        public PlayerMovementService(IInputService inputService)
        {
            _inputService = inputService;
        }

        public override void OnFixedUpdate() => 
            Move(_inputService.MovementVector);

        private void Move(Vector2 movementVector) => 
            PlayerView.Rigidbody2D.linearVelocity = movementVector * PlayerConfig.Speed;
    }
}
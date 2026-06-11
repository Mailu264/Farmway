using Farmway.Infrastructure;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public class PlayerAnimationService : PlayerService
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");

        private readonly IInputService _inputService;
        private Vector2 _lastDirection = Vector2.down;

        public PlayerAnimationService(IInputService inputService)
        {
            _inputService = inputService;
        }

        public override void OnUpdate()
        {
            if (PlayerView.Animator == null)
                return;

            var move = _inputService.MovementVector;

            if (move != Vector2.zero)
                _lastDirection = GetPriorityDirection(move);

            PlayerView.Animator.SetFloat(Speed, move.magnitude);
            PlayerView.Animator.SetFloat(MoveX, _lastDirection.x);
            PlayerView.Animator.SetFloat(MoveY, _lastDirection.y);
        }

        private static Vector2 GetPriorityDirection(Vector2 move)
        {
            if (Mathf.Abs(move.x) >= Mathf.Abs(move.y))
                return move.x > 0 ? Vector2.right : Vector2.left;

            return move.y > 0 ? Vector2.up : Vector2.down;
        }
    }
}

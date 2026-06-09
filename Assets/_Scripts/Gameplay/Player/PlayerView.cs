using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public class PlayerView : MonoBehaviour
    {
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        [field: SerializeField] public Rigidbody2D Rigidbody2D { get; private set; }
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public PlayerItemView ItemView { get; private set; }
    }
}
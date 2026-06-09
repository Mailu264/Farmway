using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public class PlayerItemView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void SetItem(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
            _spriteRenderer.enabled = sprite != null;
        }
    }
}
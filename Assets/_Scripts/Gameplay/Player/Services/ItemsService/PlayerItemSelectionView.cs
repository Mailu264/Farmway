using UnityEngine;

namespace Farmway.Gameplay.Player.Services.ItemsService
{
    public class PlayerItemSelectionView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void SetItem(Sprite itemSprite)
        {
            if (itemSprite == null)
            {
                _spriteRenderer.sprite = null;
                return;
            }
            
            _spriteRenderer.sprite = itemSprite;
        }
    }
}
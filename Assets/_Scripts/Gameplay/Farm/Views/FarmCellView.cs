using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class FarmCellView : MonoBehaviour
    {
        private static readonly Color WateredTint = new(0.6f, 0.48f, 0.4f);

        private SpriteRenderer _groundRenderer;

        public void Construct(SpriteRenderer groundRenderer) =>
            _groundRenderer = groundRenderer;

        public void UpdateView(FarmCell cell, Sprite tilledSprite, Sprite wateredSprite)
        {
            if (cell.IsWatered && wateredSprite != null)
            {
                _groundRenderer.sprite = wateredSprite;
                _groundRenderer.color = Color.white;
                return;
            }

            _groundRenderer.sprite = tilledSprite != null ? tilledSprite : SpriteLibrary.Tilled;
            _groundRenderer.color = cell.IsWatered ? WateredTint : Color.white;
        }
    }
}

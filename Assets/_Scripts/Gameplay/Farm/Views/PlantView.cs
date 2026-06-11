using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class PlantView : MonoBehaviour
    {
        private SpriteRenderer _renderer;

        public float BaseScale { get; private set; } = 1f;

        public void Construct(SpriteRenderer renderer, float baseScale)
        {
            _renderer = renderer;
            BaseScale = baseScale;
        }

        public void SetSprite(Sprite sprite) =>
            _renderer.sprite = sprite;

        public void SetTint(Color color) =>
            _renderer.color = color;

        // Множитель относительно базового размера клетки
        public void SetScaleMultiplier(float multiplier) =>
            transform.localScale = Vector3.one * (BaseScale * multiplier);
    }
}

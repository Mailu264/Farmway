using System.Collections.Generic;
using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    // Рантайм-генерация простых спрайтов, чтобы игра работала без арта
    public static class SpriteLibrary
    {
        private const int Size = 16;
        private const float PixelsPerUnit = 16f;

        private static readonly Dictionary<Color, Sprite> Cache = new();

        public static Sprite Tilled => Get(new Color(0.55f, 0.36f, 0.2f));
        public static Sprite Watered => Get(new Color(0.36f, 0.23f, 0.12f));
        public static Sprite Plant => Get(new Color(0.3f, 0.75f, 0.3f));
        public static Sprite House => Get(new Color(0.8f, 0.3f, 0.25f));

        public static Sprite Get(Color color)
        {
            if (Cache.TryGetValue(color, out var cached))
                return cached;

            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            var pixels = new Color[Size * Size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            texture.SetPixels(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), PixelsPerUnit);
            Cache[color] = sprite;
            return sprite;
        }
    }
}

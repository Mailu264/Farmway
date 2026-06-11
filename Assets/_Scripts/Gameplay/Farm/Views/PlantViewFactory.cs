using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class PlantViewFactory
    {
        private readonly Grid _grid;
        private readonly Transform _parent;

        public PlantViewFactory(Grid grid, Transform parent)
        {
            _grid = grid;
            _parent = parent;
        }

        public PlantView Create(Vector2Int pos)
        {
            float cellSize = _grid.cellSize.x;

            // Чуть выше центра клетки: спрайты растений «сидят» в нижней части тайла
            var worldPos = _grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0)) + new Vector3(0f, 0.15f * cellSize, 0f);

            var go = new GameObject($"Plant ({pos.x},{pos.y})");
            go.transform.SetParent(_parent);
            go.transform.position = worldPos;
            go.transform.localScale = Vector3.one * cellSize;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = FarmSortingOrder.Plant;

            var view = go.AddComponent<PlantView>();
            view.Construct(renderer, cellSize);
            return view;
        }
    }
}

using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class FarmCellViewFactory
    {
        private readonly Grid _grid;
        private readonly Transform _parent;

        public FarmCellViewFactory(Grid grid, Transform parent)
        {
            _grid = grid;
            _parent = parent;
        }

        public FarmCellView Create(Vector2Int pos)
        {
            var worldPos = _grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));

            var go = new GameObject($"Cell ({pos.x},{pos.y})");
            go.transform.SetParent(_parent);
            go.transform.position = worldPos;
            go.transform.localScale = Vector3.one * _grid.cellSize.x; // спрайт 1 юнит → размер клетки

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteLibrary.Tilled;
            renderer.sortingOrder = FarmSortingOrder.Cell;

            var view = go.AddComponent<FarmCellView>();
            view.Construct(renderer);
            return view;
        }
    }
}

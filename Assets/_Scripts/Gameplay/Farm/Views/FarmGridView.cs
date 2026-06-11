using System.Collections.Generic;
using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class FarmGridView : MonoBehaviour
    {
        private readonly Dictionary<Vector2Int, FarmCellView> _cellViews = new();

        public Grid Grid { get; private set; }

        public void Construct(Grid grid) =>
            Grid = grid;

        public bool TryGetCellView(Vector2Int pos, out FarmCellView view) =>
            _cellViews.TryGetValue(pos, out view);

        public void AddCellView(Vector2Int pos, FarmCellView view) =>
            _cellViews[pos] = view;

        public void RemoveCellView(Vector2Int pos)
        {
            if (!_cellViews.TryGetValue(pos, out var view)) return;
            Destroy(view.gameObject);
            _cellViews.Remove(pos);
        }
    }
}

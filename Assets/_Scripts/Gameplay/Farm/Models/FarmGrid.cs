using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class FarmGrid
    {
        private readonly Dictionary<Vector2Int, FarmCell> _cells = new();
        private readonly Subject<Vector2Int> _onCellChanged = new();

        public IObservable<Vector2Int> OnCellChanged => _onCellChanged;

        public bool TryGetCell(Vector2Int pos, out FarmCell cell) =>
            _cells.TryGetValue(pos, out cell);

        public IEnumerable<KeyValuePair<Vector2Int, FarmCell>> GetAllCells() => _cells;

        public bool TrySetCell(Vector2Int pos, FarmCell cell)
        {
            _cells[pos] = cell;
            _onCellChanged.OnNext(pos);
            return true;
        }

        public bool TryRemoveCell(Vector2Int pos)
        {
            if (_cells.Remove(pos))
            {
                _onCellChanged.OnNext(pos);
                return true;
            }

            Debug.LogWarning($"[FarmGrid] Cannot remove cell at {pos} — not found");
            return false;
        }
    }
}

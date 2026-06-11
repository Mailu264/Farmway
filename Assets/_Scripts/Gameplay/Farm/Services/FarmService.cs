using System.Collections.Generic;
using Farmway.Gameplay.Services;
using UnityEngine;

namespace Farmway.Gameplay.Farm
{
    public class FarmService : GameServiceBase
    {
        private readonly FarmGrid _farmGrid;

        public FarmService(FarmGrid farmGrid) => _farmGrid = farmGrid;

        public bool TillCell(Vector2Int pos)
        {
            if (_farmGrid.TryGetCell(pos, out var cell) && !cell.IsEmpty)
            {
                Debug.LogWarning($"[FarmService] Cannot till cell at {pos} — not empty");
                return false;
            }

            return _farmGrid.TrySetCell(pos, new FarmCell { State = FarmCellState.Tilled });
        }

        public bool WaterCell(Vector2Int pos)
        {
            if (!_farmGrid.TryGetCell(pos, out var cell) || !cell.IsTilled)
            {
                Debug.LogWarning($"[FarmService] Cannot water cell at {pos} — not tilled");
                return false;
            }

            if (cell.IsWatered)
            {
                Debug.LogWarning($"[FarmService] Cell at {pos} already watered");
                return false;
            }

            cell.IsWatered = true;
            return _farmGrid.TrySetCell(pos, cell);
        }

        public void ResetDailyWatering()
        {
            var positions = new List<Vector2Int>();
            foreach (var (pos, _) in _farmGrid.GetAllCells())
                positions.Add(pos);

            foreach (var pos in positions)
            {
                if (!_farmGrid.TryGetCell(pos, out var cell) || !cell.IsWatered) continue;

                cell.IsWatered = false;
                _farmGrid.TrySetCell(pos, cell);
            }
        }
    }
}

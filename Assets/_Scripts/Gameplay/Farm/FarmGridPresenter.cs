using System;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Farmway.Gameplay.Farm
{
    // Управляет только видом земли. Вьюшки растений живут внутри самих растений.
    public class FarmGridPresenter : IInitializable, IDisposable
    {
        private readonly FarmGrid _farmGrid;
        private readonly FarmGridView _farmGridView;
        private readonly FarmCellViewFactory _cellViewFactory;
        private readonly FarmConfig _farmConfig;
        private readonly CompositeDisposable _disposables = new();

        public FarmGridPresenter(
            FarmGrid farmGrid,
            FarmGridView farmGridView,
            FarmCellViewFactory cellViewFactory,
            Farmway.Infrastructure.IConfigProvider configProvider)
        {
            _farmGrid = farmGrid;
            _farmGridView = farmGridView;
            _cellViewFactory = cellViewFactory;
            _farmConfig = configProvider.GetConfig<FarmConfig>();
        }

        public void Initialize()
        {
            _farmGrid.OnCellChanged
                .Subscribe(OnCellChanged)
                .AddTo(_disposables);
        }

        private void OnCellChanged(Vector2Int pos)
        {
            _farmGrid.TryGetCell(pos, out var cell);

            if (cell.IsEmpty)
            {
                _farmGridView.RemoveCellView(pos);
                return;
            }

            if (!_farmGridView.TryGetCellView(pos, out var view))
            {
                view = _cellViewFactory.Create(pos);
                _farmGridView.AddCellView(pos, view);
            }

            view.UpdateView(cell, _farmConfig.TilledSprite, _farmConfig.WateredSprite);
        }

        public void Dispose() => _disposables.Dispose();
    }
}

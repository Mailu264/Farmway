namespace Farmway.Gameplay.Farm
{
    public struct FarmCell
    {
        public static FarmCell Empty => new() { State = FarmCellState.Empty };

        public FarmCellState State;
        public bool IsWatered;
        public Plant Plant;

        public bool IsEmpty => State == FarmCellState.Empty;
        public bool IsTilled => State == FarmCellState.Tilled;
        public bool HasPlant => Plant != null;
    }
}

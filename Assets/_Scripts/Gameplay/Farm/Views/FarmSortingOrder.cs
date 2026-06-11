namespace Farmway.Gameplay.Farm
{
    // Единая раскладка сортинга: фон-тайлмапы сцены ≤ 0, ферма выше фона, игрок выше фермы
    public static class FarmSortingOrder
    {
        public const int Cell = 2;
        public const int Plant = 3;
        public const int House = 4;
        public const int Player = 10;
        public const int HandItem = 11;
    }
}

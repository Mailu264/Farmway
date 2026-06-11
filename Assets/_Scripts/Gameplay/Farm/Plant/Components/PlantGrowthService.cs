namespace Farmway.Gameplay.Farm
{
    public class PlantGrowthService : PlantService
    {
        private readonly PlantData _data;

        public PlantGrowthService(PlantData data) => _data = data;

        // Вызывается при скипе ночи: растение переходит в следующую фазу
        public void AdvanceDay()
        {
            if (Plant.IsReadyToHarvest.Value)
                return;

            int stagesCount = _data.Stages is { Length: > 0 } ? _data.Stages.Length : 1;
            int nextStage = UnityEngine.Mathf.Min(Plant.Stage.Value + 1, stagesCount - 1);

            Plant.SetStage(nextStage);

            if (nextStage >= stagesCount - 1)
                Plant.SetReadyToHarvest();
        }
    }
}

using Farmway.Gameplay.Services;

namespace Farmway.Gameplay.Player
{
    public class PlayerFatigueService : PlayerService
    {
        private readonly FatigueModel _fatigueModel;
        private readonly IGameTimeService _gameTimeService;

        public PlayerFatigueService(FatigueModel fatigueModel, IGameTimeService gameTimeService)
        {
            _fatigueModel = fatigueModel;
            _gameTimeService = gameTimeService;
        }

        public override void OnUpdate() =>
            _fatigueModel.UpdateBase(_gameTimeService.GameTime.Progress.Value);
    }
}

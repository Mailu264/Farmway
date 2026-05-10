namespace Farmway.Gameplay.Player.Services.Camera
{
    public class PlayerCameraFollowService : PlayerService
    {
        private readonly GameplaySceneView _gameplaySceneView;

        public PlayerCameraFollowService(GameplaySceneView gameplaySceneView)
        {
            _gameplaySceneView = gameplaySceneView;
        }
        
        public override void OnEnable() => 
            _gameplaySceneView.CameraView.SetFollowTarget(PlayerView.transform);
    }
}
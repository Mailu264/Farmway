using Farmway.Gameplay.Player.Services.Camera;
using UnityEngine;

namespace Farmway.Gameplay
{
    public class GameplaySceneView : MonoBehaviour
    {
        [field: SerializeField] public CameraView CameraView { get; private set; }
    }
}
using Unity.Cinemachine;
using UnityEngine;

namespace Farmway.Gameplay.Player.Services.Camera
{
    public class CameraView : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        public void SetFollowTarget(Transform target) => 
            _cinemachineCamera.Follow = target;
    }
}
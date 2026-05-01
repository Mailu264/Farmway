using UnityEngine;

namespace Farmway.Infrastructure
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private void Awake() =>
            DontDestroyOnLoad(gameObject);
    }
}

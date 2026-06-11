using UnityEngine;

namespace Farmway.Gameplay.Services.Configs
{
    [CreateAssetMenu(fileName = "TimeConfig", menuName = "Configs/Gameplay/TimeConfig")]
    public class TimeConfig : ScriptableObject
    {
        [field: SerializeField] public float DayTime { get; set; }
        [field: SerializeField, Range(0, 24)] public int HourStartDay { get; set; }
        [field: SerializeField, Range(0, 24)] public int HourEndDay { get; set; }
    }
}
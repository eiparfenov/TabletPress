using UnityEngine;

namespace TabletPress
{
    [CreateAssetMenu(fileName = "RotorSettings", menuName = "TabletPress/RotorSettings", order = 0)]
    public class RotorSettings : ScriptableObject
    {
        [field: SerializeField] public AnimationCurve pistonMotion { get; private set; }
        [field: SerializeField] public float downDistance { get; private set; }
        
        [field: Range(0, 1)][field: SerializeField] public float offset { get; private set; }
    }
}
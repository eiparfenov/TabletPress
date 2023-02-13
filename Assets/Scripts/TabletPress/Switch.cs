using UnityEngine;

namespace TabletPress
{
    public class Switch: MonoBehaviour
    {
        [SerializeField] private float maxAngle;
        [SerializeField] private float minAngle;
        [SerializeField] private float angle;
        
        public bool TurnedOn
        {
            get
            {
                angle = Vector3.Angle(Vector3.up, transform.up);
                return minAngle < angle && angle < maxAngle;
            }
        }
    }
}
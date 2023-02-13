using UnityEngine;

namespace TabletPress
{
    public class RotorSpeedChanger : MonoBehaviour
    {
        public float SpeedMult
        {
            get
            {
                var angle = Vector3.SignedAngle(Vector3.up, transform.up, transform.right);
                return 1f - angle / 180f;
            }
        }
    }
}
using System;
using UnityEngine;
using Utils;

namespace TabletPress
{
    public class ReadyDoor: MonoBehaviour
    {
        private ConfigurableJoint _joint;
        [SerializeField] private Vector3Int localEulerAxis;
        [SerializeField] private float switchAngle;

        private void Start()
        {
            _joint = GetComponent<ConfigurableJoint>();
        }

        private void Update()
        {
            if (AngleTranslator.GetAngle(Vector3.Dot(transform.localEulerAngles, (Vector3) localEulerAxis)) <
                switchAngle)
            {
                _joint.angularXDrive = new JointDrive() {positionSpring = .5f};
            }
            else
            {
                _joint.angularXDrive = new JointDrive() {positionSpring = 0f};
            }
        }
    }
}
using System;
using UnityEngine;
using Utils;

namespace TabletPress
{
    public class Door: MonoBehaviour, IDoor
    {
        [SerializeField] private Handle handle;
        [SerializeField] private Transform door;
        [SerializeField] private ConfigurableJoint joint;

        [Space] 
        [SerializeField] private float openAngle;
        [SerializeField] private float switchAngle;
        [SerializeField] private Vector3Int localEulerAxis;

        private Transform _connectedDoor;

        private void OnEnable()
        {
            handle.onDoorOpenCloseStateChange += HandleOnDoorOpenCloseStateChange;
        }

        private void HandleOnDoorOpenCloseStateChange(bool opened)
        {
            if (opened)
            {
                SetLimits(openAngle, 0f);
                Opened = true;
            }
            else
            {
                print($"Door {name}: {door.localEulerAngles}");
                
                if (AngleTranslator.GetAngle(Vector3.Dot(door.localEulerAngles, (Vector3)localEulerAxis)) <= switchAngle)
                {
                    SetLimits(switchAngle, 0f);
                    Opened = false;
                }
                else
                {
                    SetLimits(openAngle, switchAngle);
                }
            }
        }

        private void OnDisable()
        {
            handle.onDoorOpenCloseStateChange -= HandleOnDoorOpenCloseStateChange;
        }

        private void OnValidate()
        {
            if(handle && door)
                handle.SetDoor(door.GetComponent<Rigidbody>());

            if (door)
                joint = door.GetComponent<ConfigurableJoint>();
        }

        private void SetLimits(float from, float to)
        {
            joint.lowAngularXLimit = new SoftJointLimit() {limit = -from};
            joint.highAngularXLimit = new SoftJointLimit() {limit = -to};
        }

        private void Update()
        {
            if (AngleTranslator.GetAngle(Vector3.Dot(door.localEulerAngles, (Vector3)localEulerAxis)) <= switchAngle && !handle.Opened)
            {
                SetLimits(switchAngle, 0f);
                Opened = false;
            }
        }

        [field: SerializeField] public bool Opened { get; private set; }
    }
}
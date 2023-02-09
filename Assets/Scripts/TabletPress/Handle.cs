using System;
using Unity.Collections;
using UnityEngine;
using Utils;

namespace TabletPress
{
    public class Handle: MonoBehaviour
    {
        [SerializeField] private ConfigurableJoint joint;
        [SerializeField] private Transform handle;
        [SerializeField] private Transform locker;
        [SerializeField] private Vector3Int lockerAxis;
        [Space]
        [SerializeField] private float openSwitchAngle;
        [SerializeField] private float closeSwitchAngle;
        [SerializeField] private float upSwitchAngle;
        [SerializeField] private float downSwitchAngle;

        [Space] 
        [SerializeField] private float openLimit;
        [SerializeField] private float closeLimit;
        [SerializeField] private float upLimit;
        [SerializeField] private float downLimit;

        [Space] [SerializeField] private float openTriggerAngle;
        
        private Vector3 _prevRotation;
        private Vector3 _startHandleAxis;
        private Vector3 _startDoorAxis;

        public event Action<bool> onDoorOpenCloseStateChange;
        public bool Opened { get; private set; }
        public void SetDoor(Rigidbody door) => joint.connectedBody = door;

        private void Start()
        {
            _startHandleAxis = handle.up;
            _startDoorAxis = handle.right;
        }

        private void Update()
        {
            var rotation = CalculateRotation();

            locker.localRotation = Quaternion.Euler(rotation.x * (Vector3)lockerAxis);

            if (rotation.y > openSwitchAngle && _prevRotation.y <= openSwitchAngle)
            {
                joint.angularZLimit = new SoftJointLimit(){limit = openLimit};
            }
            if (rotation.y < closeSwitchAngle && _prevRotation.y >= closeSwitchAngle)
            {
                joint.angularZLimit = new SoftJointLimit(){limit = closeLimit};
            }

            if (AngleTranslator.GetAngle(rotation.x) > upSwitchAngle && AngleTranslator.GetAngle(_prevRotation.x) <= upSwitchAngle)
            {
                joint.highAngularXLimit = new SoftJointLimit() {limit = -upLimit};
            }
            if (AngleTranslator.GetAngle(rotation.x) < downSwitchAngle && AngleTranslator.GetAngle(_prevRotation.x) >= downSwitchAngle)
            {
                joint.highAngularXLimit = new SoftJointLimit() {limit = downLimit};
            }
            
            if (AngleTranslator.GetAngle(rotation.x) > openTriggerAngle && AngleTranslator.GetAngle(_prevRotation.x) <= openTriggerAngle)
            {
                onDoorOpenCloseStateChange?.Invoke(true);
                Opened = true;
            }
            if (AngleTranslator.GetAngle(rotation.x) < openTriggerAngle && AngleTranslator.GetAngle(_prevRotation.x) >= openTriggerAngle)
            {
                onDoorOpenCloseStateChange?.Invoke(false);
                Opened = false;
            }
            _prevRotation = rotation;
        }
        

        private Vector3 CalculateRotation()
        {
            var doorAxisOnPlane = Vector3.ProjectOnPlane(handle.right, Vector3.up);
            var upRotation = Quaternion.FromToRotation(doorAxisOnPlane, _startDoorAxis);

            var currentDoorAxis = upRotation * handle.right;
            var currentHandleAxis = upRotation * handle.up;

            return new Vector3(
                Vector3.SignedAngle(currentHandleAxis, _startHandleAxis, currentDoorAxis),
                Vector3.Angle(currentDoorAxis, _startDoorAxis));
        }
       
    }
}
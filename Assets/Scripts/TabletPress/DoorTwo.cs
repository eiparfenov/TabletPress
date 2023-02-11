using System;
using UnityEngine;
using Utils;
using Valve.VR.InteractionSystem;

namespace TabletPress
{
    public class DoorTwo: MonoBehaviour, IDoor
    {
        [SerializeField] private Interactable[] interactables;
        [SerializeField] private Handle leftHandle;
        [SerializeField] private Handle rightHandle;
        [Space]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Transform door;
        [SerializeField] private float returnDistance;

        [Space] 
        [SerializeField] private ConfigurableJoint joint;
        private InteractablesHandler _interactablesHandler;

        private void OnEnable()
        {
            _interactablesHandler = new InteractablesHandler(interactables);
            _interactablesHandler.onDetachAll += InteractablesHandlerOnDetachAll;
            leftHandle.onDoorOpenCloseStateChange += HandleOnDoorOpenCloseStateChange;
        }

        private void HandleOnDoorOpenCloseStateChange(bool obj)
        {
            if (leftHandle.Opened && rightHandle.Opened)
            {
                Lock(false);
            }
        }

        private void InteractablesHandlerOnDetachAll()
        {
            if ((targetTransform.position - door.position).magnitude < returnDistance)
            {
                Lock(true);
            }
        }

        private void Lock(bool locked)
        {
            Opened = !locked;
            var state = locked ? ConfigurableJointMotion.Locked: ConfigurableJointMotion.Free;
            joint.xMotion = state;
            joint.yMotion = state;
            joint.zMotion = state;
            joint.angularXMotion = state;
            joint.angularYMotion = state;
            joint.angularZMotion = state;
        }

        private void OnValidate()
        {
            if(leftHandle && door)
                leftHandle.SetDoor(door.GetComponent<Rigidbody>());
            
            if(rightHandle && door)
                rightHandle.SetDoor(door.GetComponent<Rigidbody>());

            if (door)
                joint = door.GetComponent<ConfigurableJoint>();
        }

        [field:SerializeField] public bool Opened { get; private set; }
    }
}
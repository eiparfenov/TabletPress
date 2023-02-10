using System;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace TabletPress.Engineering
{
    public class PartBehaviour: MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private ConfigurableJoint joint;
        public Part Part { get; set; }
        public string FullName { get; set; }

        public event Action<string, PartBehaviour> onDetachedFromHand;
        public event Action<string, PartBehaviour> onAttachedToHand;
        public void Lock(Transform target = null)
        {
            var locked = target != null;
            if(locked)
                joint.connectedAnchor = target.position;
            var state = locked ? ConfigurableJointMotion.Locked: ConfigurableJointMotion.Free;
            joint.xMotion = state;
            joint.yMotion = state;
            joint.zMotion = state;
            joint.angularXMotion = state;
            joint.angularYMotion = state;
            joint.angularZMotion = state;
        }
        private void OnEnable()
        {
            interactable.onAttachedToHand += InteractableOnAttachedToHand;
            interactable.onDetachedFromHand += InteractableOnDetachedFromHand;
        }
        private void InteractableOnDetachedFromHand(Hand hand)
        {
            onDetachedFromHand?.Invoke(FullName, this);
        }

        private void InteractableOnAttachedToHand(Hand hand)
        {
            onAttachedToHand?.Invoke(FullName, this);
        }

        private void OnDisable()
        {
            interactable.onAttachedToHand -= InteractableOnAttachedToHand;
            interactable.onDetachedFromHand -= InteractableOnDetachedFromHand;
        }
    }
}
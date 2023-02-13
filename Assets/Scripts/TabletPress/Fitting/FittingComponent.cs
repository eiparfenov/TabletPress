using System;
using TabletPress.Engineering;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace TabletPress.Fitting
{
    public class FittingComponent:MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private ConfigurableJoint joint;
        [SerializeField] private Rigidbody rb;
        [field: SerializeField]public string DetailName { get; set; }
        [field: SerializeField]public FittingPartState State { get; set; } = FittingPartState.OnTable;

        public event Action<FittingComponent> onDetachedFromHand;
        public event Action<FittingComponent> onAttachedToHand;
        public event Action<FittingComponent, FittingStartBox> onEnterStartPosition; 
        public event Action<FittingComponent, FittingStartBox> onExitStartPosition;
        private bool _interacting = true;
        public void Lock(Transform target = null)
        {
            if (!_interacting)
            {
                return;
            }
            var locked = target != null;

            if (locked)
            {
                var rotation = transform.rotation;
                joint.autoConfigureConnectedAnchor = true;
                transform.rotation = target.rotation;
                joint.autoConfigureConnectedAnchor = false;
                var rigidBody = target.GetComponentInParent<Rigidbody>();
                if (rigidBody)
                {
                    joint.connectedBody = rigidBody;
                    joint.connectedAnchor = Vector3.zero;
                }
                transform.rotation = rotation;
                if (!joint.connectedBody)
                {
                    joint.connectedAnchor = target.position;
                }
                joint.anchor = Vector3.zero;
            }
            var state = locked ? ConfigurableJointMotion.Locked: ConfigurableJointMotion.Free;
            joint.xMotion = state;
            joint.yMotion = state;
            joint.zMotion = state;
            joint.angularXMotion = state;
            joint.angularYMotion = state;
            joint.angularZMotion = state;
            if (!locked)
            {
                joint.connectedBody = null;
                transform.SetParent(null);
            }
        }

        public void SetInteractable(bool interact)
        {
            _interacting = interact;
        }

        public void SetPhysics(bool gravity)
        {
            rb.useGravity = gravity;
            rb.drag = gravity ? 2 : 5;
            rb.angularDrag = gravity ? 2 : 5;
            transform.GetChild(0).gameObject.layer =
                gravity ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("Tabletpress");
        }

        public void Construct(string detailName)
        {
            DetailName = detailName;
        }

        private void OnTriggerEnter(Collider other)
        {
            var startPosition = other.GetComponent<FittingStartBox>();
            if (startPosition)
            {
                onEnterStartPosition?.Invoke(this, startPosition);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            var startPosition = other.GetComponent<FittingStartBox>();
            if (startPosition)
            {
                onExitStartPosition?.Invoke(this, startPosition);
            }
        }

        private void OnEnable()
        {
            interactable.onAttachedToHand += InteractableOnAttachedToHand;
            interactable.onDetachedFromHand += InteractableOnDetachedFromHand;
        }
        private void InteractableOnDetachedFromHand(Hand hand)
        {
            onDetachedFromHand?.Invoke(this);
        }

        private void InteractableOnAttachedToHand(Hand hand)
        {
            onAttachedToHand?.Invoke(this);
        }

        private void OnDisable()
        {
            interactable.onAttachedToHand -= InteractableOnAttachedToHand;
            interactable.onDetachedFromHand -= InteractableOnDetachedFromHand;
        }
    }
}
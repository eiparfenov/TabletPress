using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Valve.VR.InteractionSystem;

namespace TabletPress
{
    public class ComponentBehaviour: MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private ConfigurableJoint joint;

        private Transform _spawnPoint;
        private Transform _targetPoint;
        private float _attachDistance;
        private MeshRenderer _hint;
        private string _name;
        
        public event Action<string, ComponentBehaviour> onPosSet;

        public void Construct(Transform spawnPoint, Transform targetPoint, float attachDistance, string name)
        {
            _spawnPoint = spawnPoint;
            _targetPoint = targetPoint;
            _attachDistance = attachDistance;
            _hint = targetPoint.GetComponent<MeshRenderer>();
            _name = name;
            _hint.enabled = true;
            
            Lock(true, _spawnPoint);
        }
        private void OnEnable()
        {
            print("OnEnable");
            interactable.onAttachedToHand += InteractableOnAttachedToHand;
            interactable.onDetachedFromHand += InteractableOnDetachedFromHand;
        }

        private void Lock(bool locked, Transform target = null)
        {
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

        private void InteractableOnDetachedFromHand(Hand hand)
        {
            if ((transform.position - _spawnPoint.position).magnitude < _attachDistance)
            {
                Lock(true, _spawnPoint);
            }
            if ((transform.position - _targetPoint.position).magnitude < _attachDistance)
            {
                Lock(true, _targetPoint);
                onPosSet?.Invoke(_name, this);
                _hint.enabled = false;
            }
        }

        private void InteractableOnAttachedToHand(Hand hand)
        {
            Lock(false);
        }

        private void OnDisable()
        {
            interactable.onAttachedToHand -= InteractableOnAttachedToHand;
            interactable.onDetachedFromHand -= InteractableOnDetachedFromHand;
        }
    }
}
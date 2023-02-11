using System;
using System.Linq;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;

namespace TabletPress
{
    public class Rotor: MonoBehaviour
    {
        [SerializeField] private Transform rotor;
        [SerializeField] private Transform shaft;
        [SerializeField] private float rotationFreq;
        [Space] 
        [SerializeField] private Transform[] pistons;
        [SerializeField] private Transform[] punches;
        [SerializeField] private Transform[] matrices;

        [SerializeField] private RotorSettings settings;
        [field:SerializeField] public bool rotating { get; set; }
        
       
        private void Update()
        {
            if (rotating)
            {
                shaft.localRotation *= Quaternion.Euler(Vector3.forward * rotationFreq * 360 * Time.deltaTime);

                var mainAngle = rotor.localEulerAngles.y / 360f;
                for (int i = 0; i < pistons.Length; i++)
                {
                    var angle = mainAngle;
                    var pos = pistons[i].localPosition;
                    angle += settings.offset;
                    angle -= ((float) i) / ((float)pistons.Length) - 1;
                    angle = angle > 1 ? angle - 1 : angle;
                    angle = angle > 1 ? angle - 1 : angle;
                    
                    pos.y = settings.downDistance * (1-settings.pistonMotion.Evaluate(angle));
                    pistons[i].localPosition = pos;
                }
            }
            rotor.localRotation = Quaternion.Euler(0, shaft.localEulerAngles.z, 0);
        }
    }
}
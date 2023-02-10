using System;
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

        private float _progress;
        [Button()]
        private void Start()
        {
            for (int i = 0; i < pistons.Length; i++)
            {
                if (pistons[i])
                {
                    pistons[i].rotation = Quaternion.Euler(Vector3.up * GetAngle(i, _progress));
                    var progress = _progress + ((float) i) / (float) pistons.Length + settings.offset;
                    progress = progress > 1 ? progress - 1 : progress;
                    progress = progress > 1 ? progress - 1 : progress;
                    pistons[i].position = rotor.position + Vector3.down * settings.downDistance * (1 - settings.pistonMotion.Evaluate(progress));
                }

                if (punches[i])
                {
                    punches[i].rotation = Quaternion.Euler(Vector3.up * GetAngle(i, _progress));
                }
                if (matrices[i])
                {
                    matrices[i].rotation = Quaternion.Euler(Vector3.up * GetAngle(i, _progress));
                }
            }
        }

        private void Update()
        {
            if (rotating)
            {
                rotor.localRotation *= Quaternion.Euler(Vector3.up * rotationFreq * 360 * Time.deltaTime);
                shaft.localRotation *= Quaternion.Euler(Vector3.forward * rotationFreq * 360 * Time.deltaTime);
                _progress += Time.deltaTime * rotationFreq;
                if (_progress > 1)
                {
                    _progress -= 1;
                }

                for (int i = 0; i < pistons.Length; i++)
                {
                    if (pistons[i])
                    {
                        pistons[i].rotation = Quaternion.Euler(Vector3.up * GetAngle(i, _progress));
                        var progress = _progress + ((float) i) / (float) pistons.Length + settings.offset;
                        progress = progress > 1 ? progress - 1 : progress;
                        progress = progress > 1 ? progress - 1 : progress;
                        pistons[i].position = rotor.position + Vector3.down * settings.downDistance * (1 - settings.pistonMotion.Evaluate(progress));
                    }
                    
                    if (punches[i])
                    {
                        punches[i].rotation = Quaternion.Euler(Vector3.up * GetAngle(i, _progress));
                    }
                    if (matrices[i])
                    {
                        matrices[i].rotation = Quaternion.Euler(Vector3.up * GetAngle(i, _progress));
                    }
                }
                
            }
        }
        private float GetAngle(int pos, float progress)
        {
            var angle = pos * 360f / pistons.Length;
            angle += progress * 360;
            angle %= 360;
            return angle;
        }
        
    }
}
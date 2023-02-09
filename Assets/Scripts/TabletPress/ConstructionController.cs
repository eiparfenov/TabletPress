using System;
using Unity.VisualScripting;
using UnityEngine;

namespace TabletPress
{
    public class ConstructionController: MonoBehaviour
    {
        [Serializable]
        public struct ComponentSettings
        {
            [field: SerializeField] public string name { get; private set; }
            [field: SerializeField] public ComponentBehaviour ComponentPref { get; private set; }
            [field: SerializeField] public MeshRenderer TargetPoint { get; private set; }
            [field: SerializeField] public float attachDistance { get; private set; }
        }

        [SerializeField] private ComponentSettings[] components;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Material hintMaterial;

        public event Action<string> onComponentFinished; 
        public void SpawnComponent(int componentIdx, int spawnPointIdx)
        {
            var component = components[componentIdx];
            var componentBehaviour = Instantiate(component.ComponentPref);
            componentBehaviour.Construct(spawnPoints[spawnPointIdx], 
                component.TargetPoint.transform, 
                component.attachDistance,
                component.name
                );
            componentBehaviour.onPosSet += OnComponentFinished;
        }

        private void OnComponentFinished(string obj, ComponentBehaviour beh)
        {
            beh.onPosSet -= OnComponentFinished;
            onComponentFinished?.Invoke(obj);
        }

        private void Start()
        {
            foreach (var component in components)
            {
                component.TargetPoint.enabled = false;
                var materials = new Material[component.TargetPoint.materials.Length];
                for (int i = 0; i < component.TargetPoint.materials.Length; i++)
                {
                    materials[i] = hintMaterial;
                }

                component.TargetPoint.sharedMaterials = materials;
                print("{component.TargetPoint.name} replaced");
            }
        }
    }
}
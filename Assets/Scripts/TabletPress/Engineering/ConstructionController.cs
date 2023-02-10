using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

namespace TabletPress.Engineering
{
    public class ConstructionController: MonoBehaviour
    {
        [SerializeField] private Scenario scenario;
        [Space]
        [SerializeField] private Transform[] spawnPoints;
        [Space] 
        [SerializeField] private Material hintMaterial;
        [SerializeField] private Material noHintMaterial;
        [SerializeField] private PartBehaviour partPrefab;
        [SerializeField] private Part[] parts;

        private List<Part> _hints;
        private void Start()
        {
            _hints = new();
            InitializeParts();
            ValidateScenario();
            foreach (var partName in scenario.PartsToInstall)
            {
                SetUpHintRenderer(parts.FirstOrDefault(x => x.Name == partName)?.Renderer, false);
            }
            Engineer();
        }

       
        private void SetUpHintRenderer(MeshRenderer meshRenderer, bool showHint = true)
        {
            if (showHint)
            {
                meshRenderer.enabled = true;
                var materials = new Material[meshRenderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = hintMaterial;
                }

                meshRenderer.sharedMaterials = materials;
            }
            else
            {
                meshRenderer.enabled = false;
            }
        }

        private async void PerformEngineeringEnd(PartBehaviour partBeh, Part hint)
        {
            var hintRenderer = hint.Renderer;
            var sourceRenderer = partBeh.Part.Renderer;
            SetUpHintRenderer(hintRenderer, false);
            partBeh.Lock(hint.transform);
            
            await UniTask.Delay(100);

            hintRenderer.enabled = true;
            hintRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
            partBeh.onAttachedToHand -= PartBehOnAttachedToHand;
            partBeh.onDetachedFromHand -= PartBehOnDetachedFromHand;
            Destroy(partBeh);
            _hints.Remove(hint);
        }

        private void SpawnPartToInstall(string partName, int spawnPointIdx)
        {
            var hint = parts.FirstOrDefault(x => x.Name == partName);
            
            var partBeh = Instantiate(partPrefab);
            partBeh.FullName = partName;
            partBeh.Lock(spawnPoints[spawnPointIdx]);
            partBeh.onAttachedToHand += PartBehOnAttachedToHand;
            partBeh.onDetachedFromHand += PartBehOnDetachedFromHand;
            
            var partBehMesh = Instantiate(hint);
            partBeh.Part = partBehMesh;
            partBehMesh.Renderer.enabled = true;
            partBehMesh.transform.SetParent(partBeh.transform);
            partBehMesh.transform.localScale = Vector3.one;
            partBehMesh.transform.localPosition = Vector3.zero;
            
            SetUpHintRenderer(hint.Renderer);
            
            _hints.Add(hint);
        }

        private void PartBehOnDetachedFromHand(string partName, PartBehaviour partBehaviour)
        {
            var partShortName = partName.Split(" ")[0];
            foreach (var hint in _hints.Where(x => x.Name.Split(" ")[0] == partShortName))
            {
                if ((hint.transform.position - partBehaviour.transform.position).magnitude < hint.ContactDistance)
                {
                    PerformEngineeringEnd(partBehaviour, hint);
                    break;
                }
            }
        }

        private void PartBehOnAttachedToHand(string partName, PartBehaviour partBehaviour)
        {
            partBehaviour.Lock();
        }

        private async UniTask PerformStep(Scenario.EngineeringStep step)
        {
            for (int i = 0; i < step.PartNames.Length; i++)
            {
                SpawnPartToInstall(step.PartNames[i], i);
            }

            await UniTask.WaitUntil(() => _hints.Count == 0);
        }

        private async void Engineer()
        {
            foreach (var engineeringStep in scenario.EngineeringSteps)
            {
                await PerformStep(engineeringStep);
            }
        }
        
        #region Validation
        [Button(nameof(InitializeParts))]
        private void InitializeParts()
        {
            parts = FindObjectsOfType<Part>();
            for (int i = 0; i < parts.Length; i++)
            for (int j = i + 1; j < parts.Length; j++)
            {
                if (parts[i].Name == parts[j].Name)
                {
                    Debug.LogError($"Two parts with the same name {parts[i].Name} on game objects {parts[i].name} and {parts[j].name}.");
                }
            }
        }
        
        [Button(nameof(ValidateScenario))]
        private void ValidateScenario()
        {
            if (!scenario)
            {
                Debug.LogError("No scenario added.");
            }

            foreach (var partName in scenario.PartsToInstall)
            {
                if (parts.FirstOrDefault(x => x.Name == partName) is null)
                {
                    Debug.LogError($"No part with name \"{partName}\".");
                }
            }
            scenario.ValidateScenario();
        }
        #endregion
    }
}
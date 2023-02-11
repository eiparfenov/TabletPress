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
        [SerializeField] private ScenarioPart[] scenario;
        [Space]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform capForMultipleParts;
        [SerializeField] private Transform allDoors;
        [Space] 
        [SerializeField] private Material hintMaterial;
        [SerializeField] private Material noHintMaterial;
        [SerializeField] private PartBehaviour partPrefab;
        [SerializeField] private SinglePart[] parts;
        [SerializeField] private MultiplePart[] multipleParts;
        [Space] 
        [SerializeField] private Rotor rotor;

        [Space] [SerializeField] private bool showAll;

        private List<Part> _hints;
        private MultiplePart[] _uniqueMultipleParts;
        private Dictionary<string, (int, List<Part>)> _uniquePartsMeshes;
        private IDoor[] _doors;
        private async void Start()
        {
            _hints = new();
            InitializeDoors();
            print($"Doors: {_doors.Length}");
            InitializeParts();
            ValidateScenario();
            foreach (var scenarioPart in scenario)
            {
                if (scenarioPart is EngineeringScenario engineeringScenario)
                {
                    foreach (var partName in engineeringScenario.PartsToInstall)
                    {
                        parts.FirstOrDefault(x => x.Name == partName).Renderer.enabled = false;
                    }
                }
            }

            foreach (var multiplePart in multipleParts)
            {
                multiplePart.Renderer.enabled = false;
            }

            foreach (var scenarioPart in scenario)
            {
                if (scenarioPart is EngineeringScenario engineeringScenario)
                {
                    await Engineer(engineeringScenario);
                }

                if (scenarioPart is AddPistonsScenario addPistonsScenario)
                {
                    await AddPistons(addPistonsScenario);
                }
            }
            
            await UniTask.WaitWhile(() => _doors.Any(x => x.Opened));
            
            if(showAll)
            {
                foreach (var multiplePart in multipleParts)
                {
                    multiplePart.Renderer.enabled = true;
                }
                
            }
            rotor.rotating = true;
        }

        #region Pistons
        private async UniTask AddPistons(AddPistonsScenario addPistonsScenario)
        {
            _uniquePartsMeshes = new Dictionary<string, (int, List<Part>)>();
            for (int i = 0; i < _uniqueMultipleParts.Length; i++)
            {
                var uniqueMultiplePart = _uniqueMultipleParts[i];
                var newParts = new List<Part>();
                for (int j = 0; j < addPistonsScenario.pistonsToAdd.Length; j++)
                {
                    var newPart = Instantiate(uniqueMultiplePart);
                    newPart.gameObject.SetActive(false);
                    newParts.Add(newPart);
                }
                _uniquePartsMeshes[uniqueMultiplePart.Name.Split(" ")[0]] = (i, newParts);
            }
            foreach (var partName in _uniqueMultipleParts.SelectMany(i => addPistonsScenario.pistonsToAdd.Select(j => $"{i.Name.Split()[0]} {j}"))) 
            {
                var hint = multipleParts.FirstOrDefault(x => x.Name == partName);
                SetUpHintRenderer(hint.Renderer);
                _hints.Add(hint);
            }

            foreach (var uniquePartsMesh in _uniquePartsMeshes)
            {
                SpawnMultiplePartToInstall(uniquePartsMesh.Key);
            }
            
            await UniTask.WaitUntil(() => _hints.Count == 0);

            _uniquePartsMeshes = null;
        }
        private void SpawnMultiplePartToInstall(string partName)
        {
            if (_uniqueMultipleParts is null)
            {
                return;
            }
            var shortPartName = partName.Split(" ")[0];
            var partState = _uniquePartsMeshes[shortPartName];
            
            if (partState.Item2.Count == 0)
            {
                return;
            }
            
            var partBeh = Instantiate(partPrefab);
            partBeh.FullName = partName;
            partBeh.Lock(spawnPoints[partState.Item1]);
            partBeh.onAttachedToHand += PartBehOnAttachedToHandMultiple;
            partBeh.onDetachedFromHand += PartBehOnDetachedFromHandMultiple;

            var partBehMesh = partState.Item2.Last();
            partState.Item2.RemoveAt(partState.Item2.Count - 1);

            partBeh.Part = partBehMesh;
            partBehMesh.gameObject.SetActive(true);
            partBehMesh.Renderer.enabled = true;
            partBehMesh.transform.SetParent(partBeh.transform);
            partBehMesh.transform.localScale = Vector3.one;
            partBehMesh.transform.localPosition = Vector3.zero;
        }
        private void PartBehOnDetachedFromHandMultiple(string partName, PartBehaviour partBeh)
        {
            var partShortName = partName.Split(" ")[0];
            var hints = _hints.Where(
                    x => x.Name.Split(" ")[0] == partShortName &&
                         (x.transform.position - partBeh.transform.position).magnitude < x.ContactDistance)
                .OrderBy(x => (x.transform.position - partBeh.transform.position).magnitude);

            var hint = hints.Count() != 0 ? hints.First() : null; 
            
            if (hint != null && Vector3.ProjectOnPlane(hint.transform.position - capForMultipleParts.transform.position, Vector3.up).magnitude < .03f)
            {
                PerformEngineeringEndMultiplePart(partBeh, hint);
            }
            SpawnMultiplePartToInstall(partName);
        }
        private void PartBehOnAttachedToHandMultiple(string partName, PartBehaviour partBeh)
        {
            partBeh.Lock();
        }
        private async void PerformEngineeringEndMultiplePart(PartBehaviour partBeh, Part hint)
        {
            var hintRenderer = hint.Renderer;
            var sourceRenderer = partBeh.Part.Renderer;
            SetUpHintRenderer(hintRenderer, false);
            partBeh.Lock(hint.transform);
            
            await UniTask.Delay(100);

            hintRenderer.enabled = true;
            hintRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
            partBeh.onAttachedToHand -= PartBehOnAttachedToHandMultiple;
            partBeh.onDetachedFromHand -= PartBehOnDetachedFromHandMultiple;
            Destroy(partBeh.gameObject);
            _hints.Remove(hint);
        }
        #endregion
        #region Engineer
        private async UniTask Engineer(EngineeringScenario engineeringScenario)
        {
            foreach (var step in engineeringScenario.EngineeringSteps)
            {
                for (int i = 0; i < step.PartNames.Length; i++)
                {
                    SpawnPartToInstallSinglePart(step.PartNames[i], i);
                }

                await UniTask.WaitUntil(() => _hints.Count == 0);
            }
        }
        private async void PerformEngineeringEndSinglePart(PartBehaviour partBeh, Part hint)
        {
            var hintRenderer = hint.Renderer;
            var sourceRenderer = partBeh.Part.Renderer;
            SetUpHintRenderer(hintRenderer, false);
            partBeh.Lock(hint.transform);
            
            await UniTask.Delay(100);

            hintRenderer.enabled = true;
            hintRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
            partBeh.onAttachedToHand -= PartBehOnAttachedToHandSinglePart;
            partBeh.onDetachedFromHand -= PartBehOnDetachedFromHandSinglePart;
            Destroy(partBeh.gameObject);
            _hints.Remove(hint);
        }

        private void SpawnPartToInstallSinglePart(string partName, int spawnPointIdx)
        {
            var hint = parts.FirstOrDefault(x => x.Name == partName);
            
            var partBeh = Instantiate(partPrefab);
            partBeh.FullName = partName;
            partBeh.Lock(spawnPoints[spawnPointIdx]);
            partBeh.onAttachedToHand += PartBehOnAttachedToHandSinglePart;
            partBeh.onDetachedFromHand += PartBehOnDetachedFromHandSinglePart;
            
            var partBehMesh = Instantiate(hint);
            partBeh.Part = partBehMesh;
            partBehMesh.Renderer.enabled = true;
            partBehMesh.transform.SetParent(partBeh.transform);
            partBehMesh.transform.localScale = Vector3.one;
            partBehMesh.transform.localPosition = Vector3.zero;
            
            SetUpHintRenderer(hint.Renderer);
            
            _hints.Add(hint);
        }

        private void PartBehOnDetachedFromHandSinglePart(string partName, PartBehaviour partBehaviour)
        {
            var partShortName = partName.Split(" ")[0];
            foreach (var hint in _hints.Where(x => x.Name.Split(" ")[0] == partShortName))
            {
                if ((hint.transform.position - partBehaviour.transform.position).magnitude < hint.ContactDistance)
                {
                    PerformEngineeringEndSinglePart(partBehaviour, hint);
                    break;
                }
            }
        }

        private void PartBehOnAttachedToHandSinglePart(string partName, PartBehaviour partBehaviour)
        {
            partBehaviour.Lock();
        }
        #endregion
        #region Validation
        [Button(nameof(InitializeParts))]
        private void InitializeParts()
        {
            parts = FindObjectsOfType<SinglePart>();
            for (int i = 0; i < parts.Length; i++)
            for (int j = i + 1; j < parts.Length; j++)
            {
                if (parts[i].Name == parts[j].Name)
                {
                    Debug.LogError($"Two parts with the same name {parts[i].Name} on game objects {parts[i].name} and {parts[j].name}.");
                }
            }
            multipleParts = FindObjectsOfType<MultiplePart>();
            _uniqueMultipleParts = multipleParts.Distinct(new MultiplePartsDistinctComparer()).ToArray();
        }
        
        [Button(nameof(ValidateScenario))]
        private void ValidateScenario()
        {
            foreach (var scenarioPart in scenario)
            {
                if (scenarioPart is EngineeringScenario engineeringScenario)
                {
                    foreach (var partName in engineeringScenario.PartsToInstall)
                    {
                        if (parts.All(x => x.Name != partName))
                        {
                            Debug.LogError($"No part \"{partName}\".");
                        }
                    }
                }
            }
        }

        private void InitializeDoors()
        {
            _doors = allDoors.GetComponentsInChildren<IDoor>();
        }
        #endregion
        # region Utils
        private class MultiplePartsDistinctComparer : IEqualityComparer<MultiplePart>
        {
            public bool Equals(MultiplePart x, MultiplePart y)
            {
                return x.Name.Split(" ")[0] == y.Name.Split(" ")[0];
            }

            public int GetHashCode(MultiplePart obj)
            {
                return obj.Name.Split(" ")[0].GetHashCode();
            }
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
        
        #endregion
    }
    
}
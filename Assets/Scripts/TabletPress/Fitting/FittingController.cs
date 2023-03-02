using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TabletPress.Engineering;
using Unity.VisualScripting;
using UnityEngine;

namespace TabletPress.Fitting
{
    public class FittingController: MonoBehaviour
    {
        [SerializeField] private FitScenario[] scenario;
        [Space] 
        [SerializeField] private Transform capPosition;
        [SerializeField] private string[] detailsOnCap;
        [Space]
        [SerializeField] private FittingHint[] hints;
        [SerializeField] private FittingComponent[] components;
        [SerializeField] private FittingStartBox[] startPositions;

        [Space] 
        [SerializeField] private Material hintMaterial;

        [Space] [SerializeField] private UsageController usage;

        private List<FittingHint> _activeHints;
        private Dictionary<string, FittingComponent> _installedFittingComponents;

        private async void Start()
        {
            
            InitializeHints();
            InitializeComponents();
            _activeHints = new();
            _installedFittingComponents = new();
            await UniTask.Yield(PlayerLoopTiming.Update);
            
            foreach (var component in components)
            {
                component.onAttachedToHand += FittingComponentOnAttachToHand;
                component.onDetachedFromHand += FittingComponentOnDetachFromHand;
                component.onEnterStartPosition += FittingComponentOnEnterStartPosition;
                component.onExitStartPosition += FittingComponentOnExitStartPosition;
            }

            RecolorHints();
            foreach (var scenarioPart in scenario)
            {
                if (scenarioPart.Use)
                {
                    await usage.Run();
                }
                else if (scenarioPart.Fit)
                {
                    await Fit(scenarioPart);
                }
                else
                {
                    await UnFit(scenarioPart);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var component in components)
            {
                component.onAttachedToHand -= FittingComponentOnAttachToHand;
                component.onDetachedFromHand -= FittingComponentOnDetachFromHand;
                component.onEnterStartPosition -= FittingComponentOnEnterStartPosition;
                component.onExitStartPosition -= FittingComponentOnExitStartPosition;
            }
        }

    #region BuildStage

        private async UniTask Fit(FitScenario fitScenario)
        {
            foreach (var fittingStep in fitScenario.FittingSteps)
            {
                foreach (var hint in hints.Where(x => fittingStep.PartNames.Contains(x.FullName)))
                {
                    ActivateHint(hint);
                    startPositions.FirstOrDefault(x => x.DetailName == hint.DetailName)?.Take();
                }
                await UniTask.WaitUntil(() => _activeHints.Count == 0);
            }
        }

        private async UniTask UnFit(FitScenario fitScenario)
        {
            foreach (var fittingStep in fitScenario.FittingSteps)
            {
                var currentComponents = fittingStep.PartNames.Select(x => _installedFittingComponents[x]);
                foreach (var component in currentComponents)
                {
                    component.SetInteractable(true);
                    startPositions.FirstOrDefault(x => x.DetailName == component.DetailName)?.Drop();
                }

                await UniTask.WaitUntil(() => currentComponents.All(x => x.State == FittingPartState.OnTable));
            }
        }
        

    #endregion
    #region ComponentHandlers
        private async void FittingComponentOnDetachFromHand(FittingComponent component)
        {
            var possibleHints = _activeHints.Where(x => x.DetailName == component.DetailName &&
                                                        (x.transform.position - component.transform.position).magnitude < x.ContactDistance)
                .OrderBy(x => (x.transform.position - component.transform.position).magnitude);
            if (possibleHints.Any() &&
                (!detailsOnCap.Contains(component.DetailName) || Vector3.ProjectOnPlane(component.transform.position - capPosition.transform.position, Vector3.up).magnitude < .03f))
            {
                var hint = possibleHints.First();
                if (hint.ParentUnderSelf)
                {
                    component.transform.SetParent(hint.transform);
                }
                component.Lock(hint.transform);
                component.State = FittingPartState.OnPosition;
                DisactivateHint(hint);
                _installedFittingComponents[hint.FullName] = component;
                component.SetInteractable(false);
            }
        }
        private void FittingComponentOnAttachToHand(FittingComponent component)
        {
            if (component.State == FittingPartState.OnPosition)
            {
                if (!detailsOnCap.Contains(component.DetailName) || Vector3
                        .ProjectOnPlane(component.transform.position - capPosition.transform.position, Vector3.up)
                        .magnitude < .03f)
                {
                    component.Lock();
                }
            }

            if (component.Interacting)
            {
                component.State = FittingPartState.InSpace;
            }
        }

        private void FittingComponentOnExitStartPosition(FittingComponent component, FittingStartBox startBox)
        {
            if (component.DetailName == startBox.DetailName)
            {
                startBox.Take(-1);
            }
            component.State = FittingPartState.InSpace;
            component.SetPhysics(false);
            component.gameObject.layer = LayerMask.NameToLayer("Tabletpress");
        }
        private void FittingComponentOnEnterStartPosition(FittingComponent component, FittingStartBox startBox)
        {
            if (startBox.DetailName == component.DetailName)
            {
                startBox.Drop(-1);
            }
            if (startBox.DetailName == component.DetailName)
            {
                component.State = FittingPartState.OnTable;
            }
            component.SetPhysics(true);
            component.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    #endregion
    #region HintsDisplay
        private void ActivateHint(FittingHint hint)
        {
            _activeHints.Add(hint);
            hint.Renderer.enabled = true;
        }

        private void DisactivateHint(FittingHint hint)
        {
            _activeHints.Remove(hint);
            hint.Renderer.enabled = false;
        }

        private void RecolorHints()
        {
            foreach (var hint in hints)
            {
                var materials = new Material[hint.Renderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = hintMaterial;
                }
                hint.Renderer.sharedMaterials = materials;
                hint.Renderer.enabled = false;
            }
        }
        #endregion
    #region Initialize
        [Button()]
        private void InitializeHints()
        {
            hints = FindObjectsOfType<FittingHint>();
            for (int i = 0; i < hints.Length; i++)
            for (int j = i + 1; j < hints.Length; j++)
            {
                if (hints[i].FullName == hints[j].FullName)
                {
                    Debug.LogError($"Two parts with the same name {hints[i].FullName} on game objects {hints[i].name} and {hints[j].name}.");
                }
            }
        }
        [Button()]
        private void InitializeComponents()
        {
            var hintNames = hints.Select(x => x.DetailName).ToList();
            components = FindObjectsOfType<FittingComponent>();
            foreach (var component in components)
            {
                if (!hintNames.Contains(component.DetailName))
                {
                    Debug.unityLogger.LogWarning(nameof(FittingController), $"Component {component.DetailName} can not be placed");
                }
            }

            foreach (var component in components)
            {
                if (hintNames.Contains(component.DetailName))
                {
                    hintNames.Remove(component.DetailName);
                }
            }

            if (hintNames.Count > 0)
            {
                var unresolvedHints = string.Join(", ", hintNames.Select(x => $"\"{x}\""));
                Debug.unityLogger.LogWarning(nameof(FittingController),
                    $"Can not resolve components for {unresolvedHints}");
            }
        }
        [Button()]
        private void InitializeStartPoints()
        {
            startPositions = FindObjectsOfType<FittingStartBox>();
            var startPositionsNames = startPositions.Select(x => x.DetailName);
            var hintsNames = hints.Select(x => x.DetailName).Distinct();
            var noHint = startPositionsNames.Except(hintsNames);
            var noStartPosition = hintsNames.Except(startPositionsNames);
            if (noHint.Any())
            {
                Debug.unityLogger.LogWarning(nameof(FittingController),
                    $"There are start points but no hints for {string.Join(", ", noHint.Select(x => $"\"{x}\""))}.");
            }

            if (noStartPosition.Any())
            {
                Debug.unityLogger.LogWarning(nameof(FittingController),
                    $"There are hints but no start points for {string.Join(", ", noHint.Select(x => $"\"{x}\""))}.");
            }
        }
        #endregion
    }
}
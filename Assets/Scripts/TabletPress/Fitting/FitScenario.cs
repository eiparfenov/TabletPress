using System;
using System.Linq;
using TabletPress.Engineering;
using UnityEngine;

namespace TabletPress.Fitting
{
    [CreateAssetMenu(fileName = "FitScenario", menuName = "TabletPress/FitScenario", order = 0)]
    public class FitScenario : ScenarioPart
    {
        [Serializable]
        public struct FittingStep
        {
            [field: SerializeField] public string[] PartNames { get; private set; }
        }
        [field: SerializeField] public FittingStep[] FittingSteps { get; private set; }
        [field: SerializeField] public bool Fit { get; private set; }
        [field: SerializeField] public bool Use { get; private set; }
    }
}
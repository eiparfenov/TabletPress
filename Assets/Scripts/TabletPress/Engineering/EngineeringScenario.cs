using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace TabletPress.Engineering
{
    [CreateAssetMenu(fileName = "FitScenario", menuName = "TabletPress/FitScenario", order = 0)]
    public class EngineeringScenario : ScenarioPart
    {
        [Serializable]
        public struct EngineeringStep
        {
            [field: SerializeField] public string[] PartNames { get; private set; }
        }

        public string[] PartsToInstall => EngineeringSteps.SelectMany(x => x.PartNames).ToArray();
        [field: SerializeField] public EngineeringStep[] EngineeringSteps { get; private set; }
        
    }
}
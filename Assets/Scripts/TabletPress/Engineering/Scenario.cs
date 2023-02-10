using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace TabletPress.Engineering
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "TabletPress/Scenario", order = 0)]
    public class Scenario : ScriptableObject
    {
        [Serializable]
        public struct EngineeringStep
        {
            [field: SerializeField] public string[] PartNames { get; private set; }
        }
        [field: SerializeField] public string[] PartsToInstall { get; private set; }
        [field: SerializeField] public EngineeringStep[] EngineeringSteps { get; private set; }
        
        public void ValidateScenario()
        {
            var allParts = new HashSet<string>(PartsToInstall);
            foreach (var engineeringStep in EngineeringSteps)
            {
                foreach (string partName in engineeringStep.PartNames)
                {
                    if (!allParts.Contains(partName))
                    {
                        Debug.LogError($"Scenario validation failed! Part \"{partName}\" is in engineering steps," +
                                  $" but does not exist in parts to install or appears twice.");
                    }

                    allParts.Remove(partName);
                }
            }

            if (allParts.Count != 0)
            {
                var parts = string.Join(", ", allParts.Select(x => $"\"{x}\""));
                Debug.LogWarning($"Scenario validation warning! Parts {parts} was not installed in the ed of scenario.");
            }
        }
    }
}
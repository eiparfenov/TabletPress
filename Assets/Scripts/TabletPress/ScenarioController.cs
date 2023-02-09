using System;
using UnityEngine;

namespace TabletPress
{
    public class ScenarioController: MonoBehaviour
    {
        [SerializeField] private ConstructionController constructionController;
        [SerializeField] private int componentIdx;
        [SerializeField] private int spawnPointIdx;

        [SerializeField] private bool tempButton;

        private void Start()
        {
            constructionController.onComponentFinished += ConstructionControllerOnComponentFinished;
        }

        private void ConstructionControllerOnComponentFinished(string obj)
        {
            Debug.unityLogger.Log(nameof(ScenarioController), obj);
            componentIdx++;
            constructionController.SpawnComponent(componentIdx, spawnPointIdx);
        }

        private void Update()
        {
            if (tempButton)
            {
                tempButton = false;
                constructionController.SpawnComponent(componentIdx, spawnPointIdx);
            }
        }

        private void OnDestroy()
        {
            constructionController.onComponentFinished -= ConstructionControllerOnComponentFinished;
        }
    }
}
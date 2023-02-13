using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace TabletPress.Fitting
{
    public class StartPointsCreator: MonoBehaviour
    {
        [SerializeField] private FittingStartBox startBoxPref;
        [SerializeField] private FittingComponent fittingComponentPref;
        [SerializeField] private Vector3 offset;
        [Button()]
        private void CreateStartPoints()
        {
            var createdStartBoxes = GetComponentsInChildren<FittingStartBox>().Select(x => x.DetailName);
            var hints = FindObjectsOfType<FittingHint>();
            var hintsToCreate = hints.Select(x => x.DetailName).Distinct().Except(createdStartBoxes).ToArray();

            for (int i = 0; i < hintsToCreate.Length; i++)
            {
                var hintDetailName = hintsToCreate[i];
                var hint = hints.FirstOrDefault(x => x.DetailName == hintDetailName);
                var startBox = Instantiate(startBoxPref, transform);
                startBox.Construct(hint, fittingComponentPref);
                startBox.gameObject.name = hint.DetailName;
                startBox.transform.localPosition = i * offset;
            }
        }
    }
}
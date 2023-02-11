using UnityEngine;

namespace TabletPress.Engineering
{
    [CreateAssetMenu(fileName = "AddPistonsScenario", menuName = "TabletPress/AddPistonsScenario", order = 0)]
    public class AddPistonsScenario : ScenarioPart
    {
        [field: SerializeField] public int[] pistonsToAdd { get; private set; }
    }
}
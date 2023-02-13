using UnityEngine;

namespace TabletPress.Fitting
{
    public class FittingHint: MonoBehaviour
    {
        [field: SerializeField] public string FullName { get; private set; }
        [field: SerializeField] public float ContactDistance { get; private set; } = .1f;
        [field:SerializeField] public MeshRenderer Renderer { get; private set; }
        [field: SerializeField] public bool ParentUnderSelf;
        public string DetailName => FullName.Split()[0];
        private void Reset()
        {
            Renderer = GetComponent<MeshRenderer>();
            var nameToAdd = name.Split('_')[1];
            FullName = nameToAdd.Substring(0, 1).ToUpper() + nameToAdd.Substring(1);
            var meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
        }
    }
}
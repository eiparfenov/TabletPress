using UnityEngine;

namespace TabletPress.Engineering
{
    public class Part: MonoBehaviour
    {
        [field:SerializeField] public string Name { get; private set; }
        [field: SerializeField] public float ContactDistance { get; private set; } = .1f;
        [field:SerializeField] public MeshRenderer Renderer { get; private set; }

        private void Reset()
        {
            Renderer = GetComponent<MeshRenderer>();
            var nameToAdd = name.Split('_')[1];
            Name = nameToAdd.Substring(0, 1).ToUpper() + nameToAdd.Substring(1);
            var meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
        }
    }
}
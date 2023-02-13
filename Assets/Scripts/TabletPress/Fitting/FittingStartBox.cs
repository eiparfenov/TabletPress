using System;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabletPress.Fitting
{
    public class FittingStartBox: MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI differenceText;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private Image take;
        [SerializeField] private Image drop;
        [field: SerializeField] public string DetailName { 
            get => _detailName;
            private set
            {
                _detailName = value;
                detailNameText.text = _detailName;
            }
        }
        [SerializeField] public FittingHint relatedHint;
        [SerializeField] public FittingComponent componentPrefab;

        private int _difference;
        [SerializeField] private string _detailName;
        [Button()]
        public void Take(int amount = 1)
        {
            Difference -= amount;
        }
        [Button()]
        public void Drop(int amount = 1)
        {
            Difference += amount;
        }

        public void Construct(FittingHint relatedHint, FittingComponent componentPrefab)
        {
            DetailName = relatedHint.DetailName;
            this.relatedHint = relatedHint;
            this.componentPrefab = componentPrefab;
        }
        

        private int Difference
        {
            get => _difference;
            set
            {
                _difference = value;

                take.enabled = _difference < 0;
                drop.enabled = _difference > 0;

                differenceText.text = _difference == 0 ? "" : Math.Abs(_difference).ToString();
            }
        }
        [Button()]
        private void CreateComponent()
        {
            var component = Instantiate(componentPrefab, transform);
            var hint = Instantiate(relatedHint, component.transform);
            hint.transform.localPosition = Vector3.zero;
            hint.transform.localScale = Vector3.one;
            hint.transform.localRotation = Quaternion.identity;
            component.Construct(hint.DetailName);
            Destroy(hint);
            component.SetPhysics(true);
        }
    }
}